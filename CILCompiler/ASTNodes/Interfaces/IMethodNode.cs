namespace CILCompiler.ASTNodes.Interfaces;

public interface IMethodNode : IAstNode
{
    public Type ReturnType { get; }
    public string Name { get; }
    public string Body { get; }
    public List<IParameterNode> Parameters { get; }
}
