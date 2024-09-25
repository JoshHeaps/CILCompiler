using CILCompiler.ASTNodes.Implementations;
using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.ASTVisitors.Interfaces;
using CILCompiler.Extensions;
using System.Reflection;
using System.Reflection.Emit;

namespace CILCompiler.ASTVisitors.Implementations;

public class ILCreationVisitor : INodeVisitor
{
    public TypeBuilder CompileObject(IObjectNode node)
    {
        node.Accept(this);

        return typeBuilder!;
    }

    private TypeBuilder? _tb;
    private TypeBuilder typeBuilder { get => _tb ?? throw new Exception("Type Builder is not defined"); set => _tb = value; }
    private List<FieldBuilder> fields = [];
    private List<(ILocalVariableNode node, LocalBuilder builder)> locals = [];
    private List<MethodBuilder> methods = [];
    private int expressionChecks = 0;

    public void VisitObject(IObjectNode node, NodeVisitOptions? options = null)
    {
        var assemblyName = new AssemblyName("CIL");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? "CIL");

        typeBuilder = moduleBuilder.DefineType(node.Name, TypeAttributes.Public);

        BuildConstructorAndFields(node);

        Console.WriteLine();
        Console.WriteLine();

        foreach (var method in node.Methods)
            method.Accept(this);

        Console.WriteLine();
        Console.WriteLine();
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
        Console.WriteLine($"ldfld {node.Type.Name} {field.Name}");
    }

    public void VisitMethodCall(IMethodNode node, NodeVisitOptions? options = null)
    {
        locals = [];
        Type[] parameterTypes = node.Parameters.Select(x => x.Type).ToArray();
        var methodBuilder = typeBuilder.DefineMethod(node.Name, MethodAttributes.Public, node.ReturnType, parameterTypes);
        List<ParameterBuilder> parameters = [];

        for (int i = 0; i < parameterTypes.Length; i++)
            parameters.Add(methodBuilder.DefineParameter(i + 1, ParameterAttributes.In, node.Parameters[i].Name));

        var il = methodBuilder.GetILGenerator();

        foreach (var expression in node.Body)
        {
            expressionChecks = 0;
            expression.Accept(this, new() { IL = il, Parameters = parameters, Method = methodBuilder });
        }
    }

    public void VisitBinaryExpression(BinaryExpressionNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        node.Left.Accept(this, options);
        expressionChecks++;
        node.Right.Accept(this, options);

        if (node.Operator == "+")
        {
            il.Emit(OpCodes.Add);
            Console.WriteLine("add");
        }
        else if (node.Operator == "-")
        {
            il.Emit(OpCodes.Sub);
            Console.WriteLine("sub");
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

        expressionChecks++;
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
        node.ValueHolder.Accept(this, options);
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

        // This might be necessary, I'm unsure.
        var constructorInfo = typeof(object).GetConstructor([]);
        constructorGenerator.Emit(OpCodes.Ldarg_0);
        constructorGenerator.Emit(OpCodes.Call, constructorInfo!);

        foreach (var field in node.Fields)
        {
            fields.Add(typeBuilder.DefineField(field.Name, field.Type, FieldAttributes.Private));
            constructorGenerator.Emit(OpCodes.Ldarg_0);

            if (AddToStackDelegates.TryGetValue(field.Type, out Action<ILGenerator, object>? value))
                value(constructorGenerator, field.Value);
            else
            {
                constructorGenerator.AddObjectToStack(field.Value);
                Console.WriteLine(@"il.AddObjectToStack(literal.Value);");
            }

            constructorGenerator.Emit(OpCodes.Stfld, fields[^1]);
        }

        constructorGenerator.Emit(OpCodes.Ret);
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

        node.ValueAccessor.Accept(this, options);
        il.Emit(OpCodes.Ret);
        Console.WriteLine(@"ret");
    }
}