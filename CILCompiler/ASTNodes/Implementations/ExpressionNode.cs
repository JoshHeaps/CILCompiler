using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations;

public abstract record ExpressionNode : IExpressionNode
{
    public abstract string Expression { get; }
    public abstract T Accept<T>(INodeVisitor<T> visitor);
    public abstract void Accept(INodeVisitor visitor);
}
