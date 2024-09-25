namespace CILCompiler.ASTNodes.Interfaces;

public interface IMethodCallNode : IExpressionNode
{
    public ObjectNode ObjectNode { get; }
    public IMethodNode MethodNode { get; }
    List<IValueAccessorNode> Arguments { get; }
}
