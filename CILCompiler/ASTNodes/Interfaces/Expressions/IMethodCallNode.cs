namespace CILCompiler.ASTNodes.Interfaces;

public interface IMethodCallNode : IExpressionNode
{
    List<IValueAccessorNode> Arguments { get; }
    IMethodNode MethodNode { get; }
    Type GetMethodNodeType();
}
