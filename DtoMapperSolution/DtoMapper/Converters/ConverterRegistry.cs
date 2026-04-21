using System;
using System.Collections.Generic;

namespace DtoMapper.Converters
{
    internal sealed class ConverterRegistry
    {
        private readonly Dictionary<(Type Src, Type Dest), Delegate> _converters =
            new Dictionary<(Type, Type), Delegate>();

        public void Add(Type sourceType, Type destType, Delegate converter)
        {
            if (sourceType == null) throw new ArgumentNullException(nameof(sourceType));
            if (destType == null) throw new ArgumentNullException(nameof(destType));
            if (converter == null) throw new ArgumentNullException(nameof(converter));

            _converters[(sourceType, destType)] = converter;
        }

        public bool TryResolve(Type sourceType, Type destType, out Delegate converter)
        {
            return _converters.TryGetValue((sourceType, destType), out converter!);
        }
    }
}
