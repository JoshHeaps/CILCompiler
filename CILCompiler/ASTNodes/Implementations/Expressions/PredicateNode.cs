using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record PredicateNode(IExpressionNode Left, IExpressionNode Right, string ComparisonOperator) : IExpressionNode
{
    public string Expression => $"{Left.Expression} {ComparisonOperator} {Right.Expression}";

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitExpression(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitExpression(this, options);
}