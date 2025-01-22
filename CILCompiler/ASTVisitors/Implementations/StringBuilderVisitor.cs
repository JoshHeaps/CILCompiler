using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Implementations.FlowControllers;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTVisitors.Implementations;

public class StringBuilderVisitor : INodeVisitor<string>
{
    public string VisitCalculation(CalculationNode node, NodeVisitOptions? options = null) =>
        $"({node.Left.Accept(this)} {node.Operator} {node.Right.Accept(this)})";

    public string VisitExpression(IExpressionNode node, NodeVisitOptions? options = null) =>
        (node as LiteralNode)?.Value?.ToString() ?? "";

    public string VisitPrintStatement(PrintStatementNode node, NodeVisitOptions? options = null) =>
        $"print {node.Expression.Accept(this)};";

    public string VisitMethod(IMethodNode node, NodeVisitOptions? options = null)
    {
        string result = $"{node.Name}(";

        for (int i = 0; i < node.Parameters.Count; i++)
        {
            result += node.Parameters[i].Accept(this);

            if (i < node.Parameters.Count - 1)
                result += ", ";
        }

        return result + ");";
    }

    public string VisitObject(IObjectNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public string VisitField(IFieldNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public string VisitStatement(StatementNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public string VisitParameter(IParameterNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public string VisitLocalVariable(ILocalVariableNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public string VisitValueAccessor(IValueAccessorNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public string VisitAssignment(AssignmentNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public string VisitMethodCall(IMethodCallNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public string VisitIfStatement(IfStatementNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public string VisitWhileLoop(WhileLoopNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }
}
