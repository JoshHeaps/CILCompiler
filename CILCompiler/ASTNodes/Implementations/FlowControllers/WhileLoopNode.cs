using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.FlowControllers;

public record WhileLoopNode(IExpressionNode Condition, List<IExpressionNode> Body) : IFlowControllerNode
{
    public string Expression => throw new NotImplementedException();

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitWhileLoop(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitWhileLoop(this, options);
}
