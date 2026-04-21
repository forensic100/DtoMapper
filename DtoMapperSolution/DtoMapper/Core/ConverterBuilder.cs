using System;

namespace DtoMapper.Core
{
    public sealed class ConverterBuilder<TSource, TDest>
    {
        private readonly MapperConfiguration _config;

        internal ConverterBuilder(MapperConfiguration config)
        {
            _config = config;
        }

        public ConverterBuilder<TSource, TDest> ConvertUsing(
            Func<TSource, TDest> converter)
        {
            // IMPORTANT:
            // This must delegate to the EXISTING mechanism
            _config.AddGlobalConverter(converter);
            return this;
        }

        public void ReverseMap()
        {
            _config.ReverseMap();
        }
    }
}