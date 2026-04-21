namespace DtoMapper.Core
{
    /// <summary>
    /// Minimal mapper interface used by MapperConfiguration.Build().
    /// </summary>
    public interface IMapper
    {
        TDest Map<TSource, TDest>(TSource source);
        object? Map(object? source, System.Type srcType, System.Type destType);
    }
}