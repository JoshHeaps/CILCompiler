using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTVisitors.Interfaces;
using System.Reflection.Emit;

namespace CILCompiler.Utilities;

public static class Definitions
{
    public static readonly Dictionary<string, Type> StaticTypes = new()
    {
        { "int", typeof(int) },
        { "string", typeof(string) },
        { "bool", typeof(bool) },
        { "float", typeof(float) },
        { "double", typeof(double) },
        { "long", typeof(long) },
        { "short", typeof(short) },
        { "byte", typeof(byte) },
        { "char", typeof(char) },
        { "object", typeof(object) },
        { "void", typeof(void) },
    };

    public static Dictionary<string, Action<ILGenerator, MethodCallNode, INodeVisitor, NodeVisitOptions>> DefaultMethods { get; } = new()
    {
        { "Print", Print },
        { "PrintLine", PrintLine },
        { "ReadLine", ReadLine },
        { "ReadInt", ReadInt },
        { "RandomInt", RandomInt },
    };

    public static Dictionary<string, Type> DefaultMethodReturnTypes { get; } = new()
    {
        { "Print", typeof(void) },
        { "PrintLine", typeof(void) },
        { "ReadLine", typeof(string) },
        { "ReadInt", typeof(int) },
        { "RandomInt", typeof(int) },
    };

    private static void Print(ILGenerator il, MethodCallNode callNode, INodeVisitor visitor, NodeVisitOptions? options = null)
    {
        callNode.Arguments[0].Accept(visitor, options);
        var type = callNode.Arguments[0].GetValueType();

        if (type == typeof(object))
            type = typeof(string);

        if (type == typeof(void))
            throw new InvalidProgramException("Cannot get value from type void.");

        il.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", [type])!);
        Console.WriteLine($"call void [System.Console]System.Console::Write({type.Name})");
    }

    private static void PrintLine(ILGenerator il, MethodCallNode callNode, INodeVisitor visitor, NodeVisitOptions? options = null)
    {
        callNode.Arguments[0].Accept(visitor, options);
        var type = callNode.Arguments[0].GetValueType();

        if (type == typeof(void))
            throw new InvalidProgramException("Cannot get value from type void.");

        il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", [type])!);
        Console.WriteLine($"call void [System.Console]System.Console::WriteLine({type.Name})");
    }

    private static void ReadLine(ILGenerator il, MethodCallNode callNode, INodeVisitor visitor, NodeVisitOptions? options = null)
    {
        il.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadLine", [callNode.Arguments[0].GetValueType()])!);
        Console.WriteLine($"call string [System.Console]System.Console::ReadLine()");
    }

    private static void ReadInt(ILGenerator il, MethodCallNode callNode, INodeVisitor visitor, NodeVisitOptions? options = null)
    {
        il.Emit(OpCodes.Call, typeof(Console).GetMethod("ReadLine", [])!);
        Console.WriteLine($"call string [System.Console]System.Console::ReadLine()");
        il.Emit(OpCodes.Call, typeof(int).GetMethod("Parse", [typeof(string)])!);
        Console.WriteLine("call int32 [System.Runtime]System.Int32::Parse(string)");
    }

    private static void RandomInt(ILGenerator il, MethodCallNode callNode, INodeVisitor visitor, NodeVisitOptions? options = null)
    {
        il.Emit(OpCodes.Newobj, typeof(Random).GetConstructor([])!);
        Console.WriteLine($"newobj instance void [System.Runtime]System.Random::.ctor()");

        if (callNode.Arguments.Count == 2)
        {
            callNode.Arguments[0].Accept(visitor, options);
            callNode.Arguments[1].Accept(visitor, options);
            il.Emit(OpCodes.Callvirt, typeof(Random).GetMethod("Next", [typeof(int), typeof(int)])!);
            Console.WriteLine("callvirt instance int32 [System.Runtime]System.Random::Next(int32, int32)");
        }
        else if (callNode.Arguments.Count == 1)
        {
            callNode.Arguments[0].Accept(visitor, options);
            il.Emit(OpCodes.Callvirt, typeof(Random).GetMethod("Next", [typeof(int)])!);
            Console.WriteLine("callvirt instance int32 [System.Runtime]System.Random::Next(int32)");
        }
        else
        {
            il.Emit(OpCodes.Callvirt, typeof(Random).GetMethod("Next", [])!);
            Console.WriteLine("callvirt instance int32 [System.Runtime]System.Random::Next()");
        }
    }
}
