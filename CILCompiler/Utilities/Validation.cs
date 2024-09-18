namespace CILCompiler.Utilities;

public record Validation<TResult, TFailure>
{
    public TResult Value { get; }
    public TFailure Failure { get; }
    public bool Succeeded { get; }

    private Validation(TResult value)
    {
        Value = value;
        Succeeded = true;
    }

    public static Validation<TResult, TFailure> Succeed(TResult value) =>
        new(value);

    public static implicit operator Validation<TResult, TFailure>(TResult value) => Succeed(value);

    private Validation(TFailure failure)
    {
        Failure = failure;
        Succeeded = false;
    }

    public static Validation<TResult, TFailure> Fail(TFailure failure) =>
        new(failure);

    public static implicit operator Validation<TResult, TFailure>(TFailure value) => Fail(value);

    public T Match<T>(Func<TResult, T> onSuccess, Func<TFailure, T> onFailure) =>
        Succeeded ? onSuccess(Value) : onFailure(Failure);

    public void Match(Action<TResult> onSuccess, Action<TFailure> onFailure)
    {
        if (Succeeded)
            onSuccess(Value);
        else
            onFailure(Failure);
    }

    public Validation<T, TFailure> Bind<T>(Func<TResult, Validation<T, TFailure>> onSuccess) =>
        Succeeded ? onSuccess(Value) : Validation<T, TFailure>.Fail(Failure);

    public Validation<T, TFailure> Map<T>(Func<TResult, T> func) =>
        Succeeded ? Validation<T, TFailure>.Succeed(func(Value)) : Validation<T, TFailure>.Fail(Failure);
}