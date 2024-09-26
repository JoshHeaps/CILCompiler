using CILCompiler;
using CILCompiler.ASTNodes.Implementations;
using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTVisitors.Implementations;
using CILCompiler.Tokens;
using System.Reflection;
using System.Reflection.Metadata;
using Mono.Cecil;

var printStatement = new PrintStatementNode
(
    new BinaryExpressionNode
    (
        new LiteralNode(5),  // Left operand
        new LiteralNode(3),  // Right operand
        "+"                  // Operator
    )
);

//var stringBuilder = new StringBuilderVisitor();
//var printer = new PrintVisitor();

//Console.WriteLine(printStatement.Accept(stringBuilder));
//printStatement.Accept(printer);

string filePath = Environment.ExpandEnvironmentVariables(@"%userprofile%\Desktop\class foo.jh.txt");
//var fileContents = FileReader.ReadFile(filePath).ToList();

//Console.WriteLine(string.Join("\r\n", fileContents.SelectMany(x => x.Code)));

string src = FileReader.ReadFile(filePath);

var lexer = new Lexer(src);
var parser = new Parser(lexer);
var obj = parser.Parse();
var printer = new PrintVisitor();
var generator = new ILCreationVisitor();

obj.Accept(printer);
var type = generator.CompileObject([obj])[0].CreateType();
var bar = Activator.CreateInstance(type);
Console.WriteLine(type.GetMethod("Main").Invoke(bar, [5, 2, 1]));

//Mono.Cecil.AssemblyDefinition assemblyDefinition = Mono.Cecil.AssemblyDefinition.ReadAssembly();
;