using CILCompiler.ASTNodes.Implementations.Expressions;

namespace CILCompiler.ASTNodes.Interfaces;

public interface IFlowControllerNode : IExpressionNode
{
    public PredicateNode Condition { get; }
    public List<IExpressionNode> Body { get; }
}
