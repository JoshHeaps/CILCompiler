using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTVisitors.Implementations;

public class StringBuilderVisitor : INodeVisitor<string>
{
    public string VisitBinaryExpression(BinaryExpressionNode node) =>
        $"({node.Left.Accept(this)} {node.Operator} {node.Right.Accept(this)})";

    public string VisitExpression(IExpressionNode node) =>
        (node as LiteralNode)?.Value?.ToString() ?? "";

    public string VisitPrintStatement(PrintStatementNode node) =>
        $"print {node.Expression.Accept(this)};";

    public string VisitMethodCall(IMethodNode node)
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

    public string VisitObject(IObjectNode node)
    {
        throw new NotImplementedException();
    }

    public string VisitField(IFieldNode node)
    {
        throw new NotImplementedException();
    }

    public string VisitStatement(StatementNode node)
    {
        throw new NotImplementedException();
    }

    public string VisitParameter(IParameterNode node)
    {
        throw new NotImplementedException();
    }

    public string VisitLocalVariable(ILocalVariableNode node)
    {
        throw new NotImplementedException();
    }

    public string VisitValueAccessor(IValueAccessorNode node)
    {
        throw new NotImplementedException();
    }

    public string VisitAssignment(AssignmentNode node)
    {
        throw new NotImplementedException();
    }

    public string VisitMethodCall(IMethodCallNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }
}
