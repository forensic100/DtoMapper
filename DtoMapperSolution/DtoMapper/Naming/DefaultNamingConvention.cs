using System;

namespace DtoMapper.Naming
{
    /// <summary>
    /// Default naming normalization strategy used throughout the 
    /// mapping engine to ensure consistent property name comparison.
    ///
    /// Rules:
    ///   • Trim whitespace
    ///   • Convert to lowercase
    ///   • Remove underscores
    ///   • Remove hyphens
    ///
    /// Examples:
    ///   "FirstName"      → "firstname"
    ///   "first_name"     → "firstname"
    ///   "FIRST-NAME"     → "firstname"
    /// </summary>
    public sealed class DefaultNamingConvention : INamingConvention
    {
        public string Normalize(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            return name
                .Trim()
                .Replace("_", "")
                .Replace("-", "")
                .ToLowerInvariant();
        }
    }
}