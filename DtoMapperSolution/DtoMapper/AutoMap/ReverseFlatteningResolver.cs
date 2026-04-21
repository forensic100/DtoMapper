using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using DtoMapper.Naming;

namespace DtoMapper.AutoMap
{
    internal static class ReverseFlatteningResolver
    {
        /// <summary>
        /// Reconstructs a nested object graph based on a flattened leaf.
        /// 
        /// Example:
        ///     Flat.CustomerName → Customer.Name
        /// 
        /// ReverseMap should reconstruct:
        ///     new Customer { Name = Flat.CustomerName }
        /// 
        /// IMPORTANT:
        /// - Only supports OBJECT unflattening (not arrays/lists).
        /// - Caller must handle root‑null logic (AutoMapBuilder does).
        /// </summary>
        public static Expression? TryResolveReverseFlattening(
            INamingConvention naming,
            ParameterExpression srcParam,
            Type srcType,
            Type destRootType,
            PropertyInfo destLeaf)
        {
            if (srcParam == null || destLeaf == null)
                return null;

            string leafName = naming.Normalize(destLeaf.Name);

            foreach (var flatPath in EnumerateSourcePaths(srcType))
            {
                if (flatPath.Count == 0)
                    continue;

                var leaf = flatPath[flatPath.Count - 1];

                if (leaf.GetIndexParameters().Length > 0)
                    continue;

                if (!IsLeafType(leaf.PropertyType))
                    continue;

                // Compare "CustomerName" → "Customer" + "Name"
                string combined = BuildFlattenedName(flatPath, naming);

                if (!string.Equals(combined, leafName, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Now reconstruct nested tree: Customer.Name
                var treeExpr = BuildReverseTree(
                    srcParam,
                    flatPath,
                    destRootType,
                    destLeaf);

                return treeExpr;
            }

            return null;
        }

        // --------------------------------------------------------------------
        // BUILD REVERSE TREE (nested unflattening)
        // --------------------------------------------------------------------
        private static Expression? BuildReverseTree(
            ParameterExpression srcParam,
            IReadOnlyList<PropertyInfo> flatPath,
            Type destRootType,
            PropertyInfo destLeaf)
        {
            PropertyInfo leafSrc = flatPath[flatPath.Count - 1];

            Expression leafValueExpr = Expression.Property(srcParam, leafSrc);

            // Convert if needed
            if (!destLeaf.PropertyType.IsAssignableFrom(leafSrc.PropertyType))
            {
                leafValueExpr = Expression.Convert(leafValueExpr, destLeaf.PropertyType);
            }

            // Bottom-most object that owns the leaf:
            Type ownerType = destLeaf.DeclaringType!;
            var ownerInit = Expression.MemberInit(
                Expression.New(ownerType),
                Expression.Bind(destLeaf, leafValueExpr));

            Expression current = ownerInit;

            // Walk backward up the chain
            // flatPath: [ Customer, Name ]
            // destPath: Customer.Name
            // (Build Customer { Name = xxx })
            for (int i = flatPath.Count - 2; i >= 0; i--)
            {
                PropertyInfo destProp =
                    destRootType.GetProperty(flatPath[i].Name,
                        BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)!;

                var wrapperInit = Expression.MemberInit(
                    Expression.New(destProp.PropertyType),
                    Expression.Bind(destProp, current));

                current = wrapperInit;
            }

            return current;
        }

        // --------------------------------------------------------------------
        // SOURCE PATH ENUMERATION (for detecting flatten paths)
        // --------------------------------------------------------------------
        private static IEnumerable<List<PropertyInfo>> EnumerateSourcePaths(Type type)
        {
            foreach (var p in GetPublicProps(type))
            {
                if (p.GetIndexParameters().Length > 0)
                    continue;

                var head = new List<PropertyInfo> { p };
                yield return head;

                if (!IsLeafType(p.PropertyType))
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
        // SUPPORT METHODS
        // --------------------------------------------------------------------

        /// <summary>
        /// Reverse flatten only operates on actual “leaf” values.
        /// </summary>
        private static bool IsLeafType(Type t)
        {
            if (t == typeof(string)) return true;
            if (t.IsPrimitive) return true;
            if (t.IsEnum) return true;
            if (t == typeof(decimal)) return true;

            var u = Nullable.GetUnderlyingType(t);
            return (u != null && u.IsPrimitive);
        }

        /// <summary>
        /// Builds flattened name from a path:
        ///     [Customer, Name] → "CustomerName"
        ///     normalized by naming convention.
        /// </summary>
        private static string BuildFlattenedName(
            IReadOnlyList<PropertyInfo> path,
            INamingConvention naming)
        {
            string name = naming.Normalize(path[0].Name);

            for (int i = 1; i < path.Count; i++)
                name += naming.Normalize(path[i].Name);

            return name;
        }
    }
}