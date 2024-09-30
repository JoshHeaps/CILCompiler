using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTVisitors.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

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

    public static readonly Dictionary<string, Action<ILGenerator, MethodCallNode, INodeVisitor, NodeVisitOptions>> DefaultMethods = new()
    {
        { "Print", Print },
        { "PrintLine", PrintLine },
        { "ReadLine", ReadLine },
        { "ReadInt", ReadInt },
    };

    private static void Print(ILGenerator il, MethodCallNode callNode, INodeVisitor visitor, NodeVisitOptions? options = null)
    {
        callNode.Arguments[0].Accept(visitor, options);
        il.Emit(OpCodes.Call, typeof(Console).GetMethod("Write", [callNode.Arguments[0].GetValueType()])!);
        Console.WriteLine($"call void [System.Console]System.Console::Write({callNode.Arguments[0].GetValueType().Name})");
    }

    private static void PrintLine(ILGenerator il, MethodCallNode callNode, INodeVisitor visitor, NodeVisitOptions? options = null)
    {
        callNode.Arguments[0].Accept(visitor, options);
        il.Emit(OpCodes.Call, typeof(Console).GetMethod("WriteLine", [callNode.Arguments[0].GetValueType()])!);
        Console.WriteLine($"call void [System.Console]System.Console::WriteLine({callNode.Arguments[0].GetValueType().Name})");
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
}
