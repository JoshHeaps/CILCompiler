using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record ReturnStatementNode(IValueAccessorNode ValueAccessor) : IExpressionNode
{
    public string Expression => $"return {ValueAccessor.GetValue()}";

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitExpression(this);

    public void Accept(INodeVisitor visitor) =>
        visitor.VisitExpression(this);
}
