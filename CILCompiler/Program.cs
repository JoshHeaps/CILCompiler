using CILCompiler;
using CILCompiler.ASTVisitors.Implementations;
using CILCompiler.Tokens;

string filePath = Environment.ExpandEnvironmentVariables(@"%userprofile%\Desktop\class foo.jh.txt");

string src = FileReader.ReadFile(filePath);

var lexer = new Lexer(src);
var parser = new Parser(lexer);
var obj = parser.Parse();
var generator = new ILCreationVisitor();

var type = generator.CompileObject([obj])[0].CreateType();
var bar = Activator.CreateInstance(type);
Console.WriteLine(type.GetMethod("Main")?.Invoke(bar, [5, 2, 1]) ?? throw new InvalidProgramException("Project has no suitable entry point"));