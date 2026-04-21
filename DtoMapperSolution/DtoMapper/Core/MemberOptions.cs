using System;
using System.Linq.Expressions;

namespace DtoMapper.Core
{
    public sealed class MemberOptions<TSource, TDestMember>
    {
        private readonly TypeMap _typeMap;
        private readonly string _destinationName;

        internal MemberOptions(TypeMap typeMap, string destinationName)
        {
            _typeMap = typeMap ?? throw new ArgumentNullException(nameof(typeMap));
            _destinationName = destinationName ??
                               throw new ArgumentNullException(nameof(destinationName));
        }

        public void MapFrom(Expression<Func<TSource, TDestMember>> sourceExpression)
        {
            if (sourceExpression == null)
                throw new ArgumentNullException(nameof(sourceExpression));

            _typeMap.CustomMemberMaps.Add(
                new MemberMap(_destinationName, sourceExpression));
        }

        public void MapFromCollection<TSourceElement>(
            Expression<Func<TSource, System.Collections.Generic.IEnumerable<TSourceElement>>> sourceExpression)
        {
            if (sourceExpression == null)
                throw new ArgumentNullException(nameof(sourceExpression));

            _typeMap.CustomMemberMaps.Add(
                new MemberMap(_destinationName, sourceExpression));
        }

        /// ✅ NEW: Ignore this destination member
        public void Ignore()
        {
            _typeMap.CustomMemberMaps.Add(
                MemberMap.Ignore(_destinationName));
        }
    }
}