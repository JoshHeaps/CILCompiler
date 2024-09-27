using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

// This needs to be a class so we can update the reference for binary expressions.
public class MethodCallNode(IMethodNode methodNode, List<IValueAccessorNode> arguments) : IMethodCallNode
{
    public string Expression => $"{MethodNode.Name}";
    public IMethodNode MethodNode { get => methodNode; set => methodNode = value; }
    public List<IValueAccessorNode> Arguments { get => arguments; }

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitMethodCall(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitMethodCall(this, options);
}
