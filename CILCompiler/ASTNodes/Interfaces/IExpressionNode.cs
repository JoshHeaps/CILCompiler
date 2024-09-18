namespace CILCompiler.ASTNodes.Interfaces;

public interface IExpressionNode : IAstNode
{
    public string Expression { get; }
}
