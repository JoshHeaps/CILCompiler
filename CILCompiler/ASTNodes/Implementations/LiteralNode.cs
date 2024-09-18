using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations;

public record LiteralNode(object Value) : IExpressionNode
{
    public string Expression => Value?.ToString() ?? "";

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitLiteral(this);
    public void Accept(INodeVisitor visitor) =>
        visitor.VisitLiteral(this);
}