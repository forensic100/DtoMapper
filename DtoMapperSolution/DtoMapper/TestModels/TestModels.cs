namespace DtoMapper.TestModels
{
    /// <summary>
    /// Flat DTO used to test reverse flattening:
    /// src.Street → dest.Customer.Address.Street
    /// </summary>
    public class RDto
    {
        public string? Street { get; set; }
    }

    /// <summary>
    /// Nested leaf object containing the Street property.
    /// </summary>
    public class RAddress
    {
        public string? Street { get; set; }
    }

    /// <summary>
    /// Wraps RAddress. Required to validate full nested reconstruction.
    /// </summary>
    public class RCustomer
    {
        public RAddress? Address { get; set; }
    }

    /// <summary>
    /// The root of the nested object graph.
    /// Used as the destination for reverse flattening.
    /// </summary>
    public class ROrder
    {
        public RCustomer? Customer { get; set; }
    }
}