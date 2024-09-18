namespace CILCompiler.Utilities;

//
// Summary:
//     Represents an option type that can either have a value (Some) or not (None).
//
//
// Type parameters:
//   T:
//     The type of the value.
public class Optional<T>
{
    private readonly T? value;

    private readonly bool isSome;

    internal Optional(T t)
    {
        isSome = true;
        value = t;
    }

    internal Optional()
    {
        isSome = false;
    }

    //
    // Summary:
    //     Matches the option and executes the corresponding function.
    //
    // Parameters:
    //   None:
    //     The function to execute if the option is COPA.Models.Functional.None``1.
    //
    //   Some:
    //     The function to execute if the option is COPA.Models.Functional.Some``1(``0).
    //
    //
    // Type parameters:
    //   R:
    //     The type of the result.
    //
    // Returns:
    //     The result of the executed function.
    public R Match<R>(Func<R> None, Func<T, R> Some)
    {
        if (!isSome)
        {
            return None();
        }

        return Some(value!);
    }
}