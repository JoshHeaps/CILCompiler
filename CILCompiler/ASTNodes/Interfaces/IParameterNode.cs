namespace CILCompiler.ASTNodes.Interfaces;

public interface IParameterNode : IAstNode
{
    public Type Type { get; }
    public string Name { get; }
}
