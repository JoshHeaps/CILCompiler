namespace CILCompiler.ASTNodes.Interfaces;

public interface IMethodCallNode : IExpressionNode
{
    List<IValueAccessorNode> Arguments { get; }
    public IMethodNode MethodNode { get; }
}
