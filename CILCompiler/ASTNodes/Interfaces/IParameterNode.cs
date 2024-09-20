namespace CILCompiler.ASTNodes.Interfaces;

public interface IParameterNode : IAstNode
{
    public IValueAccessorNode ValueAccessor { get; }
    public Type Type { get; }
    public string Name { get; }
}
