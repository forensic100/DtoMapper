using System;
using System.Linq.Expressions;

namespace DtoMapper.Core
{
    /// <summary>
    /// Fluent wrapper returned by AutoRegister.
    /// Supports ConvertUsing(), ForMember(), and ReverseMap().
    /// </summary>
    public sealed class AutoMapExpression<TSource, TDest>
    {
        private readonly MapperConfiguration _config;

        internal AutoMapExpression(MapperConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Registers a global converter for this source/destination pair.
        /// This is pure configuration sugar over AddGlobalConverter.
        /// </summary>
        public AutoMapExpression<TSource, TDest> ConvertUsing(
            Func<TSource, TDest> converter)
        {
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            _config.AddGlobalConverter(converter);
            return this;
        }

        /// <summary>
        /// Overrides mapping for a specific destination member.
        ///
        /// Example:
        ///   ForMember(d => d.Name, opt => opt.MapFrom(s => s.FullName))
        /// </summary>
        public AutoMapExpression<TSource, TDest> ForMember<TMember>(
            Expression<Func<TDest, TMember>> destinationMember,
            Action<MemberOptions<TSource, TMember>> options)
        {
            if (destinationMember == null)
                throw new ArgumentNullException(nameof(destinationMember));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // Ensure destination member is a simple property access
            if (destinationMember.Body is not MemberExpression memberExpr)
            {
                throw new ArgumentException(
                    "ForMember requires a destination property access expression",
                    nameof(destinationMember));
            }

            var destPropertyName = memberExpr.Member.Name;

            var typeMap = _config.GetTypeMap(
                typeof(TSource),
                typeof(TDest));

            var memberOptions =
                new MemberOptions<TSource, TMember>(
                    typeMap,
                    destPropertyName);

            options(memberOptions);

            return this;
        }

        /// <summary>
        /// Registers the reverse TypeMap.
        /// </summary>
        public MapperConfiguration ReverseMap()
        {
            _config.ReverseMap();
            return _config;
        }
    }
}
