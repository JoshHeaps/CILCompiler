using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CILCompiler.Utilities;

public static class Definitions
{
    public static readonly Dictionary<string, Type> StaticTypes = new()
    {
        { "int", typeof(int) },
        { "string", typeof(string) },
        { "bool", typeof(bool) },
        { "float", typeof(float) },
        { "double", typeof(double) },
        { "long", typeof(long) },
        { "short", typeof(short) },
        { "byte", typeof(byte) },
        { "char", typeof(char) },
        { "object", typeof(object) },
        { "void", typeof(void) },
    };
}
