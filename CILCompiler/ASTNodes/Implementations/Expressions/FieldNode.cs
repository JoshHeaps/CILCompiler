using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record FieldNode(Type Type, string Name, IExpressionNode ValueContainer) : IFieldNode
{
    public string Expression { get => $"{Type.Name} {Name} {Value}"; }

    public object Value { get => new ValueAccessorNode(ValueContainer).GetValue(); }

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitField(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitField(this, options);
}

public record FieldNode<T>(string Name, IExpressionNode ValueContainer) : ITypedFieldNode<T> where T : class
{
    public Type Type { get => typeof(T); }
    public object Value { get => Value!; }
    public string Expression { get => $"{Type.Name} {Name} {Value}"; }

    public T TypedValue
    {
        get
        {
            if (Value is not T typed)
                throw new InvalidOperationException();

            return typed;
        }
    }

    public TResult Accept<TResult>(INodeVisitor<TResult> visitor) =>
        visitor.VisitField(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitField(this, options);
}
