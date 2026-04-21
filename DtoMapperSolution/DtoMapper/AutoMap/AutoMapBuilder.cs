using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DtoMapper.Converters;
using DtoMapper.Core;
using DtoMapper.Naming;

namespace DtoMapper.AutoMap
{
    internal sealed class AutoMapBuilder
    {
        private readonly ConverterRegistry _converters;
        private readonly MapperConfiguration _config;
        private readonly INamingConvention _naming;

        public AutoMapBuilder(
            INamingConvention naming,
            ConverterRegistry converters,
            MapperConfiguration config)
        {
            _naming = naming;
            _converters = converters;
            _config = config;
        }

        // =============================================================
        // ENTRY
        // =============================================================
        public LambdaExpression BuildLambda(Type srcType, Type destType)
        {
            var src = Expression.Parameter(srcType, "src");

            // ---------- LIST ----------
            if (IsList(srcType) && IsList(destType))
            {
                var se = srcType.GetGenericArguments()[0];
                var de = destType.GetGenericArguments()[0];

                // enum-in-list unsupported unless element map exists
                if (!HasElementMapping(se, de))
                {
                    return MakeLambda(
                        srcType,
                        destType,
                        Expression.Constant(null, destType),
                        src);
                }

                return BuildList(srcType, destType, src);
            }

            // ---------- ARRAY ----------
            if (srcType.IsArray && destType.IsArray)
            {
                return BuildArray(srcType, destType, src);
            }

            // ---------- LEAF ----------
            if (IsLeaf(destType))
            {
                return MakeLambda(
                    srcType,
                    destType,
                    BuildLeaf(src, srcType, destType),
                    src);
            }

            // ---------- OBJECT ----------
            return BuildObject(srcType, destType, src);
        }

        // =============================================================
        // LIST
        // =============================================================
        private LambdaExpression BuildList(
            Type srcType,
            Type destType,
            ParameterExpression src)
        {
            var srcElem = srcType.GetGenericArguments()[0];
            var destElem = destType.GetGenericArguments()[0];

            var listVar = Expression.Variable(destType, "list");
            var idxVar = Expression.Variable(typeof(int), "i");

            var ctor = destType.GetConstructor(Type.EmptyTypes)!;
            var add = destType.GetMethod("Add")!;
            var count = Expression.Property(src, "Count");
            var indexer = srcType.GetProperty("Item")!.GetMethod!;
            var srcItem = Expression.Call(src, indexer, idxVar);

            var mapped = BuildElement(srcItem, srcElem, destElem);
            var breakLabel = Expression.Label();

            var loop = Expression.Loop(
                Expression.IfThenElse(
                    Expression.LessThan(idxVar, count),
                    Expression.Block(
                        Expression.Call(listVar, add, mapped),
                        Expression.PostIncrementAssign(idxVar)),
                    Expression.Break(breakLabel)),
                breakLabel);

            return MakeLambda(
                srcType,
                destType,
                Expression.Block(
                    new[] { listVar, idxVar },
                    Expression.Assign(listVar, Expression.New(ctor)),
                    Expression.Assign(idxVar, Expression.Constant(0)),
                    loop,
                    listVar),
                src);
        }

        // =============================================================
        // ARRAY
        // =============================================================
        private LambdaExpression BuildArray(
            Type srcType,
            Type destType,
            ParameterExpression src)
        {
            var srcElem = srcType.GetElementType()!;
            var destElem = destType.GetElementType()!;

            var lenVar = Expression.Variable(typeof(int), "len");
            var arrVar = Expression.Variable(destType, "arr");
            var idxVar = Expression.Variable(typeof(int), "i");

            var srcItem = Expression.ArrayIndex(src, idxVar);
            var mapped = BuildElement(srcItem, srcElem, destElem);

            var breakLabel = Expression.Label();

            var loop = Expression.Loop(
                Expression.IfThenElse(
                    Expression.LessThan(idxVar, lenVar),
                    Expression.Block(
                        Expression.Assign(
                            Expression.ArrayAccess(arrVar, idxVar),
                            mapped),
                        Expression.PostIncrementAssign(idxVar)),
                    Expression.Break(breakLabel)),
                breakLabel);

            return MakeLambda(
                srcType,
                destType,
                Expression.Block(
                    new[] { lenVar, arrVar, idxVar },
                    Expression.Assign(lenVar, Expression.ArrayLength(src)),
                    Expression.Assign(arrVar, Expression.NewArrayBounds(destElem, lenVar)),
                    Expression.Assign(idxVar, Expression.Constant(0)),
                    loop,
                    arrVar),
                src);
        }

        // =============================================================
        // ELEMENT
        // =============================================================
        private Expression BuildElement(
            Expression srcItem,
            Type srcElem,
            Type destElem)
        {
            Expression invoke;

            if (_converters.TryResolve(srcElem, destElem, out var conv))
            {
                invoke = Expression.Invoke(Expression.Constant(conv), srcItem);
            }
            else
            {
                var map = _config.GetTypeMap(srcElem, destElem);
                invoke = Expression.Invoke(
                    Expression.Constant(map.CompiledDelegate!),
                    srcItem);
            }

            return srcElem.IsValueType
                ? invoke
                : Expression.Condition(
                    Expression.Equal(srcItem, Expression.Constant(null, srcElem)),
                    Expression.Default(destElem),
                    invoke);
        }

        // =============================================================
        // OBJECT
        // =============================================================
        private LambdaExpression BuildObject(
            Type srcType,
            Type destType,
            ParameterExpression src)
        {
            var srcProps = srcType.GetProperties();
            var destProps = destType.GetProperties();
            var typeMap = _config.GetTypeMap(srcType, destType);

            foreach (var ctor in destType.GetConstructors())
            {
                var parms = ctor.GetParameters();
                var args = new Expression[parms.Length];
                bool usable = true;

                for (int i = 0; i < parms.Length; i++)
                {
                    var sp = Array.Find(
                        srcProps,
                        p => string.Equals(
                            p.Name,
                            parms[i].Name,
                            StringComparison.OrdinalIgnoreCase));

                    if (sp == null)
                    {
                        usable = false;
                        break;
                    }

                    args[i] = BuildValue(src, sp, parms[i].ParameterType);
                }

                if (!usable) continue;

                var newExpr = Expression.New(ctor, args);
                var bindings = new List<MemberBinding>();
                var customMapped = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                // ---------- CUSTOM MEMBER MAPS ----------
                foreach (var custom in typeMap.CustomMemberMaps)
                {
                    var destProp = destType.GetProperty(custom.DestinationName)!;

                    // ✅ IGNORE SUPPORT
                    if (custom.IsIgnored)
                    {
                        customMapped.Add(destProp.Name);
                        continue;
                    }

                    var rewritten =
                        new ParameterReplacer(
                            custom.SourceExpression!.Parameters[0],
                            src)
                        .Visit(custom.SourceExpression!.Body)!;

                    Expression boundValue;

                    // ✅ COLLECTION REBIND (FIXED)
                    if (IsArrayOrList(destProp.PropertyType)
                        && IsArrayOrList(rewritten.Type)
                        && rewritten.Type != destProp.PropertyType)
                    {
                        var collectionMap =
                            BuildLambda(rewritten.Type, destProp.PropertyType);

                        boundValue = Expression.Invoke(collectionMap, rewritten);
                    }
                    else
                    {
                        boundValue = rewritten;
                    }

                    bindings.Add(Expression.Bind(destProp, boundValue));
                    customMapped.Add(destProp.Name);
                }

                // ---------- DEFAULT AUTOMAPPING ----------
                foreach (var dp in destProps)
                {
                    if (!dp.CanWrite) continue;
                    if (customMapped.Contains(dp.Name)) continue;

                    var sp = Array.Find(
                        srcProps,
                        s => string.Equals(
                            s.Name,
                            dp.Name,
                            StringComparison.OrdinalIgnoreCase));

                    if (sp == null) continue;

                    bindings.Add(
                        Expression.Bind(
                            dp,
                            BuildValue(src, sp, dp.PropertyType)));
                }

                return MakeLambda(
                    srcType,
                    destType,
                    bindings.Count > 0
                        ? Expression.MemberInit(newExpr, bindings)
                        : (Expression)newExpr,
                    src);
            }

            throw new ExpressionBuildException(
                srcType,
                destType,
                "Destination type does not have a usable constructor.");
        }

        // =============================================================
        // VALUE / LEAF
        // =============================================================
        private Expression BuildValue(
            ParameterExpression src,
            PropertyInfo sp,
            Type destType)
        {
            var srcExpr = Expression.Property(src, sp);
            var srcType = sp.PropertyType;

            if (_converters.TryResolve(srcType, destType, out var conv))
                return Expression.Invoke(Expression.Constant(conv), srcExpr);

            if (destType.IsAssignableFrom(srcType))
                return srcExpr;

            if (IsArrayOrList(srcType) && IsArrayOrList(destType))
                return Expression.Invoke(
                    BuildLambda(srcType, destType),
                    srcExpr);

            if (_config.HasTypeMap(srcType, destType))
            {
                var map = _config.GetTypeMap(srcType, destType);
                return Expression.Invoke(
                    Expression.Constant(map.CompiledDelegate!),
                    srcExpr);
            }

            return Expression.Default(destType);
        }

        private Expression BuildLeaf(
            ParameterExpression src,
            Type srcType,
            Type destType)
        {
            if (_converters.TryResolve(srcType, destType, out var conv))
                return Expression.Invoke(Expression.Constant(conv), src);

            if (destType.IsAssignableFrom(srcType))
                return src;

            return Expression.Default(destType);
        }

        // =============================================================
        // HELPERS
        // =============================================================
        private bool HasElementMapping(Type se, Type de) =>
            _config.HasTypeMap(se, de) ||
            _converters.TryResolve(se, de, out _);

        private static LambdaExpression MakeLambda(
            Type src,
            Type dest,
            Expression body,
            ParameterExpression param) =>
            Expression.Lambda(
                typeof(Func<,>).MakeGenericType(src, dest),
                body,
                param);

        private static bool IsList(Type t) =>
            t.IsGenericType &&
            t.GetGenericTypeDefinition() == typeof(List<>);

        private static bool IsArrayOrList(Type t) =>
            t.IsArray || IsList(t);

        private static bool IsLeaf(Type t) =>
            t.IsPrimitive ||
            t.IsEnum ||
            t == typeof(string) ||
            t == typeof(decimal);
    }
}