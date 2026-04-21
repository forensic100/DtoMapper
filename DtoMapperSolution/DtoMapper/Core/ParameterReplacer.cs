using System.Linq.Expressions;

namespace DtoMapper.Core
{
    /// <summary>
    /// Replaces one parameter expression with another expression.
    ///
    /// Used by AutoMapBuilder when rewriting MapFrom expressions
    /// to bind them to the actual source parameter.
    /// </summary>
    internal sealed class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _from;
        private readonly Expression _to;

        public ParameterReplacer(ParameterExpression from, Expression to)
        {
            _from = from;
            _to = to;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _from
                ? _to
                : base.VisitParameter(node);
        }
    }
}