﻿using CILCompiler.ASTNodes.Implementations;
using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Implementations.FlowControllers;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTVisitors.Implementations;

public class PrintVisitor : INodeVisitor
{
    public void VisitCalculation(CalculationNode node, NodeVisitOptions? options = null)
    {
        Console.Write("(");
        node.Left.Accept(this);
        Console.Write($" {node.Operator} ");
        node.Right.Accept(this);
        Console.Write(")");
    }

    public void VisitExpression(IExpressionNode node, NodeVisitOptions? options = null)
    {
        if (node is LiteralNode literal)
        {
            if (literal.Value.GetType() == typeof(string))
                Console.Write((literal?.Value?.GetType().Name ?? "") + " \"" + literal?.Value?.ToString() + "\""?? "");
            else
                Console.Write((literal?.Value?.GetType().Name ?? "") + " " + literal?.Value?.ToString() ?? "");
        }

        else if (node is CalculationNode binary)
            binary.Accept(this);

        else if (node is AssignmentNode assignment)
            assignment.Accept(this);

        else if (node is ReturnStatementNode returnStatement)
            VisitReturnStatement(returnStatement);
    }

    private void VisitReturnStatement(ReturnStatementNode returnStatement)
    {
        Console.Write("return");

        if (returnStatement.ValueAccessor.ValueContainer is not null)
        {
            Console.Write(" ");
            returnStatement.ValueAccessor.Accept(this);
        }
    }

    public void VisitPrintStatement(PrintStatementNode node, NodeVisitOptions? options = null)
    {
        Console.Write("print ");
        node.Expression.Accept(this);
        Console.WriteLine(";");
    }

    public void VisitMethod(IMethodNode node, NodeVisitOptions? options = null)
    {
        Console.Write($"{node.ReturnType.Name} {node.Name}(");

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

    public void VisitObject(IObjectNode node, NodeVisitOptions? options = null)
    {
        Console.WriteLine(node.Name);
        Console.WriteLine("{");

        Console.WriteLine();

        foreach (var field in node.Fields)
        {
            field.Accept(this);

            if (field.Type == typeof(string))
                Console.WriteLine($" = {field.Type.Name} \"{field.Value}\";");
            else
                Console.WriteLine($" = {field.Type.Name} {field.Value};");
        }

        Console.WriteLine();

        foreach (var method in node.Methods)
        {
            method.Accept(this);
            Console.WriteLine();
        }

        Console.WriteLine("}");
    }

    public void VisitField(IFieldNode node, NodeVisitOptions? options = null)
    {
        Console.Write($"{node.Type.Name} {node.Name}");
    }

    public void VisitStatement(StatementNode node, NodeVisitOptions? options = null)
    {
        Console.Write(node.Expression);
    }

    public void VisitParameter(IParameterNode node, NodeVisitOptions? options = null)
    {
        Console.Write($"{node.Type.Name} {node.Name}");
    }

    public void VisitLocalVariable(ILocalVariableNode node, NodeVisitOptions? options = null)
    {
        if (options is not null && !options.IsFirst)
        {
            Console.Write($"{node.Type.Name} {node.Name}");

            return;
        }

        LocalVariableNode variable = (node as LocalVariableNode)!;
        Console.Write($"{node.Type.Name} {node.Name} = ");
        variable.ValueAccessor.Accept(this);
    }

    public void VisitValueAccessor(IValueAccessorNode node, NodeVisitOptions? options = null) =>
        node.ValueContainer.Accept(this, new() { IsFirst = false });

    public void VisitAssignment(AssignmentNode node, NodeVisitOptions? options = null)
    {
        Console.Write($"{node.Type.Name} {node.VariableName} = ");
        node.ValueAccessor.Accept(this);
    }

    public void VisitMethodCall(IMethodCallNode node, NodeVisitOptions? options = null)
    {
        Console.Write($"{node.Expression}(");

        foreach (var argument in node.Arguments)
        {
            argument.Accept(this, options);
        }

        Console.Write(")");
    }

    public void VisitIfStatement(IfStatementNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public void VisitWhileLoop(WhileLoopNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public void VisitBinaryExpression(BinaryExpressionNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }
}
