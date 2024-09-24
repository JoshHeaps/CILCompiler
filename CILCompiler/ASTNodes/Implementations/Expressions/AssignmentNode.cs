using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record AssignmentNode(Type Type, string VariableName, IValueAccessorNode ValueAccessor) : IExpressionNode
{
    public string Expression => $"{VariableName} = {ValueAccessor.GetValue()}";

    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitAssignment(this);

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitAssignment(this, options);
}
