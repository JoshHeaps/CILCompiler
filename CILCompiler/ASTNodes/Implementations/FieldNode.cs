using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations;

public record FieldNode(string Name, object Value) : IFieldNode
{
    public Type Type { get => Value.GetType(); }

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitField(this);

    public void Accept(INodeVisitor visitor) =>
        visitor.VisitField(this);
}

public record FieldNode<T>(string Name, T TypedValue) : ITypedFieldNode<T> where T : class
{
    public Type Type { get => typeof(T); }
    public object Value { get => Value!; }

    public TResult Accept<TResult>(INodeVisitor<TResult> visitor) =>
        visitor.VisitField(this);

    public void Accept(INodeVisitor visitor) =>
        visitor.VisitField(this);
}
