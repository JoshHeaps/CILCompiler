namespace CILCompiler.ASTNodes.Interfaces;

public interface IObjectNode : IAstNode
{
    public string Name { get; }
    public List<IFieldNode> Fields { get; }
    public List<IMethodNode> Methods { get; }
}
