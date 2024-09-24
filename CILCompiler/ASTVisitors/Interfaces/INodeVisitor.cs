using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Interfaces;
using System.Reflection.Emit;

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
    public void VisitBinaryExpression(BinaryExpressionNode node, NodeVisitOptions? options = null);
    public void VisitExpression(IExpressionNode node, NodeVisitOptions? options = null);
    public void VisitPrintStatement(PrintStatementNode node, NodeVisitOptions? options = null);
    public void VisitMethodCall(IMethodNode node, NodeVisitOptions? options = null);
    public void VisitObject(IObjectNode node, NodeVisitOptions? options = null);
    public void VisitField(IFieldNode node, NodeVisitOptions? options = null);
    public void VisitStatement(StatementNode node, NodeVisitOptions? options = null);
    public void VisitParameter(IParameterNode node, NodeVisitOptions? options = null);
    public void VisitLocalVariable(ILocalVariableNode node, NodeVisitOptions? options = null);
    public void VisitValueAccessor(IValueAccessorNode node, NodeVisitOptions? options = null);
    public void VisitAssignment(AssignmentNode node, NodeVisitOptions? options = null);
}

public interface IOptionsNodeVisitor
{
    public void VisitBinaryExpression(BinaryExpressionNode node, NodeVisitOptions? options = null);
    public void VisitExpression(IExpressionNode node, NodeVisitOptions? options = null);
    public void VisitPrintStatement(PrintStatementNode node, NodeVisitOptions? options = null);
    public void VisitMethodCall(IMethodNode node, NodeVisitOptions? options = null);
    public void VisitObject(IObjectNode node, NodeVisitOptions? options = null);
    public void VisitField(IFieldNode node, NodeVisitOptions? options = null);
    public void VisitStatement(StatementNode node, NodeVisitOptions? options = null);
    public void VisitParameter(IParameterNode node, NodeVisitOptions? options = null);
    public void VisitLocalVariable(ILocalVariableNode node, NodeVisitOptions? options = null);
    public void VisitValueAccessor(IValueAccessorNode node, NodeVisitOptions? options = null);
    public void VisitAssignment(AssignmentNode node, NodeVisitOptions? options = null);
    public void VisitReturnStatement(ReturnStatementNode node, NodeVisitOptions? options = null);
}

public class NodeVisitOptions
{
    private ILGenerator? _generator;
    public ILGenerator IL { get => _generator ?? throw new NullReferenceException("ILGenerator is null"); set => _generator = value; }

    public List<ParameterBuilder> Parameters { get; set; } = [];

    private MethodBuilder _methodBuilder;
    public MethodBuilder Method { get => _methodBuilder ?? throw new NullReferenceException("Method is null"); set => _methodBuilder = value; }
}