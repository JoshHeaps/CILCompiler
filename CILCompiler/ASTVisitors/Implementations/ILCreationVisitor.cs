using CILCompiler.ASTNodes.Implementations;
using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Implementations.FlowControllers;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;
using CILCompiler.Extensions;
using CILCompiler.Utilities;
using System.Reflection;
using System.Reflection.Emit;

namespace CILCompiler.ASTVisitors.Implementations;

public class ILCreationVisitor : INodeVisitor
{
    public List<TypeBuilder> CompileObject(List<IObjectNode> nodes)
    {
        nodes[0].Accept(this);

        return [typeBuilder!];
    }

    private TypeBuilder? _tb;
    private TypeBuilder typeBuilder { get => _tb ?? throw new Exception("Type Builder is not defined"); set => _tb = value; }
    private List<FieldBuilder> fields = [];
    private List<MethodBuilder> methods = [];
    private List<(ILocalVariableNode node, LocalBuilder builder)> locals = [];

    public void VisitObject(IObjectNode node, NodeVisitOptions? options = null)
    {
        var assemblyName = new AssemblyName("CIL");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? "CIL");

        typeBuilder = moduleBuilder.DefineType(node.Name, TypeAttributes.Public);

        BuildConstructorAndFields(node);

        Console.WriteLine();
        Console.WriteLine();

        var methodOptions = DefineMethods(node.Methods);

        foreach ((var method, var option) in methodOptions)
        {
            Console.WriteLine($"{typeBuilder.Name}::{method.Name}({string.Join(", ", method.Parameters.Select(x => x.Type.Name))})");
            method.Accept(this, option);
            Console.WriteLine();
            Console.WriteLine();
        }
    }

    private List<(IMethodNode node, NodeVisitOptions options)> DefineMethods(List<IMethodNode> methodNodes)
    {
        List<(IMethodNode node, NodeVisitOptions options)> methodOptions = [];

        foreach (var method in methodNodes)
        {
            Type[] parameterTypes = method.Parameters.Select(x => x.Type).ToArray();
            var methodBuilder = typeBuilder.DefineMethod(method.Name, MethodAttributes.Public, method.ReturnType, parameterTypes);
            List<ParameterBuilder> parameters = [];

            for (int i = 0; i < parameterTypes.Length; i++)
                parameters.Add(methodBuilder.DefineParameter(i + 1, ParameterAttributes.In, method.Parameters[i].Name));

            var il = methodBuilder.GetILGenerator();

            methodOptions.Add((method, new() { IL = il, Parameters = parameters, Method = methodBuilder }));
            methods.Add(methodBuilder);
        }

        return methodOptions;
    }

    public void VisitField(IFieldNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        var field = fields.FirstOrDefault(x => x.Name == node.Name) ?? throw new NullReferenceException($"{node.Name} does not exist in this context.");

        il.Emit(OpCodes.Ldarg_0);
        Console.WriteLine(@"ldarg.0");
        il.Emit(OpCodes.Ldfld, field);
        Console.WriteLine($"ldfld {node.Type.Name.ToLower()} {typeBuilder.Name}::{field.Name}");
    }

    public void VisitMethod(IMethodNode node, NodeVisitOptions? options = null)
    {
        locals = [];

        foreach (var expression in node.Body)
        {
            expression.Accept(this, options);
        }
    }

    public void VisitBinaryExpression(BinaryExpressionNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        node.Left.Accept(this, options);
        node.Right.Accept(this, options);

        if (node.Operator == "+" && new ValueAccessorNode(node.Left).GetValueType() == typeof(int))
        {
            il.Emit(OpCodes.Add);
            Console.WriteLine("add");
        }
        else if (node.Operator == "+" && new ValueAccessorNode(node.Left).GetValueType() == typeof(string))
        {
            var method = typeof(string).GetMethod("Concat", [typeof(string), typeof(string)])!;
            il.Emit(OpCodes.Call, method);
            Console.WriteLine("call string [System.Runtime]System.String::Concat(string, string)");
        }
        else if (node.Operator == "-")
        {
            il.Emit(OpCodes.Sub);
            Console.WriteLine("sub");
        }
        else if (node.Operator == "*")
        {
            il.Emit(OpCodes.Mul);
            Console.WriteLine("mul");
        }
        else if (node.Operator == "/")
        {
            il.Emit(OpCodes.Div);
            Console.WriteLine("div");
        }
    }

    public void VisitExpression(IExpressionNode node, NodeVisitOptions? options = null)
    {
        if (node is LiteralNode literal)
            AddToStack(literal, options);

        else if (node is BinaryExpressionNode binary)
            binary.Accept(this, options);

        else if (node is AssignmentNode assignment)
            assignment.Accept(this, options);

        else if (node is ReturnStatementNode returnStatement)
            VisitReturnStatement(returnStatement, options);
    }

    public void VisitLocalVariable(ILocalVariableNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        LocalVariableNode variable = (node as LocalVariableNode)!;

        if (locals.Any(x => x.node.Name == variable.Name))
        {
            var local = locals.First(x => x.node.Name == variable.Name);
            il.Emit(OpCodes.Ldloc, local.builder.LocalIndex);
            Console.WriteLine($"ldloc.{local.builder.LocalIndex}");

            return;
        }

        locals.Add((variable, il.DeclareLocal(variable.Type)));
        variable.ValueAccessor.Accept(this, options);
        il.Emit(OpCodes.Stloc, locals[^1].builder);
        Console.WriteLine($"stloc.{locals[^1].builder.LocalIndex}");
    }

    public void VisitParameter(IParameterNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;
        var param = options.Parameters.First(x => x.Name == node.Name);

        il.Emit(OpCodes.Ldarg, (short)param.Position);
        Console.WriteLine($"ldarg.{param.Position}");
    }

    public void VisitPrintStatement(PrintStatementNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public void VisitStatement(StatementNode node, NodeVisitOptions? options = null)
    {
        throw new NotImplementedException();
    }

    public void VisitValueAccessor(IValueAccessorNode node, NodeVisitOptions? options = null)
    {
        node.ValueContainer.Accept(this, options);
    }

    public void VisitAssignment(AssignmentNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        var parameter = options.Parameters.FirstOrDefault(x => x.Name == node.VariableName);
        (ILocalVariableNode? node, LocalBuilder? builder) local = locals!.FirstOrDefault(x => x.node?.Name == node.VariableName, (default, default));
        var field = fields.FirstOrDefault(x => x.Name == node.VariableName);

        if (parameter is null && local.builder is null && field is null)
            throw new Exception("No space to assign to");

        if (field is not null)
        {
            il.Emit(OpCodes.Ldarg_0);
            Console.WriteLine(@"ldarg.0");
        }

        //if (parameter is not null)
        //{
        //    il.Emit(OpCodes.Ldarg, (short)parameter.Position);
        //    Console.WriteLine($"ldarg.{(short)parameter.Position}");
        //}

        node.ValueAccessor.Accept(this, options); // Add value to store at top of the stack.

        if (parameter is not null)
        {
            il.Emit(OpCodes.Starg, parameter.Position);
            Console.WriteLine($"starg.{parameter.Position}");
        }
        else if (local.builder is not null)
        {
            il.Emit(OpCodes.Stloc, local.builder);
            Console.WriteLine($"stloc.{local.builder.LocalIndex}");
        }
        else if (field is not null)
        {
            il.Emit(OpCodes.Stfld, field);
            Console.WriteLine($"stfld {field.FieldType.Name} {field.Name}");
        }
    }

    private void BuildConstructorAndFields(IObjectNode node)
    {
        var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, []);
        ILGenerator constructorGenerator = constructorBuilder.GetILGenerator();

        Console.WriteLine($"{typeBuilder.Name}::.cctor");

        var constructorInfo = typeof(object).GetConstructor([]);
        constructorGenerator.Emit(OpCodes.Ldarg_0);
        Console.WriteLine("ldarg_0");
        constructorGenerator.Emit(OpCodes.Call, constructorInfo!);
        Console.WriteLine($"call {constructorInfo!.Name}");

        foreach (var field in node.Fields)
        {
            fields.Add(typeBuilder.DefineField(field.Name, field.Type, FieldAttributes.Private));
            constructorGenerator.Emit(OpCodes.Ldarg_0);
            Console.WriteLine("ldarg_0");

            if (AddToStackDelegates.TryGetValue(field.Type, out Action<ILGenerator, object>? value))
                value(constructorGenerator, field.Value);
            else
            {
                constructorGenerator.AddObjectToStack(field.Value);
                Console.WriteLine(@"il.AddObjectToStack(literal.Value);");
            }

            constructorGenerator.Emit(OpCodes.Stfld, fields[^1]);
            Console.WriteLine($"stfld {typeBuilder.Name}::{fields[^1].Name}");
        }

        constructorGenerator.Emit(OpCodes.Ret);
        Console.WriteLine("ret");
    }

    private Dictionary<Type, Action<ILGenerator, object>> AddToStackDelegates = new()
    {
        { 
            typeof(int), (il, x) => 
            {
                il.Emit(OpCodes.Ldc_I4, (int)x);
                Console.WriteLine($"ldc.i4.s {(int)x}");
            } 
        },
        { 
            typeof(string), (il, x) =>
            {
                il.Emit(OpCodes.Ldstr, x.ToString() ?? "");
                Console.WriteLine($"ldstr {x}");
            }
        },
    };

    private void AddToStack(LiteralNode literal, NodeVisitOptions? options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        var type = literal.Value.GetType();

        if (AddToStackDelegates.TryGetValue(type, out Action<ILGenerator, object>? value))
            value(il, literal.Value);
        else
        {
            il.AddObjectToStack(literal.Value);
            Console.WriteLine(@"il.AddObjectToStack(literal.Value);");
        }
    }

    private void VisitReturnStatement(ReturnStatementNode node, NodeVisitOptions? options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        if (node.ValueAccessor.ValueContainer is not null)
            node.ValueAccessor.Accept(this, options);

        il.Emit(OpCodes.Ret);
        Console.WriteLine(@"ret");
    }

    public void VisitMethodCall(IMethodCallNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        var callNode = node as MethodCallNode ?? throw new NullReferenceException();

        if (Definitions.DefaultMethods.TryGetValue(callNode.MethodNode.Name, out Action<ILGenerator, MethodCallNode, INodeVisitor, NodeVisitOptions>? value))
        {
            value.Invoke(il, callNode, this, options);

            return;
        }

        il.Emit(OpCodes.Ldarg_0);
        Console.WriteLine(@"ldarg.0");

        foreach (var arg in node.Arguments)
        {
            arg.Accept(this, options);
        }

        il.Emit(OpCodes.Call, methods.First(x => x.Name == callNode.MethodNode.Name));
        Console.WriteLine($"call instance {callNode.MethodNode.ReturnType.Name.ToLower()} {typeBuilder.Name}::{callNode.MethodNode.Name}({string.Join(", ", node.Arguments.Select(x => x.GetValueType()))})");
    }

    Dictionary<string, OpCode> ComparisonActions = new()
    {
        { ">", OpCodes.Ble }, // do the opposite comparison because it branches to the else block.
        { ">=", OpCodes.Blt },
        { "<", OpCodes.Bge },
        { "<=", OpCodes.Bgt },
        { "==", OpCodes.Bne_Un },
        { "!=", OpCodes.Beq },
    };

    public void VisitIfStatement(IfStatementNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        var elseLabel = il.DefineLabel();
        var endIfLabel = il.DefineLabel();

        node.Condition.Left.Accept(this, options);
        node.Condition.Right.Accept(this, options);
        il.Emit(ComparisonActions[node.Condition.ComparisonOperator], elseLabel);
        Console.WriteLine($"{ComparisonActions[node.Condition.ComparisonOperator]} ELSE_LABEL");
        Console.WriteLine();

        foreach (var expression in node.Body)
            expression.Accept(this, options);

        il.Emit(OpCodes.Br_S, endIfLabel);
        Console.WriteLine("br.s END_IF_LABEL");

        Console.WriteLine();
        il.MarkLabel(elseLabel);
        Console.WriteLine("ELSE_LABEL:");

        foreach (var expression in node.ElseBody)
            expression.Accept(this, options);

        Console.WriteLine();
        il.MarkLabel(endIfLabel);
        Console.WriteLine("END_IF_LABEL:");
    }
}