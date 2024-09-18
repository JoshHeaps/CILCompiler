namespace CILCompiler.ASTNodes.Interfaces;

public interface IFieldNode : IAstNode
{
    public Type Type { get; }
    public string Name { get; }
    public object Value { get; }

}

/// <summary>
/// I think these may be useful later, but I'm currently unsure.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITypedFieldNode<T> : IFieldNode where T : class
{
    public T TypedValue { get; }
}
