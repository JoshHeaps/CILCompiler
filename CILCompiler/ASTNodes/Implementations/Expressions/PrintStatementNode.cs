using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record PrintStatementNode(IExpressionNode Expression) : IAstNode
{
    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitPrintStatement(this);
    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitPrintStatement(this, options);
}
