namespace CILCompiler.ASTNodes.Interfaces;

public interface IValueAccessorNode : IAstNode
{
    public IExpressionNode ValueContainer { get; }
    public Type GetValueType();
    public object GetValue();
    public void SetValue(IExpressionNode expressionNode);
}
