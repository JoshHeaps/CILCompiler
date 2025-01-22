using CILCompiler.ASTNodes;
using CILCompiler.ASTNodes.Implementations;
using CILCompiler.ASTNodes.Implementations.Expressions;
using CILCompiler.ASTNodes.Implementations.FlowControllers;
using CILCompiler.ASTNodes.Interfaces;
using CILCompiler.Utilities;
using System.Numerics;

namespace CILCompiler.Tokens;

public class Parser
{
    private readonly Lexer _lexer;
    private Token _currentToken;
    private string _name;
    private readonly List<IFieldNode> _fields;
    private readonly List<IMethodNode> _methods;
    private readonly List<(MethodCallNode node, int fixIndex)> _methodCallPlaceholders;
    private readonly List<Action<MethodNode>> _fixMethodCallDelegates;

    public Parser(Lexer lexer)
    {
        _name = "";
        _lexer = lexer;
        _currentToken = _lexer.Advance();
        _fields = [];
        _methods = [];
        _methodCallPlaceholders = [];
        _fixMethodCallDelegates = [];
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
        _name = ParseIdentifier(); // class name

        ParseFields();

        ParseMethods();

        FixMethodCalls();

        return new(_name, _fields, _methods);
    }

    private void ParseFields()
    {
        // Parse fields first, so method bodies can access them.
        int depth = 0;

        while (_currentToken.Type != TokenType.EndOfFile)
        {
            Eat(TokenType.Any);

            if (_currentToken.Value == "{")
                depth++;
            else if (_currentToken.Value == "}")
                depth--;

            bool currentTokenCanBeField = _currentToken.Type == TokenType.Type && depth == 0;

            if (!currentTokenCanBeField)
                continue;

            bool isCurrentTokenMethod = PeekNextToken().Type == TokenType.Parenthesis;
            bool isCurrentTokenParameter = PeekNextToken().Type == TokenType.Comma;
            bool isField = !isCurrentTokenMethod && !isCurrentTokenParameter;

            if (isField)
                _fields.Add(ParseField());
        }

        _lexer.Position = 0;
        _currentToken = _lexer.Advance();
        Eat(TokenType.Keyword);
        Eat(TokenType.Identifier);
    }

    private void ParseMethods()
    {
        int depth = 0;

        while (_currentToken.Type != TokenType.EndOfFile)
        {
            if (_currentToken.Type != TokenType.Type)
                Eat(TokenType.Any);

            if (_currentToken.Value == "{")
                depth++;
            else if (_currentToken.Value == "}")
                depth--;

            bool currentTokenCanBeField = _currentToken.Type == TokenType.Type && depth == 0;

            if (!currentTokenCanBeField)
            {
                Eat(TokenType.Any);

                continue;
            }

            bool isCurrentTokenMethod = PeekNextToken().Type == TokenType.Parenthesis;

            if (isCurrentTokenMethod)
                _methods.Add(ParseMethod());
            else
                Eat(TokenType.Any);
        }

        _lexer.Position = 0;
        _currentToken = _lexer.Advance();
        Eat(TokenType.Keyword);
        Eat(TokenType.Identifier);
    }

    private void FixMethodCalls()
    {
        for (int i = 0; i < _methodCallPlaceholders.Count; i++)
        {
            if (Definitions.DefaultMethods.ContainsKey(_methodCallPlaceholders[i].node.MethodNode.Name))
                continue;

            var method = _methods.First(x => x.Name == _methodCallPlaceholders[i].node.MethodNode.Name);
            var callNode = _methodCallPlaceholders[i].node;
            callNode.MethodNode = method;
            _methodCallPlaceholders[i] = (callNode, _methodCallPlaceholders[i].fixIndex);
            _fixMethodCallDelegates[_methodCallPlaceholders[i].fixIndex].Invoke((method as MethodNode)!);
        }
    }

    private FieldNode ParseField()
    {
        var type = ParseType(); // int, string
        var name = ParseIdentifier(); // foo
        Eat(TokenType.Equals); // =

        var expression = ParseExpression(type);

        return new(type, name, expression);
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
        var body = ParseMethodBody(type, parameters, []);
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

    private List<IExpressionNode> ParseMethodBody(Type type, List<IParameterNode> parameters, List<ILocalVariableNode> outerLocals)
    {
        // Simplified parsing of method body, assuming it's a list of expressions
        var body = new List<IExpressionNode>();
        var locals = new List<ILocalVariableNode>();

        while (_currentToken.Type != TokenType.Brace)
        {
            if (_currentToken.Type == TokenType.If)
                body.Add(ParseIfStatement(type, parameters, [.. locals, .. outerLocals]));

            else if (_currentToken.Type == TokenType.FlowControl && _currentToken.Value == "while")
                body.Add(ParseWhileLoop(type, parameters, [.. locals, .. outerLocals]));

            else if (_currentToken.Type == TokenType.Type && PeekNextToken().Type == TokenType.Equals)
                body.Add(ParseLocalDeclaration(parameters, [.. locals, .. outerLocals]));

            else if (_currentToken.Type == TokenType.Identifier && PeekNextToken().Type == TokenType.Equals)
                body.Add(ParseValueAssignment(parameters, [.. locals, .. outerLocals]));

            else if (_currentToken.Type == TokenType.Identifier
                && (PeekNextToken().Type == TokenType.Parenthesis || NextTypesAre(TokenType.Dot, TokenType.Identifier, TokenType.Parenthesis)))
                body.Add(ParseMethodCall(parameters, [.. locals, .. outerLocals]));

            else if (_currentToken.Type == TokenType.Return)
                body.Add(ParseReturnStatement(type, parameters, [.. locals, .. outerLocals]));

            else if (_currentToken.Type == TokenType.Identifier && NextTypesAre(TokenType.ArithmeticOperator, TokenType.ArithmeticOperator))
                body.Add(ParseIncrement(parameters, [.. locals, .. outerLocals]));

            if (body.Count > 0 && body[^1] is ILocalVariableNode local)
                locals.Add(local);

            if (body[^1] is not IFlowControllerNode)
                Eat(TokenType.Semicolon);
        }

        return body;
    }

    private IfStatementNode ParseIfStatement(Type type, List<IParameterNode> parameters, List<ILocalVariableNode> locals)
    {
        Eat(TokenType.If);
        Eat(TokenType.Parenthesis);
        var condition = ParseBinaryExpression(typeof(bool), parameters, locals);
        Eat(TokenType.Parenthesis);
        Eat(TokenType.Brace);
        var body = ParseMethodBody(type, parameters, locals);
        Eat(TokenType.Brace);

        if (_currentToken != (TokenType.FlowControl, "else"))
            return new IfStatementNode(condition, body, []);

        Eat(TokenType.FlowControl);
        Eat(TokenType.Brace);

        var elseBody = ParseMethodBody(type, parameters, locals);

        Eat(TokenType.Brace);

        return new IfStatementNode(condition, body, elseBody);
    }

    private WhileLoopNode ParseWhileLoop(Type type, List<IParameterNode> parameters, List<ILocalVariableNode> locals)
    {
        Eat(TokenType.FlowControl);
        Eat(TokenType.Parenthesis);
        var condition = ParseBinaryExpression(type, parameters, locals);
        Eat(TokenType.Parenthesis);
        Eat(TokenType.Brace);
        var body = ParseMethodBody(type, parameters, locals);
        Eat(TokenType.Brace);

        return new(condition, body);
    }

    private PredicateNode ParsePredicate(List<IParameterNode> parameters, List<ILocalVariableNode> locals, bool operationCheck = true)
    {
        var left = ParseExpression(null, parameters, locals, operationCheck, false);

        if (left is PredicateNode predicateNode)
            return predicateNode; // I'll fix this later (he said knowing he wouldn't)

        string Operator;

        if (_currentToken.Type == TokenType.Equals)
        {
            Operator = "==";

            Eat(TokenType.Equals);
            Eat(TokenType.Equals);
        }
        else
        {
            Operator = _currentToken.Value;

            Eat(TokenType.Comparer);

            if (_currentToken.Type == TokenType.Equals)
            {
                Operator += "=";

                Eat(TokenType.Equals);
            }
        }

        var right = ParseExpression(null, parameters, locals, operationCheck);

        return new(left, right, Operator);
    }

    private MethodCallNode ParseMethodCall(List<IParameterNode> parameters, List<ILocalVariableNode> locals, string? objectName = null, string? methodName = null)
    {
        var tokens = PeekUntil(TokenType.Parenthesis);
        objectName ??= _name;

        if (tokens.Any(x => x.Type == TokenType.Dot))
        {
            objectName = ParseIdentifier();
            Eat(TokenType.Dot);
        }

        methodName ??= ParseIdentifier();

        Eat(TokenType.Parenthesis);
        List<IValueAccessorNode> valueAccessors = [];

        while(_currentToken.Type == TokenType.Identifier || _currentToken.Type == TokenType.QuotationMark)
        {
            valueAccessors.Add(new ValueAccessorNode(ParseExpression(null, parameters, locals)));

            if (_currentToken.Type == TokenType.Comma)
                Eat(TokenType.Comma);
        }

        Eat(TokenType.Parenthesis);

        var dummyMethod = new MethodNode(methodName, typeof(object), [], []);
        var result = new MethodCallNode(dummyMethod, valueAccessors);

        _methodCallPlaceholders.Add((result, _methodCallPlaceholders.Count));
        _fixMethodCallDelegates.Add(method =>
        {
            result.MethodNode = method;
        });

        return result;
    }

    private ReturnStatementNode ParseReturnStatement(Type type, List<IParameterNode> parameters, List<ILocalVariableNode> locals)
    {
        Eat(TokenType.Return);

        if (_currentToken.Type == TokenType.Semicolon)
            return new(new ValueAccessorNode(null));

        return new(new ValueAccessorNode(ParseExpression(type, parameters, locals)));
    }

    private IExpressionNode ParseExpression(Type? type = null, List<IParameterNode>? parameters = null, List<ILocalVariableNode>? locals = null, bool calculationCheck = true, bool logicCheck = true)
    {
        type ??= typeof(object);

        if (calculationCheck || logicCheck)
        {
            var tokenType = CheckOperationType();

            if (calculationCheck && tokenType == TokenType.ArithmeticOperator)
            {
                return ParseCalculation(type, parameters, locals);
            }
            else if (logicCheck && tokenType == TokenType.LogicalOperator)
            {
                return ParseBinaryExpression(type, parameters, locals);
            }
        }
        
        if (_currentToken.Type == TokenType.QuotationMark)
            return ParseValue(typeof(string), parameters, locals);

        var identifier = ParseIdentifier();

        if (parameters is not null && parameters.Any(x => x.Name == identifier))
            return parameters.First(x => x.Name == identifier);
        else if (locals is not null && locals.Any(x => x.Name == identifier))
            return locals.First(x => x.Name == identifier);
        else if (_fields is not null && _fields.Any(x => x.Name == identifier))
            return _fields.First(x => x.Name == identifier);
        else if ((_currentToken.Type == TokenType.Parenthesis && _currentToken.Value == "(")
              || (_currentToken.Type == TokenType.Dot && NextTypesAre(TokenType.Identifier, TokenType.Parenthesis)))
        {
            if (_currentToken.Type == TokenType.Parenthesis)
                return ParseMethodCall(parameters!, locals!, methodName: identifier);

            return ParseMethodCall(parameters!, locals!, objectName: identifier);
        }

        if (int.TryParse(identifier, out _))
            type = typeof(int);

        return new LiteralNode(typeConversions[type](identifier));
    }

    private TokenType CheckOperationType()
    {
        var token = _currentToken;
        var currentIndex = _lexer.Position;
        _ = ParseExpression(calculationCheck: false, logicCheck: false);
        var nextToken = _currentToken;
        _lexer.Position = currentIndex;
        _currentToken = token;

        return nextToken.Type;
    }

    private AssignmentNode ParseIncrement(List<IParameterNode> parameters, List<ILocalVariableNode> locals)
    {
        string identifier = ParseIdentifier();
        var type = locals.FirstOrDefault(x => x.Name == identifier)?.Type
            ?? parameters.FirstOrDefault(x => x.Name == identifier)?.Type
            ?? _fields.FirstOrDefault(x => x.Name == identifier)?.Type
            ?? throw new InvalidProgramException("Identifier doesn't exist in this context.");

        if (!type.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(INumber<>))))
            throw new InvalidProgramException("Cannot increment a non numeric type");

        IExpressionNode expression;

        if (parameters is not null && parameters.Any(x => x.Name == identifier))
            expression = parameters.First(x => x.Name == identifier);
        else if (locals is not null && locals.Any(x => x.Name == identifier))
            expression = locals.First(x => x.Name == identifier);
        else if (_fields is not null && _fields.Any(x => x.Name == identifier))
            expression = _fields.First(x => x.Name == identifier);
        else
            throw new InvalidProgramException("Identifier doesn't exist in this context.");

        string @operator = ParseArithmeticOperator();
        Eat(TokenType.ArithmeticOperator);

        return new(type, identifier, new ValueAccessorNode(new CalculationNode(expression, new LiteralNode(1), @operator)));
    }

    private AssignmentNode ParseValueAssignment(List<IParameterNode> parameters, List<ILocalVariableNode> locals)
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
        if (_currentToken.Type == TokenType.Identifier
         && (PeekNextToken().Value == "(" || NextTypesAre(TokenType.Dot, TokenType.Identifier, TokenType.Parenthesis)))
        {
            ArgumentNullException.ThrowIfNull(parameters);
            ArgumentNullException.ThrowIfNull(locals);

            return ParseMethodCall(parameters, locals);
        }
        else if (type == typeof(bool) && PeekNextToken().Type == TokenType.Comparer || PeekNextToken().Type == TokenType.Equals)
        {
            ArgumentNullException.ThrowIfNull(parameters);
            ArgumentNullException.ThrowIfNull(locals);

            return ParsePredicate(parameters, locals);
        }
        
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

    private LocalVariableNode ParseLocalDeclaration(List<IParameterNode> parameters, List<ILocalVariableNode> locals)
    {
        int declaredPosition = _lexer.Position;
        var type = ParseType();
        var name = ParseIdentifier();

        Eat(TokenType.Equals);

        IExpressionNode? expression = ParseExpression(type, parameters, locals);

        return new LocalVariableNode(type, name, new ValueAccessorNode(expression), declaredPosition);
    }

    private readonly Dictionary<Type, Func<string, object>> typeConversions = new()
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

    private readonly Dictionary<string, int> _operatorPriorities = new()
    {
        { "+", 0 },
        { "-", 0 },
        { "*", 1 },
        { "/", 1 },
        { "%", 1 },
    };

    private IExpressionNode ParseBinaryExpression(Type? type = null, List<IParameterNode>? parameters = null, List<ILocalVariableNode>? locals = null)
    {
        type ??= typeof(bool);

        List<IExpressionNode> values = [];
        List<string> operators = [];
        
        values.Add(ParseValue(typeof(bool), parameters, locals));

        while (_currentToken.Type == TokenType.LogicalOperator || _currentToken.Type == TokenType.Equals)
        {
            operators.Add(ParseLogicalOperator());
            values.Add(ParseValue(typeof(bool), parameters, locals));
        }

        return PrioritizeLogicalOperations(values, operators);
    }

    private IExpressionNode PrioritizeLogicalOperations(List<IExpressionNode> values, List<string> operators)
    {
        if (values.Count == 2)
            return new BinaryExpressionNode(values[0], values[1], operators[0]);

        if (values.Count == 1)
            return values[0];

        var lowestPriorityIndex = operators.LastIndexOf(operators.MinBy(x => _operatorPriorities[x])!);

        var left = values[0];
        var @operator = operators[0];
        var right = PrioritizeLogicalOperations(values[1..], operators[1..]);

        return new BinaryExpressionNode(left, right, @operator);
    }
    

    private IExpressionNode ParseCalculation(Type? type = null, List<IParameterNode>? parameters = null, List<ILocalVariableNode>? locals = null)
    {
        type ??= typeof(object);

        List<IExpressionNode> values = [];
        List<string> operators = [];

        if (int.TryParse(_currentToken.Value, out _))
            type = typeof(int);

        values.Add(ParseValue(type, parameters, locals));

        while (_currentToken.Type == TokenType.ArithmeticOperator)
        {
            operators.Add(ParseArithmeticOperator());

            if (int.TryParse(_currentToken.Value, out _))
                type = typeof(int);

            values.Add(ParseValue(type, parameters, locals));
        }

        return PrioritizeOperations(values, operators);
    }

    private IExpressionNode PrioritizeOperations(List<IExpressionNode> values, List<string> operators)
    {
        if (values.Count == 2)
            return new CalculationNode(values[0], values[1], operators[0]);

        if (values.Count == 1)
            return values[0];

        var lowestPriorityIndex = operators.LastIndexOf(operators.MinBy(x => _operatorPriorities[x])!);

        var leftValues = values[..(lowestPriorityIndex + 1)];
        var leftOperators = operators.Take(lowestPriorityIndex).ToList();
        var rightValues = values[(lowestPriorityIndex + 1)..];
        var rightOperators = operators.TakeLast(operators.Count - leftOperators.Count - 1).ToList();

        IExpressionNode left = PrioritizeOperations(leftValues, leftOperators);
        string Operator = operators[lowestPriorityIndex];
        IExpressionNode right = PrioritizeOperations(rightValues, rightOperators);

        return new CalculationNode(left, right, Operator);
    }

    private string ParseArithmeticOperator()
    {
        if (_currentToken.Type != TokenType.ArithmeticOperator)
            throw new Exception("Expected arithmetic operator");

        var op = _currentToken.Value;
        Eat(TokenType.ArithmeticOperator);

        return op;
    }

    private string ParseLogicalOperator()
    {
        if (_currentToken.Type != TokenType.LogicalOperator)
            throw new Exception("Expected logical operator");

        var op = _currentToken.Value;
        Eat(TokenType.LogicalOperator);

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

    private List<Token> PeekUntil(params TokenType[] untilTypes)
    {
        var savedPosition = _lexer.Position;  // save the current position
        List<Token> tokens = [_currentToken,_lexer.Advance()];

        while (!untilTypes.Contains(tokens[^1].Type))
            tokens.Add(_lexer.Advance());

        _lexer.Position = savedPosition;  // restore the position

        return tokens;
    }

    private bool NextTypesAre(params TokenType[] types)
    {
        var savedPosition = _lexer.Position;  // save the current position

        bool result = !types.Any(type => _lexer.Advance().Type != type);

        _lexer.Position = savedPosition;  // restore the position

        return result;
    }
}
