using System;
using System.Collections.Generic;
using System.Linq;
using DtoMapper.AutoMap;
using DtoMapper.Converters;
using DtoMapper.Naming;

namespace DtoMapper.Core
{
    public sealed class MapperConfiguration
    {
        private readonly Dictionary<(Type Src, Type Dest), TypeMap> _typeMaps =
            new Dictionary<(Type, Type), TypeMap>();

        private readonly ConverterRegistry _converters = new ConverterRegistry();
        private readonly INamingConvention _naming = new DefaultNamingConvention();

        private bool _built;
        private TypeMap? _lastRegistered;

        // =============================================================
        // REGISTRATION
        // =============================================================
        public AutoMapExpression<TSource, TDest> AutoRegister<TSource, TDest>()
        {
            EnsureNotBuilt();

            var src = typeof(TSource);
            var dest = typeof(TDest);

            // 🚫 ENUMS ARE NOT TYPEMAPS
            // Enums are mapped inline, best-effort, by name.
            if (src.IsEnum && dest.IsEnum)
            {
                _lastRegistered = null;
                return new AutoMapExpression<TSource, TDest>(this);
            }

            var key = (src, dest);

            if (!_typeMaps.ContainsKey(key))
            {
                _typeMaps[key] = new TypeMap(src, dest);
            }

            _lastRegistered = _typeMaps[key];
            return new AutoMapExpression<TSource, TDest>(this);
        }

        public void ReverseMap()
        {
            EnsureNotBuilt();

            if (_lastRegistered == null)
                throw new InvalidOperationException(
                    "ReverseMap() must be called after AutoRegister<TSource, TDest>().");

            var src = _lastRegistered.SourceType;
            var dest = _lastRegistered.DestinationType;

            // 🚫 ENUMS HAVE NO REVERSE TYPEMAPS
            if (src.IsEnum && dest.IsEnum)
                return;

            var reverseKey = (dest, src);

            if (!_typeMaps.ContainsKey(reverseKey))
            {
                _typeMaps[reverseKey] = new TypeMap(dest, src);
            }
        }

        // =============================================================
        // CONVERTERS
        // =============================================================
        public void AddGlobalConverter<TSource, TDest>(Func<TSource, TDest> converter)
        {
            EnsureNotBuilt();
            _converters.Add(typeof(TSource), typeof(TDest), converter);
        }

        // =============================================================
        // INTERNAL ACCESS (USED BY AutoMapBuilder)
        // =============================================================
        internal bool HasTypeMap(Type source, Type destination)
            => _typeMaps.ContainsKey((source, destination));

        internal TypeMap GetTypeMap(Type source, Type destination)
            => _typeMaps[(source, destination)];

        // =============================================================
        // BUILD
        // =============================================================
        public Mapper Build()
        {
            EnsureNotBuilt();
            _built = true;

            // ✅ FINAL SAFETY NET:
            // Completely remove any enum → enum typemaps that might
            // have been registered indirectly (ReverseMap, ordering, tests).
            foreach (var key in _typeMaps.Keys
                .Where(k => k.Src.IsEnum && k.Dest.IsEnum)
                .ToList())
            {
                _typeMaps.Remove(key);
            }

            var builder = new AutoMapBuilder(_naming, _converters, this);

            foreach (var map in _typeMaps.Values)
            {
                map.CompiledDelegate =
                    builder.BuildLambda(map.SourceType, map.DestinationType)
                           .Compile();
            }

            return new Mapper(this);
        }

        // =============================================================
        // GUARDS
        // =============================================================
        private void EnsureNotBuilt()
        {
            if (_built)
                throw new InvalidOperationException(
                    "MapperConfiguration cannot be modified after Build() has been called.");
        }
    }
}