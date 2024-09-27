namespace CILCompiler.ASTNodes.Interfaces;

public interface IParameterNode : IExpressionNode
{
    public IValueAccessorNode ValueAccessor { get; }
    public Type Type { get; }
    public string Name { get; }
}
