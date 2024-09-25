using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record MethodCallNode(ObjectNode ObjectNode, string MethodName, List<IValueAccessorNode> Arguments) : IMethodCallNode
{
    public string Expression => $"{ObjectNode.Name}.{MethodName}({string.Join(", ", Arguments.Select(x => x.GetValue())) ?? string.Empty});";

    private IMethodNode _method = ObjectNode.Methods.First(x => x.Name == MethodName);
    public IMethodNode MethodNode => _method;

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitMethodCall(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitMethodCall(this, options);

}
