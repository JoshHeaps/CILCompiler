using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Interfaces;

public interface IAstNode
{
    public T Accept<T>(INodeVisitor<T> visitor);
    public void Accept(INodeVisitor visitor);
}
