﻿using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes.Implementations.Expressions;

public record BinaryExpressionNode(IExpressionNode Left, IExpressionNode Right, string Operator) : IExpressionNode
{
    public string Expression => throw new NotImplementedException();

    public T Accept<T>(INodeVisitor<T> visitor) =>
        throw new NotImplementedException();

    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitBinaryExpression(this, options);
}
