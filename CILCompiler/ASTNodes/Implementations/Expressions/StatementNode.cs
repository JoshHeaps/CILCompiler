using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record StatementNode(string Expression) : IExpressionNode
{
    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitStatement(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitStatement(this, options);
}
