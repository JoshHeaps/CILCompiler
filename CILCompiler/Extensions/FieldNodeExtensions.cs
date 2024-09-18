using CILCompiler.ASTNodes.Implementations;
using CILCompiler.ASTNodes.Interfaces;

namespace CILCompiler.Extensions;

public static class FieldNodeExtensions
{
    public static ITypedFieldNode<T> GetTypedField<T>(this IFieldNode field) where T : class
    {
        if (field.Value is not T)
            throw new InvalidOperationException();

        return new FieldNode<T>(field.Name, (field.Value as T)!);
    }

    public static IFieldNode GetGenericField<T>(this ITypedFieldNode<T> field) where T : class =>
        new FieldNode(field.Name, field.Value!);
}
