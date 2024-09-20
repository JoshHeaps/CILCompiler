using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;
using System.Numerics;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record LiteralNode(object Value) : IExpressionNode
{
    public string Expression => Value?.ToString() ?? "";

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitExpression(this);
    public void Accept(INodeVisitor visitor) =>
        visitor.VisitExpression(this);
}

public record LiteralNode<T>(T Number) : IExpressionNode where T : INumber<T>
{
    public string Expression => Number.ToString()!;

    public TResult Accept<TResult>(INodeVisitor<TResult> visitor) =>
        visitor.VisitExpression(this);
    public void Accept(INodeVisitor visitor) =>
        visitor.VisitExpression(this);
}
