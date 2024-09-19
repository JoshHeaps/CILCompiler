using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record BinaryExpressionNode(ExpressionNode Left, ExpressionNode Right, string Operator) : IExpressionNode
{
    public string Expression => $"{Left.Expression} {Operator} {Right.Expression}";

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitBinaryExpression(this);

    public void Accept(INodeVisitor visitor) =>
        visitor.VisitBinaryExpression(this);
}
