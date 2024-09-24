using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations;

public record ValueAccessorNode : IValueAccessorNode
{
    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitValueAccessor(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitValueAccessor(this, options);

    public ValueAccessorNode(IExpressionNode expressionNode)
    {
        ValueHolder = expressionNode;
    }

    public IExpressionNode ValueHolder { get; set; }

    public Type GetValueType()
    {
        return ValueHolder switch
        {
            AssignmentNode assignmentNode => assignmentNode.Type,
            BinaryExpressionNode binaryExpression => binaryExpression.Left is not null ? new ValueAccessorNode(binaryExpression.Left).GetValueType() : binaryExpression.Right is not null ? new ValueAccessorNode(binaryExpression.Right).GetValueType() : throw new InvalidProgramException(),
            LiteralNode literalNode => literalNode.Value.GetType(),
            _ => throw new NotImplementedException(),
        };
    }

    public object GetValue()
    {
        return ValueHolder switch
        {
            AssignmentNode assignmentNode => GetValueFromAssignment(assignmentNode),
            BinaryExpressionNode binaryExpression => GetValueFromBinaryExpression(binaryExpression),
            LiteralNode literalNode => literalNode.Value,
            _ => throw new NotImplementedException(),
        };
    }

    private object GetValueFromAssignment(AssignmentNode assignmentNode) =>
        (assignmentNode.Expression,assignmentNode.ValueAccessor.GetValue());

    public void SetValue(IExpressionNode expressionNode)
    {
        ValueHolder = expressionNode;
    }

    private object GetValueFromBinaryExpression(BinaryExpressionNode binaryExpression)
    {
        IExpressionNode left = binaryExpression.Left;
        IExpressionNode right = binaryExpression.Right;

        object? leftValue = null;
        object? rightValue = null;

        if (left is LiteralNode)
            leftValue = (left as LiteralNode)!.Value;
        if (right is LiteralNode)
            rightValue = (right as LiteralNode)!.Value;

        if (leftValue is null || rightValue is null)
            throw new InvalidProgramException();

        if (leftValue.GetType() != rightValue.GetType())
            throw new InvalidProgramException();

        Dictionary<string, Func<object, object, object>> operations = new()
        {
            { "+", (l, r) => (int)l + (int)r },
            { "-", (l, r) => (int)l - (int)r },
            { "*", (l, r) => (int)l * (int)r },
            { "/", (l, r) => (int)l / (int)r },
        };

        return operations[binaryExpression.Operator].Invoke(leftValue, rightValue);
    }
}
