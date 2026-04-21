using System;
using System.Collections.Generic;

namespace DtoMapper.Core
{
    public sealed class TypeMap
    {
        public Type SourceType { get; }
        public Type DestinationType { get; }

        internal Delegate? CompiledDelegate { get; set; }

        // ✅ NEW: explicit member maps
        internal List<MemberMap> CustomMemberMaps { get; } =
            new List<MemberMap>();

        public TypeMap(Type sourceType, Type destinationType)
        {
            SourceType = sourceType ?? throw new ArgumentNullException(nameof(sourceType));
            DestinationType = destinationType ?? throw new ArgumentNullException(nameof(destinationType));
        }
    }
}