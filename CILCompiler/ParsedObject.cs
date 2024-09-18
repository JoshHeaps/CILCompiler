namespace CILCompiler;

/// <summary>
/// Mostly a helper object for easy tuple management.
/// </summary>
public class ParsedObject
{
    public string Name { get; set; }
    public List<string> Code { get; set; }

    public ParsedObject(string name, List<string> code)
    {
        Name = name;
        Code = code;
    }

    public static implicit operator ParsedObject((string name, List<string> code) tuple) => new(tuple.name, tuple.code);
    public static implicit operator (string name, List<string> code)(ParsedObject unreadObject) => (unreadObject.Name, unreadObject.Code);
}
