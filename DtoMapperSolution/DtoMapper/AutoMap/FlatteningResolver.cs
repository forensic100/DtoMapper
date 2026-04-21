using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DtoMapper.Naming;

namespace DtoMapper.AutoMap
{
    internal static class FlatteningResolver
    {
        /// <summary>
        /// Attempts to resolve a forward-flattening chain
        /// such as "Address.City" → "AddressCity", "Address_City", or any naming‑normalized version.
        /// 
        /// Returns an expression OR null if no flattening chain matches.
        /// </summary>
        public static Expression? TryResolveFlattening(
            INamingConvention naming,
            ParameterExpression srcParam,
            Type srcType,
            Type expectedDestType,
            string destName)
        {
            if (string.IsNullOrWhiteSpace(destName))
                return null;

            // Normalize the destination member name for comparison
            string normalizedDest = naming.Normalize(destName);

            // Explore all possible property paths under the source type
            foreach (var path in EnumerateSourcePaths(srcType))
            {
                // Skip indexers or empty paths
                if (path.Count == 0)
                    continue;

                // Must end in a property that can convert to expectedDestType
                PropertyInfo leaf = path[path.Count - 1];

                if (leaf.GetIndexParameters().Length > 0)
                    continue;

                if (!expectedDestType.IsAssignableFrom(leaf.PropertyType)
                    && !CanConvertValue(leaf.PropertyType, expectedDestType))
                    continue;

                // Build full flattened name (normalized)
                string flattenedName = BuildFlattenedName(path, naming);

                if (!string.Equals(flattenedName, normalizedDest, StringComparison.OrdinalIgnoreCase))
                    continue;

                // If matched name, build expression path
                Expression? expr = BuildPropertyChainExpression(srcParam, path);
                if (expr == null)
                    continue;

                // If needed, ensure type conversion
                if (!expectedDestType.IsAssignableFrom(leaf.PropertyType))
                {
                    expr = Expression.Convert(expr, expectedDestType);
                }

                return expr;
            }

            return null;
        }

        // --------------------------------------------------------------------
        // BUILD CHAIN EXPRESSION (null‑safe)
        // --------------------------------------------------------------------
        private static Expression? BuildPropertyChainExpression(
            ParameterExpression srcParam, IReadOnlyList<PropertyInfo> path)
        {
            Expression current = srcParam;

            foreach (var prop in path)
            {
                if (prop.GetIndexParameters().Length > 0)
                    return null;

                // current = current?.Prop
                // but expression trees cannot use ?. operator, so we use:
                //
                // current == null ? null : current.Prop
                //
                // except if prop.PropertyType is value type.
                Type propType = prop.PropertyType;

                Expression getProp = Expression.Property(current, prop);

                if (!current.Type.IsValueType) // null‑safe only for ref types
                {
                    var nullTest = Expression.Equal(current, Expression.Constant(null, current.Type));

                    Expression nullReturn =
                        propType.IsValueType
                            ? Expression.Default(propType)
                            : Expression.Constant(null, propType);

                    getProp = Expression.Condition(nullTest, nullReturn, getProp);
                }

                current = getProp;
            }

            return current;
        }

        // --------------------------------------------------------------------
        // ENUMERATE PROPERTY PATHS ON SOURCE TYPE
        // --------------------------------------------------------------------
        private static IEnumerable<List<PropertyInfo>> EnumerateSourcePaths(Type type)
        {
            foreach (var p in GetPublicProps(type))
            {
                if (p.GetIndexParameters().Length > 0)
                    continue;

                var head = new List<PropertyInfo> { p };
                yield return head;

                if (!FlatteningTerminal(p.PropertyType))
                {
                    foreach (var sub in EnumerateSourcePaths(p.PropertyType))
                    {
                        var clone = new List<PropertyInfo>(1 + sub.Count) { p };
                        clone.AddRange(sub);
                        yield return clone;
                    }
                }
            }
        }

        private static PropertyInfo[] GetPublicProps(Type t)
            => t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // --------------------------------------------------------------------
        // SHOULD WE STOP RECURSION?
        // (Leaf types, primitives, enums, decimal, nullable primitives)
        // --------------------------------------------------------------------
        private static bool FlatteningTerminal(Type t)
        {
            if (t == typeof(string)) return true;
            if (t.IsPrimitive) return true;
            if (t.IsEnum) return true;
            if (t == typeof(decimal)) return true;

            var u = Nullable.GetUnderlyingType(t);
            return (u != null && u.IsPrimitive);
        }

        // --------------------------------------------------------------------
        // BUILD NORMALIZED FLATTENED NAME (Address + City → AddressCity)
        // --------------------------------------------------------------------
        private static string BuildFlattenedName(
            IReadOnlyList<PropertyInfo> path,
            INamingConvention naming)
        {
            string result = naming.Normalize(path[0].Name);

            for (int i = 1; i < path.Count; i++)
            {
                result += naming.Normalize(path[i].Name);
            }

            return result;
        }

        // --------------------------------------------------------------------
        // TYPE COMPATIBILITY CHECK
        // --------------------------------------------------------------------
        private static bool CanConvertValue(Type src, Type dest)
        {
            if (dest.IsAssignableFrom(src))
                return true;

            // allow nullable conversions
            var underlying = Nullable.GetUnderlyingType(dest);
            if (underlying != null)
                return underlying.IsAssignableFrom(src);

            return false;
        }
    }
}