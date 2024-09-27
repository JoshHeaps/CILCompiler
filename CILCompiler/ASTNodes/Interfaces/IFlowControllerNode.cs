namespace CILCompiler.ASTNodes.Interfaces;

public interface IFlowControllerNode : IAstNode
{
    public IExpressionNode Condition { get; }
    public List<IExpressionNode> Body { get; }
}
