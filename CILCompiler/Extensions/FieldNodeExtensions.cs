using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Interfaces;

namespace CILCompiler.Extensions;

public static class FieldNodeExtensions
{
    public static ITypedFieldNode<T> GetTypedField<T>(this IFieldNode field) where T : class
    {
        if (field.Value is not T)
            throw new InvalidOperationException();

        return new FieldNode<T>(field.Name, field.ValueContainer);
    }

    public static IFieldNode GetGenericField<T>(this ITypedFieldNode<T> field) where T : class =>
        new FieldNode(field.Name, field.ValueContainer);
}
