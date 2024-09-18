using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations;

public record StatementNode(string Expression) : IExpressionNode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    public void Accept(INodeVisitor visitor)
    {
        throw new NotImplementedException();
    }
}
