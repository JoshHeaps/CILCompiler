namespace CILCompiler.Tokens;

public class Lexer
{
    public int Position { get; set; }
    private readonly string _source;

    private static readonly Dictionary<string, TokenType> KeywordTypes = new()
    {
        { "class", TokenType.Keyword },
        { "if", TokenType.FlowControl },
        { "while", TokenType.FlowControl },
        { "object", TokenType.Type },
        { "int", TokenType.Type },
        { "long", TokenType.Type },
        { "float", TokenType.Type },
        { "double", TokenType.Type },
        { "bool", TokenType.Type },
        { "string", TokenType.Type },
        { "void", TokenType.Type },
        { "return", TokenType.Return },
    };

    private static readonly Dictionary<char, Token> SymbolTokens = new()
    {
        { '{', new (TokenType.Brace, "{") },
        { '}', new (TokenType.Brace, "}") },
        { ',', new (TokenType.Comma, ",") },
        { '.', new (TokenType.Dot, ".") },
        { '=', new (TokenType.Equals, "=") },
        { '+', new (TokenType.Operator, "+") },
        { '-', new (TokenType.Operator, "-") },
        { '(', new (TokenType.Parenthesis, "(") },
        { ')', new (TokenType.Parenthesis, ")") },
        { '"', new (TokenType.QuotationMark, "\"") },
        { ';', new (TokenType.Semicolon, ";") },
    };

    public Lexer(string source)
    {
        _source = source;
        Position = 0;
    }

    private const char END_OF_FILE = '\0';
    private char CurrentChar => Position >= _source.Length ? END_OF_FILE : _source[Position];
    private void NextChar() => Position++;

    public Token Advance()
    {
        while(char.IsWhiteSpace(CurrentChar))
        {
            NextChar();
        }

        if (CurrentChar == END_OF_FILE)
            return new(TokenType.EndOfFile, string.Empty);

        if (char.IsLetter(CurrentChar))
        {
            var value = GetValue(char.IsLetterOrDigit);

            if (KeywordTypes.TryGetValue(value, out var type))
                return new Token(type, value);

            return new(TokenType.Identifier, value);
        }

        if (char.IsDigit(CurrentChar))
        {
            var value = GetValue(char.IsDigit);

            return new(TokenType.Identifier, value);
        }

        if (SymbolTokens.TryGetValue(CurrentChar, out var token))
        {
            NextChar();

            return token;
        }

        NextChar();

        return new(TokenType.Error, string.Empty);
    }

    public Token GetString()
    {
        Predicate<char> predicate = c => c != '"';

        return new(TokenType.Identifier, GetValue(predicate));
    }

    private string GetValue(Predicate<char> condition)
    {
        var start = Position;

        while (condition(CurrentChar)) NextChar();

        return _source[start..Position];
    }
}
