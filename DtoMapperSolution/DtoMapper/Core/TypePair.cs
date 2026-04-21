using System;

namespace DtoMapper.Core
{
    /// <summary>
    /// Represents a unique (Source → Destination) mapping pair.
    /// Used as a dictionary key in MapperConfiguration & Mapper.
    ///
    /// Struct is immutable and optimized for dictionary lookup.
    /// </summary>
    public readonly struct TypePair : IEquatable<TypePair>
    {
        public Type Source { get; }
        public Type Destination { get; }

        public TypePair(Type source, Type destination)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Destination = destination ?? throw new ArgumentNullException(nameof(destination));
        }

        public bool Equals(TypePair other)
        {
            return Source == other.Source &&
                   Destination == other.Destination;
        }

        public override bool Equals(object? obj)
        {
            return obj is TypePair other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + Source.GetHashCode();
                h = h * 31 + Destination.GetHashCode();
                return h;
            }
        }

        public static bool operator ==(TypePair left, TypePair right) => left.Equals(right);
        public static bool operator !=(TypePair left, TypePair right) => !left.Equals(right);

        public override string ToString()
        {
            return $"{Source.FullName} → {Destination.FullName}";
        }
    }
}