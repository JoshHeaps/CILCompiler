using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations;

public record ParameterNode(Type Type, string Name) : IParameterNode
{
    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitParameter(this);

    public void Accept(INodeVisitor visitor) =>
        visitor.VisitParameter(this);
}
