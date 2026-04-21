using System.Linq.Expressions;

namespace DtoMapper.Core
{
    internal sealed class MemberMap
    {
        public string DestinationName { get; }
        public LambdaExpression? SourceExpression { get; }
        public bool IsIgnored { get; }

        private MemberMap(
            string destinationName,
            LambdaExpression? sourceExpression,
            bool isIgnored)
        {
            DestinationName = destinationName;
            SourceExpression = sourceExpression;
            IsIgnored = isIgnored;
        }

        public MemberMap(string destinationName, LambdaExpression sourceExpression)
            : this(destinationName, sourceExpression, false)
        {
        }

        public static MemberMap Ignore(string destinationName)
        {
            return new MemberMap(destinationName, null, true);
        }
    }
}