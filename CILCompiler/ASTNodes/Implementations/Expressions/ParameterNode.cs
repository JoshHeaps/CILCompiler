using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record ParameterNode(Type Type, string Name) : IParameterNode
{
    public IValueAccessorNode ValueAccessor { get => Type == typeof(string) ? new ValueAccessorNode(new LiteralNode("")) : new ValueAccessorNode(new LiteralNode(Activator.CreateInstance(Type)!)); }
    public string Expression { get => $"{Type.Name} {Name}"; }

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitParameter(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitParameter(this, options);
}
