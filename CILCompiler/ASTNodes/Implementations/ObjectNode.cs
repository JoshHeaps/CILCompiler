using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;

namespace CILCompiler.ASTNodes;

/// <summary>
/// This node represents the entirety of an object.
/// </summary>
public sealed record ObjectNode(string Name, List<IFieldNode> Fields, List<IMethodNode> Methods) : IObjectNode
{
    public T Accept<T>(INodeVisitor<T> visitor) =>
        visitor.VisitObject(this);
    public void Accept(INodeVisitor visitor, NodeVisitOptions? options = null) =>
        visitor.VisitObject(this, options);
}
