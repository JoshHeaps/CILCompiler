namespace CILCompiler.Utilities;

//
// Summary:
//     Provides functional methods for ease of use.
public static class Functional
{
    //
    // Summary:
    //     Creates an instance of COPA.Models.Optional`1 with the specified value.
    //
    // Parameters:
    //   value:
    //     The value to wrap in the option.
    //
    // Type parameters:
    //   T:
    //     The type of the value.
    //
    // Returns:
    //     An instance of COPA.Models.Optional`1.
    //
    // Exceptions:
    //   T:System.ArgumentNullException:
    //     Thrown when the value is null.
    public static Optional<T> Some<T>(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        return new Optional<T>(value);
    }

    //
    // Summary:
    //     Creates an instance of COPA.Models.Optional`1 with no value.
    //
    // Type parameters:
    //   T:
    //     The type of the value.
    //
    // Returns:
    //     An instance of COPA.Models.Optional`1 with no value.
    public static Optional<T> None<T>()
    {
        return new Optional<T>();
    }
}