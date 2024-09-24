namespace CILCompiler.ASTNodes.Interfaces;

public interface IValueAccessorNode : IAstNode
{
    public IExpressionNode ValueHolder { get; }
    public Type GetValueType();
    public object GetValue();
    public void SetValue(IExpressionNode expressionNode);
}
