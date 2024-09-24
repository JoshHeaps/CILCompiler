using CILCompiler.ASTNodes;
using CILCompiler.ASTNodes.Implementations;
using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.Utilities;
using System.Linq.Expressions;

namespace CILCompiler.Tokens;

public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;
    private List<IFieldNode> _fields;
    private List<IMethodNode> _methods;

    public Parser(Lexer lexer)
    {
        _lexer = lexer;
        _currentToken = _lexer.Advance();
        _fields = [];
        _methods = [];
    }

    private void Eat(TokenType type)
    {
        if (_currentToken.Type != type && type != TokenType.Any)
            throw new Exception($"Expected token {type} but got {_currentToken.Type}");

        _currentToken = _lexer.Advance();
    }

    public ObjectNode Parse()
    {
        Eat(TokenType.Keyword); // class
        var name = ParseIdentifier(); // class name
        Eat(TokenType.Brace); // "{"

        // Parse fields first, so method bodies can access them.
        int depth = 0;

        while (_currentToken.Type != TokenType.EndOfFile)
        {
            while (_currentToken.Type == TokenType.Type && depth == 0)
            {
                if (PeekNextToken().Type != TokenType.Parenthesis)
                    _fields.Add(ParseField());
                else
                    break;
            }

            if (_currentToken.Value == "{")
                depth++;
            else if (_currentToken.Value == "}")
                depth--;

            Eat(TokenType.Any); // "}", ";" both of those are options, as stray semicolons are allowed.
        }

        _lexer.Position = 0;
        _currentToken = _lexer.Advance();

        while (_currentToken.Type != TokenType.EndOfFile)
        {
            while (_currentToken.Type == TokenType.Type)
            {
                if (PeekNextToken().Type == TokenType.Parenthesis)
                    _methods.Add(ParseMethod());
                else
                    break;
            }

            Eat(TokenType.Any); // "}", ";" both of those are options, as stray semicolons are allowed.
        }

        return new(name, _fields, _methods);
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

        var expression = ParseExpression(type);

        Eat(TokenType.Semicolon); // ";"
        return new(name, expression);
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
        var body = ParseMethodBody(type, parameters);
        Eat(TokenType.Brace); // "}"

        return new(name, type, body, parameters);
    }

    private List<IParameterNode> ParseParameters()
    {
        var parameters = new List<IParameterNode>();

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

    private List<IExpressionNode> ParseMethodBody(Type type, List<IParameterNode> parameters)
    {
        // Simplified parsing of method body, assuming it's a list of expressions
        var body = new List<IExpressionNode>();
        List<ILocalVariableNode> locals = [];

        while (_currentToken.Type != TokenType.Brace)
        {
            if (_currentToken.Type == TokenType.Type && PeekNextToken().Type == TokenType.Equals)
                body.Add(ParseLocalDeclaration(locals, parameters));

            else if (_currentToken.Type == TokenType.Identifier && PeekNextToken().Type == TokenType.Equals)
                body.Add(ParseValueAssignment(locals, parameters));

            else if (_currentToken.Type == TokenType.Return)
                body.Add(ParseReturnStatement(type, locals, parameters));

            if (body.Count > 0 && body[^1] is ILocalVariableNode local)
                locals.Add(local);

            Eat(TokenType.Semicolon);
        }

        return body;
    }

    private ReturnStatementNode ParseReturnStatement(Type type, List<ILocalVariableNode> locals, List<IParameterNode> parameters)
    {
        Eat(TokenType.Return);

        return new(new ValueAccessorNode(ParseExpression(type, parameters, locals)));
    }

    private IExpressionNode ParseExpression(Type? type = null, List<IParameterNode>? parameters = null, List<ILocalVariableNode>? locals = null)
    {
        if (PeekUntil(TokenType.Semicolon).Any(x => x.Type == TokenType.Operator))
            return ParseBinaryOperation(type, parameters, locals);

        if (_currentToken.Type == TokenType.QuotationMark && type == typeof(string))
            return ParseValue(type, parameters, locals);

        IExpressionNode? expression = null;
        type ??= typeof(object);

        var identifier = ParseIdentifier();

        if (parameters is not null && parameters.Any(x => x.Name == identifier))
            expression = parameters.First(x => x.Name == identifier);
        else if (locals is not null && locals.Any(x => x.Name == identifier))
            expression = locals.First(x => x.Name == identifier);
        else if (_fields is not null && _fields.Any(x => x.Name == identifier))
            expression = _fields.First(x => x.Name == identifier);
        else
            expression = new LiteralNode(typeConversions[type](identifier));

        return expression;
    }

    private AssignmentNode ParseValueAssignment(List<ILocalVariableNode> locals, List<IParameterNode> parameters)
    {
        string name = ParseIdentifier();
        var type = locals.FirstOrDefault(x => x.Name == name)?.Type
            ?? parameters.FirstOrDefault(x => x.Name == name)?.Type
            ?? _fields.FirstOrDefault(x => x.Name == name)?.Type
            ?? throw new InvalidProgramException("Identifier doesn't exist in this context.");

        Eat(TokenType.Equals);
        var tokens = PeekUntil(TokenType.Semicolon);
        IExpressionNode? expression = ParseExpression(type, parameters, locals);

        return new(type, name, new ValueAccessorNode(expression));
    }

    private IExpressionNode ParseValue(Type? type = null, List<IParameterNode>? parameters = null, List<ILocalVariableNode>? locals = null)
    {
        type ??= typeof(object);

        IExpressionNode? expression = null;
        var identifier = string.Empty;

        if (type == typeof(string) && _currentToken.Type == TokenType.QuotationMark)
        {
            identifier += _lexer.GetString().Value;

            Eat(TokenType.QuotationMark);
            Eat(TokenType.QuotationMark);

            return new LiteralNode(typeConversions[type](identifier));
        }

        identifier = ParseIdentifier();

        if (parameters is not null && parameters.Any(x => x.Name == identifier))
            expression = parameters.First(x => x.Name == identifier);
        else if (locals is not null && locals.Any(x => x.Name == identifier))
            expression = locals.First(x => x.Name == identifier);
        else if (_fields is not null && _fields.Any(x => x.Name == identifier))
            expression = _fields.First(x => x.Name == identifier);
        else
            expression = new LiteralNode(typeConversions[type](identifier));

        return expression;
    }

    private ILocalVariableNode ParseLocalDeclaration(List<ILocalVariableNode> locals, List<IParameterNode> parameters)
    {
        int declaredPosition = _lexer.Position;
        var type = ParseType();
        var name = ParseIdentifier();
        Eat(TokenType.Equals);

        IExpressionNode? expression = ParseExpression(type, parameters, locals);

        return new LocalVariableNode(name, new ValueAccessorNode(expression), declaredPosition);
    }

    Dictionary<Type, Func<string, object>> typeConversions = new()
    {
        { typeof(int), value => int.TryParse(value, out int i) ? i : throw new InvalidCastException() },
        { typeof(string), value => value },
        { typeof(bool), value => bool.TryParse(value, out bool b) ? b : throw new InvalidCastException() },
        { typeof(float), value => float.TryParse(value, out float f) ? f : throw new InvalidCastException() },
        { typeof(double), value => double.TryParse(value, out double d) ? d : throw new InvalidCastException() },
        { typeof(long), value => long.TryParse(value, out long l) ? l : throw new InvalidCastException() },
        { typeof(short), value => short.TryParse(value, out short s) ? s : throw new InvalidCastException() },
        { typeof(byte), value => byte.TryParse(value, out byte b) ? b : throw new InvalidCastException() },
        { typeof(char), value => char.TryParse(value, out char c) ? c : throw new InvalidCastException() },
        { typeof(object), value => value },
    };

    private BinaryExpressionNode ParseBinaryOperation(Type? type = null, List<IParameterNode>? parameters = null, List<ILocalVariableNode>? locals = null)
    {
        type ??= typeof(object);

        IExpressionNode left = ParseValue(type, parameters, locals);
        string Operator = ParseOperator();
        IExpressionNode right = ParseValue(type, parameters, locals);

        return new BinaryExpressionNode(left, right, Operator);
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

        if (nextToken.Type == TokenType.Identifier)
            nextToken = _lexer.Advance();

        _lexer.Position = savedPosition;  // restore the position

        return nextToken;
    }

    private List<Token> PeekUntil(TokenType untilType)
    {
        var savedPosition = _lexer.Position;  // save the current position
        List<Token> tokens = [_currentToken,_lexer.Advance()];

        while (tokens[^1].Type != untilType)
            tokens.Add(_lexer.Advance());

        _lexer.Position = savedPosition;  // restore the position

        return tokens;
    }
}
