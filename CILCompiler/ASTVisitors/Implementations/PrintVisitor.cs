using CILCompiler.ASTNodes.Implementations;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTVisitors.Implementations;

public class PrintVisitor : INodeVisitor
{
    public void VisitBinaryExpression(BinaryExpressionNode node)
    {
        Console.Write("(");
        node.Left.Accept(this);
        Console.Write($" {node.Operator} ");
        node.Right.Accept(this);
        Console.Write(")");
    }

    public void VisitLiteral(LiteralNode node)
    {
        Console.Write(node?.Value?.ToString() ?? "");
    }

    public void VisitPrintStatement(PrintStatementNode node)
    {

        Console.Write("print ");
        node.Expression.Accept(this);
        Console.WriteLine(";");
    }

    public void VisitMethodCall(IMethodNode node)
    {
        Console.Write($"{node.Name}(");

        for (int i = 0; i < node.Parameters.Count; i++)
        {
            node.Parameters[i].Accept(this);

            if (i < node.Parameters.Count - 1)
                Console.Write(", ");
        }

        Console.WriteLine(")");
        Console.WriteLine("{");

        foreach (var line in node.Body)
        {
            Console.Write("    ");
            line.Accept(this);
            Console.WriteLine(";");
        }

        Console.WriteLine("}");
    }

    public void VisitObject(IObjectNode node)
    {
        Console.WriteLine(node.Name);
        Console.WriteLine("{");

        foreach (var field in node.Fields)
            field.Accept(this);

        Console.WriteLine();

        foreach (var method in node.Methods)
            method.Accept(this);

        Console.WriteLine("}");
    }

    public void VisitField(IFieldNode node)
    {
        Console.WriteLine($"{node.Type.Name} {node.Name} = {node.Value};");
    }

    public void VisitStatement(StatementNode node)
    {
        Console.Write(node.Expression);
    }

    public void VisitParameter(IParameterNode node)
    {
        Console.Write($"{node.Type.Name} {node.Name}");
    }
}
