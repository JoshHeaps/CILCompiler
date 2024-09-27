using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.FlowControllers;

public record IfStatementNode : IFlowControllerNode
{
    public IExpressionNode Condition => throw new NotImplementedException();

    public List<IExpressionNode> Body => throw new NotImplementedException();

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        throw new NotImplementedException();
    }

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }
}
