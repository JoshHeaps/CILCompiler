using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.FlowControllers;

public record IfStatementNode(IExpressionNode Condition, List<IExpressionNode> Body, List<IExpressionNode> ElseBody) : IFlowControllerNode
{
    public string Expression => throw new NotImplementedException();

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitIfStatement(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitIfStatement(this, options);
}
