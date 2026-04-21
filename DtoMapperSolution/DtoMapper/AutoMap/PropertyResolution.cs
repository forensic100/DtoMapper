using System.Linq;
using System.Reflection;
using DtoMapper.Naming;

namespace DtoMapper.AutoMap
{
    /// <summary>
    /// Provides helpers for direct property resolution based on normalized names.
    /// Used by AutoMapBuilder before attempting flattening or reverse-flattening.
    /// </summary>
    internal static class PropertyResolution
    {
        /// <summary>
        /// Attempts to find a direct property on the source type
        /// whose normalized name matches the destination name.
        ///
        /// Example:
        ///     destName = "FirstName"
        ///     srcProps = [ first_name, FIRSTNAME, FirstName ]
        ///     → returns whichever matches after normalization
        /// </summary>
        public static PropertyInfo? ResolveDirect(
            INamingConvention naming,
            string destName,
            PropertyInfo[] srcProps)
        {
            string norm = naming.Normalize(destName);

            return srcProps.FirstOrDefault(p =>
                naming.Normalize(p.Name)
                    .Equals(norm, System.StringComparison.OrdinalIgnoreCase));
        }
    }
}