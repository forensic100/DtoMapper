using System;

namespace DtoMapper.Core
{
    public sealed class Mapper : IMapper
    {
        private readonly MapperConfiguration _config;

        internal Mapper(MapperConfiguration config)
        {
            _config = config;
        }

        public TDest Map<TSource, TDest>(TSource source)
        {
            if (source == null)
            {
                if (typeof(TDest).IsValueType)
                    return default!;
                return Activator.CreateInstance<TDest>()!;
            }

            var map = _config.GetTypeMap(typeof(TSource), typeof(TDest));
            var func = (Func<TSource, TDest>)map.CompiledDelegate!;
            return func(source);
        }

        public object? Map(object? source, Type sourceType, Type destinationType)
        {
            if (source == null)
                return destinationType.IsValueType
                    ? Activator.CreateInstance(destinationType)
                    : Activator.CreateInstance(destinationType);

            var map = _config.GetTypeMap(sourceType, destinationType);
            return map.CompiledDelegate!.DynamicInvoke(source);
        }
    }
}