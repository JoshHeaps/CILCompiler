namespace CILCompiler;

public static class FileReader
{
    private static readonly char[] endingCharacters = { '{', '}', ';' };

    public static string ReadFile(string path) =>
        string.Concat(File.ReadAllLines(path)
            .RemoveSingleLineComments()
            .Select(x => x.Trim())
            .ConcatToSpecialCharacter());

    private static IEnumerable<string> RemoveSingleLineComments(this IEnumerable<string> lines) => lines.Select(x => x.Contains("//") ? x[..x.IndexOf("//")] : x);

    private static List<string> ConcatToSpecialCharacter(this IEnumerable<string> lines)
    {
        var list = lines.ToList();

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Count(x => endingCharacters.Contains(x)) > 1)
            {
                List<char> characters = list[i].Where(x => endingCharacters.Contains(x)).ToList();

                var splits = list[i].Split(endingCharacters).ToList();

                list.InsertRange(i + 1, list[i].Split(endingCharacters)[..^1].Select((x, i) => x + characters[i]));
                list.RemoveAt(i);
            }

            while (!endingCharacters.Any(x => list[i].EndsWith(x)) && i < list.Count - 1)
            {
                list[i] += list[i + 1];
                list.RemoveAt(i + 1);
            }
        }

        return list;
    }

    #region ClassHelpers
    public static List<ParsedObject> SeperateObjects(this List<string> lines)
    {
        List<ParsedObject> objects = [];
        ParsedObject currentObject = ("", []);

        List<int> startDepths = [];
        int currentDepth = 0;
        List<string> openObjects = [];

        foreach (var line in lines)
        {
            if (line.StartsWith("class") && line.EndsWith('{'))
            {
                startDepths.Add(currentDepth);
                openObjects.Add(line.Split(' ')[1][..^1]);
                currentObject = (openObjects.Last(), []);
                objects.Add(currentObject);
            }

            if (line.EndsWith('{'))
                currentDepth++;
            else if (line.EndsWith('}'))
                currentDepth--;

            currentObject.Code.Add(line);

            if (currentDepth == startDepths.Last())
            {
                openObjects.RemoveAt(openObjects.Count - 1);

                currentObject = GetCurrentObject(objects, openObjects);
            }
        }

        return objects;
    }

    private static ParsedObject GetCurrentObject(List<ParsedObject> objects, List<string> openObjects)
    {
        if (openObjects.Count == 0)
            return ("", []);

        return objects.First(x => x.Name == openObjects[^1]);
    }
    #endregion ClassHelpers
}