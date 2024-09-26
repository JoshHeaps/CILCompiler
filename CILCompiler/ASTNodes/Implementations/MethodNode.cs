using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations;

public record MethodNode(string Name, Type ReturnType, List<IExpressionNode> Body, List<IParameterNode> Parameters) : IMethodNode
{
    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitMethod(this);
    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitMethod(this, options);
}
