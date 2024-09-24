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

    public void VisitObject(IObjectNode node, NodeVisitOptions? options = null)
    {
        var assemblyName = new AssemblyName("CIL");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name ?? "CIL");

        typeBuilder = moduleBuilder.DefineType(node.Name, TypeAttributes.Public);

        BuildConstructorAndFields(node);

        foreach (var method in node.Methods)
            method.Accept(this);
    }

    public void VisitField(IFieldNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        var field = fields.FirstOrDefault(x => x.Name == node.Name) ?? throw new NullReferenceException($"{node.Name} does not exist in this context.");

        il.Emit(OpCodes.Ldarg_0);
        //Console.WriteLine(@"il.Emit(OpCodes.Ldarg_0);");
        il.Emit(OpCodes.Ldfld, field);
        //Console.WriteLine($"il.Emit(OpCodes.Ldfld, {field.Name});");
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
            expression.Accept(this, new() { IL = il, Parameters = parameters, Method = methodBuilder });
        }

        //il.Emit(OpCodes.Ldarg, parameters[0].Position);
        //il.Emit(OpCodes.Ldc_I4, 10);
        //il.Emit(OpCodes.Add);
        //il.Emit(OpCodes.Starg_S, (byte)parameters[0].Position);
        //il.Emit(OpCodes.Ldarg_0);
        //il.Emit(OpCodes.Ldarg_1);
        //il.Emit(OpCodes.Ldarg_0);
        //il.Emit(OpCodes.Ldfld, fields[0]);
        //il.Emit(OpCodes.Sub);
        //il.Emit(OpCodes.Stfld, fields[0]);
        //il.Emit(OpCodes.Ldarg_0);
        //il.Emit(OpCodes.Ldfld, fields[0]);
        //il.Emit(OpCodes.Ldarg_1);
        //il.Emit(OpCodes.Add);
        //il.Emit(OpCodes.Ret);
    }

    public void VisitBinaryExpression(BinaryExpressionNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        node.Left.Accept(this, options);
        node.Right.Accept(this, options);

        if (node.Operator == "+")
        {
            il.Emit(OpCodes.Add);
            //Console.WriteLine("il.Emit(OpCodes.Add);");
        }
        else if (node.Operator == "-")
        {
            il.Emit(OpCodes.Sub);
            //Console.WriteLine("il.Emit(OpCodes.Sub);");
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

        locals.Add((variable, il.DeclareLocal(variable.Type)));
        variable.ValueAccessor.Accept(this, options);
        il.Emit(OpCodes.Stloc, locals[^1].builder);
        //Console.WriteLine($"il.Emit(OpCodes.Stloc, {locals[^1].node.Name});");
    }

    public void VisitParameter(IParameterNode node, NodeVisitOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;
        var param = options.Parameters.First(x => x.Name == node.Name);

        il.Emit(OpCodes.Ldarg, (short)param.Position);
        //Console.WriteLine($"il.Emit(OpCodes.Ldarg, {param.Position});");
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

        node.ValueAccessor.Accept(this, options); // Add value to store at top of the stack.

        if (parameter is not null)
        {
            il.Emit(OpCodes.Starg, parameter.Position);
            //Console.WriteLine($"il.Emit(OpCodes.Starg, {parameter.Position});");
            il.Emit(OpCodes.Ldarg_0);
        }
        else if (local.builder is not null)
        {
            il.Emit(OpCodes.Stloc, local.builder);
            //Console.WriteLine($"il.Emit(OpCodes.Stloc, {local.node!.Name});");
        }
        else if (field is not null)
        {
            il.Emit(OpCodes.Stfld, field);
            //Console.WriteLine($"il.Emit(OpCodes.Stfld, {field.Name});");
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
                //Console.WriteLine(@"il.AddObjectToStack(literal.Value);");
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
                //Console.WriteLine($"il.Emit(OpCodes.Ldc_I4, {(int)x});");
            } 
        },
        { 
            typeof(string), (il, x) =>
            {
                il.Emit(OpCodes.Ldstr, x.ToString() ?? "");
                //Console.WriteLine($"il.Emit(OpCodes.Ldstr, {x} ??\"\");");
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
            //Console.WriteLine(@"il.AddObjectToStack(literal.Value);");
        }
    }

    private void VisitReturnStatement(ReturnStatementNode node, NodeVisitOptions? options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.IL);
        ILGenerator il = options.IL;

        node.ValueAccessor.Accept(this, options);
        il.Emit(OpCodes.Ret);
        //Console.WriteLine(@"il.Emit(OpCodes.Ret);");
    }
}