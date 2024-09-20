using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTVisitors.Implementations;

public class ILCreationVisitor : INodeVisitor
{
    public void VisitBinaryExpression(BinaryExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitField(IFieldNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitExpression(IExpressionNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitLocalVariable(ILocalVariableNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitMethodCall(IMethodNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitObject(IObjectNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitParameter(IParameterNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitPrintStatement(PrintStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitStatement(StatementNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitValueAccessor(IValueAccessorNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitAssignment(AssignmentNode node)
    {
        throw new NotImplementedException();
    }
}
