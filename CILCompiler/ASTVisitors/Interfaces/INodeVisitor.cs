using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Implementations.FlowControllers;
using CILCompiler.ASTNodes.Interfaces;
using System.Reflection.Emit;

namespace CILCompiler.ASTVisitors.Interfaces;

public interface INodeVisitor<T>
{
    public T VisitBinaryExpression(BinaryExpressionNode node, NodeVisitOptions? options = null);
    public T VisitExpression(IExpressionNode node, NodeVisitOptions? options = null);
    public T VisitPrintStatement(PrintStatementNode node, NodeVisitOptions? options = null);
    public T VisitMethod(IMethodNode node, NodeVisitOptions? options = null);
    public T VisitObject(IObjectNode node, NodeVisitOptions? options = null);
    public T VisitField(IFieldNode node, NodeVisitOptions? options = null);
    public T VisitStatement(StatementNode node, NodeVisitOptions? options = null);
    public T VisitParameter(IParameterNode node, NodeVisitOptions? options = null);
    public T VisitLocalVariable(ILocalVariableNode node, NodeVisitOptions? options = null);
    public T VisitValueAccessor(IValueAccessorNode node, NodeVisitOptions? options = null);
    public T VisitAssignment(AssignmentNode node, NodeVisitOptions? options = null);
    public T VisitMethodCall(IMethodCallNode node, NodeVisitOptions? options = null);
    public T VisitIfStatement(IfStatementNode node, NodeVisitOptions? options = null);
    public T VisitWhileLoop(WhileLoopNode node, NodeVisitOptions? options = null);
}

public interface INodeVisitor
{
    public void VisitBinaryExpression(BinaryExpressionNode node, NodeVisitOptions? options = null);
    public void VisitExpression(IExpressionNode node, NodeVisitOptions? options = null);
    public void VisitPrintStatement(PrintStatementNode node, NodeVisitOptions? options = null);
    public void VisitMethod(IMethodNode node, NodeVisitOptions? options = null);
    public void VisitObject(IObjectNode node, NodeVisitOptions? options = null);
    public void VisitField(IFieldNode node, NodeVisitOptions? options = null);
    public void VisitStatement(StatementNode node, NodeVisitOptions? options = null);
    public void VisitParameter(IParameterNode node, NodeVisitOptions? options = null);
    public void VisitLocalVariable(ILocalVariableNode node, NodeVisitOptions? options = null);
    public void VisitValueAccessor(IValueAccessorNode node, NodeVisitOptions? options = null);
    public void VisitAssignment(AssignmentNode node, NodeVisitOptions? options = null);
    public void VisitMethodCall(IMethodCallNode node, NodeVisitOptions? options = null);
    public void VisitIfStatement(IfStatementNode node, NodeVisitOptions? options = null);
    public void VisitWhileLoop(WhileLoopNode node, NodeVisitOptions? options = null);
}

public class NodeVisitOptions
{
    private ILGenerator? _generator;
    public ILGenerator IL { get => _generator ?? throw new NullReferenceException("ILGenerator is null"); set => _generator = value; }

    public List<ParameterBuilder> Parameters { get; set; } = [];

    private MethodBuilder _methodBuilder;
    public MethodBuilder Method { get => _methodBuilder ?? throw new NullReferenceException("Method is null"); set => _methodBuilder = value; }

    public bool IsFirst = true;
}