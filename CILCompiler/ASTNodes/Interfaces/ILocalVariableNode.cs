namespace CILCompiler.ASTNodes.Interfaces;

public interface ILocalVariableNode : IExpressionNode
{
    public Type Type { get; }
    public string Name { get; }
    public object Value { get; }
}

/// <summary>
/// I think these may be useful later, but I'm currently unsure.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ILocalVariableNode<T> : ILocalVariableNode where T : class
{
    public T TypedValue { get; }
}
