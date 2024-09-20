using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Interfaces;

namespace CILCompiler.ASTVisitors.Interfaces;

public interface INodeVisitor<T>
{
    public T VisitBinaryExpression(BinaryExpressionNode node);
    public T VisitExpression(IExpressionNode node);
    public T VisitPrintStatement(PrintStatementNode node);
    public T VisitMethodCall(IMethodNode node);
    public T VisitObject(IObjectNode node);
    public T VisitField(IFieldNode node);
    public T VisitStatement(StatementNode node);
    public T VisitParameter(IParameterNode node);
    public T VisitLocalVariable(ILocalVariableNode node);
    public T VisitValueAccessor(IValueAccessorNode node);
    public T VisitAssignment(AssignmentNode node);
}

public interface INodeVisitor
{
    public void VisitBinaryExpression(BinaryExpressionNode node);
    public void VisitExpression(IExpressionNode node);
    public void VisitPrintStatement(PrintStatementNode node);
    public void VisitMethodCall(IMethodNode node);
    public void VisitObject(IObjectNode node);
    public void VisitField(IFieldNode node);
    public void VisitStatement(StatementNode node);
    public void VisitParameter(IParameterNode node);
    public void VisitLocalVariable(ILocalVariableNode node);
    public void VisitValueAccessor(IValueAccessorNode node);
    public void VisitAssignment(AssignmentNode node);
}