using CILCompiler.Tokens;

public record Token(TokenType Type, string Value)
{
    public static implicit operator (TokenType type, string value)(Token token) => (token.Type, token.Value);
    public static implicit operator Token((TokenType type, string value) tuple) => new(tuple.type, tuple.value);
}