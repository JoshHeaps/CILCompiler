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
        ValueContainer = expressionNode;
    }

    public IExpressionNode ValueContainer { get; set; }

    public Type GetValueType()
    {
        return ValueContainer switch
        {
            AssignmentNode assignmentNode => assignmentNode.Type,
            BinaryExpressionNode binaryExpression => binaryExpression.Left is not null ? new ValueAccessorNode(binaryExpression.Left).GetValueType() : binaryExpression.Right is not null ? new ValueAccessorNode(binaryExpression.Right).GetValueType() : throw new InvalidProgramException(),
            LiteralNode literal => literal.Value.GetType(),
            FieldNode field => field.Type,
            LocalVariableNode localVariable => localVariable.Type,
            ParameterNode parameter => parameter.ValueAccessor.GetValueType(),
            MethodCallNode methodCall => methodCall.GetMethodNodeType(),
            _ => throw new NotImplementedException(),
        };
    }

    public object GetValue()
    {
        return ValueContainer switch
        {
            AssignmentNode assignment => GetValueFromAssignment(assignment),
            BinaryExpressionNode binaryExpression => GetValueFromBinaryExpression(binaryExpression),
            LiteralNode literal => literal.Value,
            FieldNode field => field.Value,
            LocalVariableNode localVariable => localVariable.Value,
            ParameterNode parameter => parameter.ValueAccessor.GetValue(),
            MethodCallNode methodCall => GetDefaultOfMethodType(methodCall),
            _ => throw new NotImplementedException(),
        };
    }

    private object GetDefaultOfMethodType(MethodCallNode node)
    {
        if (node.MethodNode.ReturnType == typeof(string))
            return "";

        var result = Activator.CreateInstance(node.MethodNode.ReturnType);

        return result!;
    }

    private object GetValueFromAssignment(AssignmentNode assignmentNode) =>
        (assignmentNode.Expression,assignmentNode.ValueAccessor.GetValue());

    public void SetValue(IExpressionNode expressionNode)
    {
        ValueContainer = expressionNode;
    }

    private object GetValueFromBinaryExpression(BinaryExpressionNode binaryExpression)
    {
        IExpressionNode left = binaryExpression.Left;
        IExpressionNode right = binaryExpression.Right;

        object? leftValue = new ValueAccessorNode(left).GetValue();
        object? rightValue = new ValueAccessorNode(right).GetValue();

        if (leftValue is null || rightValue is null)
            throw new InvalidProgramException();

        if (leftValue.GetType() != rightValue.GetType())
            throw new InvalidProgramException();

        Dictionary<(Type, string), Func<object, object, object>> operations = new()
        {
            { (typeof(int), "+"), (l, r) => (int)l + (int)r },
            { (typeof(int), "-"), (l, r) => (int)l - (int)r },
            { (typeof(int), "*"), (l, r) => (int)l * (int)r },
            { (typeof(int), "/"), (l, r) => (int)l / (int)r },
            { (typeof(int), "%"), (l, r) => (int)l % (int)r },
            { (typeof(string), "+"), (l, r) => (string)l + (string)r },
        };

        return operations[(leftValue.GetType(), binaryExpression.Operator)].Invoke(leftValue, rightValue);
    }
}
