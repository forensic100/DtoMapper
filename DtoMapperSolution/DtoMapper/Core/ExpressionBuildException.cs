using System;

namespace DtoMapper.Core
{
    /// <summary>
    /// Exception thrown when AutoMapBuilder fails to build
    /// a mapping expression between two types.
    ///
    /// This is primarily used to wrap reflection and expression
    /// construction errors with clearer diagnostic context.
    /// </summary>
    public sealed class ExpressionBuildException : Exception
    {
        /// <summary>
        /// Gets the source type from the mapping operation that failed.
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Gets the destination type from the mapping operation that failed.
        /// </summary>
        public Type DestinationType { get; }

        public ExpressionBuildException(
            Type sourceType,
            Type destType,
            string message)
            : base(BuildMessage(sourceType, destType, message))
        {
            SourceType = sourceType;
            DestinationType = destType;
        }

        public ExpressionBuildException(
            Type sourceType,
            Type destType,
            string message,
            Exception inner)
            : base(BuildMessage(sourceType, destType, message), inner)
        {
            SourceType = sourceType;
            DestinationType = destType;
        }

        private static string BuildMessage(Type source, Type destination, string details)
        {
            return $"Failed to build mapping expression: " +
                   $"{source.FullName} → {destination.FullName}. " +
                   $"{details}";
        }
    }
}