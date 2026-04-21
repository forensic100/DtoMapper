namespace DtoMapper.Naming
{
    /// <summary>
    /// Defines a naming normalization strategy used across the AutoMap system
    /// for direct mapping, flattening, and reverse‑flattening.
    ///
    /// Implementations should:
    ///   • Normalize case
    ///   • Remove separators (underscores, hyphens)
    ///   • Provide consistent comparison across model layers
    /// </summary>
    public interface INamingConvention
    {
        /// <summary>
        /// Normalizes a property name for comparison.
        /// </summary>
        string Normalize(string name);
    }
}
