using CILCompiler;
using CILCompiler.ASTNodes.Implementations;
using CILCompiler.ASTVisitors.Implementations;
using CILCompiler.Tokens;

//var printStatement = new PrintStatementNode
//(
//    new BinaryExpressionNode
//    (
//        new LiteralNode(5),  // Left operand
//        new LiteralNode(3),  // Right operand
//        "+"                  // Operator
//    )
//);

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

obj.Accept(printer);
;