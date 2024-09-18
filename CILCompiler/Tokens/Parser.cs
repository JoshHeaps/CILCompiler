using CILCompiler.ASTNodes;
using CILCompiler.ASTNodes.Implementations;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.Utilities;

namespace CILCompiler.Tokens;

public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.Advance();
    }

    private void Eat(TokenType type)
    {
        if (_currentToken.Type == type)
        {
            _currentToken = _lexer.Advance();
        }
        else
        {
            throw new Exception($"Expected token {type} but got {_currentToken.Type}");
        }
    }

    public ObjectNode Parse()
    {
        Eat(TokenType.Keyword); // class
        var name = ParseIdentifier(); // class name
        List<IFieldNode> fields = [];
        List<IMethodNode> methods = [];
        Eat(TokenType.Brace); // "{"

        while (_currentToken.Type == TokenType.Type)
        {
            if (PeekNextToken().Type == TokenType.Parenthesis)
            {
                methods.Add(ParseMethod());
            }
            else
            {
                fields.Add(ParseField());
            }
        }

        Eat(TokenType.Brace); // "}"

        return new(name, fields, methods);
    }

    private readonly Dictionary<Type, Func<string, object>> _converter = new()
    {
        { typeof(int), x => int.Parse(x) },
        { typeof(long), x => long.Parse(x) },
        { typeof(string), x => x },
        { typeof(bool), x => bool.Parse(x) },
    };

    private FieldNode ParseField()
    {
        var type = ParseType(); // int, string
        var name = ParseIdentifier(); // foo
        Eat(TokenType.Equals); // =

        if (type == typeof(string)) // "
            Eat(TokenType.QuotationMark);

        var stringValue = ParseIdentifier(); // 12, apple

        if (type == typeof(string))
            Eat(TokenType.QuotationMark); // "

        var value = _converter[type](stringValue);

        Eat(TokenType.QuotationMark); // ";"
        return new(name, value);
    }

    private Type ParseType()
    {
        if (_currentToken.Type != TokenType.Type)
            throw new Exception("Expected type");

        var type = Definitions.StaticTypes[_currentToken.Value];

        Eat(TokenType.Type);

        return type;
    }

    private MethodNode ParseMethod()
    {
        var type = ParseType();
        var name = ParseIdentifier();
        Eat(TokenType.Parenthesis); // "("
        var parameters = ParseParameters();
        Eat(TokenType.Parenthesis); // ")"
        Eat(TokenType.Brace); // "{"
        var body = ParseMethodBody();
        Eat(TokenType.Brace); // "}"

        return new(name, type, body, []);
    }

    private List<ParameterNode> ParseParameters()
    {
        var parameters = new List<ParameterNode>();

        while (_currentToken.Type == TokenType.Type)
        {
            parameters.Add(new ParameterNode
            (
                ParseType(),
                ParseIdentifier()
            ));

            if (_currentToken.Type == TokenType.Comma)
            {
                Eat(TokenType.Comma); // ","
            }
        }

        return parameters;
    }

    private List<StatementNode> ParseMethodBody()
    {
        // Simplified parsing of method body, assuming it's a list of expressions
        var body = new List<StatementNode>();

        while (_currentToken.Type != TokenType.Brace)
        {
            body.Add(ParseStatement());
        }

        return body;
    }

    private IExpressionNode ParseStatement()
    {
        // For now, we'll handle only basic expressions as method statements
        if (_currentToken.Type != TokenType.Identifier)
            throw new Exception("Unexpected statement");

        var expression = ParseIdentifier() + ParseOperator() + ParseIdentifier();
        Eat(TokenType.Semicolon); // ";"

        return new StatementNode(expression);
    }

    private string ParseOperator()
    {
        if (_currentToken.Type != TokenType.Operator)
            throw new Exception("Expected operator");

        var op = _currentToken.Value;
        Eat(TokenType.Operator);

        return op;
    }

    private string ParseIdentifier()
    {
        if (_currentToken.Type == TokenType.Identifier)
        {
            var value = _currentToken.Value;
            Eat(TokenType.Identifier);

            return value;
        }

        throw new Exception("Expected identifier");
    }

    private Token PeekNextToken()
    {
        var savedPosition = _lexer.Position;  // save the current position
        var nextToken = _lexer.Advance();
        _lexer.Position = savedPosition;  // restore the position

        return nextToken;
    }
}
