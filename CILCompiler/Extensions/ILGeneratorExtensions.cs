using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace CILCompiler.Extensions;

public static class ILGeneratorExtensions
{
    /// <summary>
    /// Burn a reference to the specified runtime object instance into the DynamicMethod
    /// </summary>
    public static void AddObjectToStack<TInst>(this ILGenerator il, TInst inst, bool disposePointer = false)
        where TInst : class
    {
        var gch = GCHandle.Alloc(inst);

        var ptr = GCHandle.ToIntPtr(gch);

        if (IntPtr.Size == 4)
            il.Emit(OpCodes.Ldc_I4, ptr.ToInt32());
        else
            il.Emit(OpCodes.Ldc_I8, ptr.ToInt64());

        il.Emit(OpCodes.Ldobj, typeof(TInst));

        if (disposePointer)
            gch.Free();
    }
}
