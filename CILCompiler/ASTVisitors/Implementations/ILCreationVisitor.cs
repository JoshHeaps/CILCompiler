using CILCompiler.ASTNodes.Implementations;
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

    public void VisitLiteral(LiteralNode node)
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

    public void VisitPrintStatement(PrintStatementNode node)
    {
        throw new NotImplementedException();
    }

    public void VisitStatement(StatementNode node)
    {
        throw new NotImplementedException();
    }
}
