namespace VeracodeDemo.Shared;

public sealed record DomainResult<T>(
    bool Success,
    string Module,
    string Message,
    T? Data)
{
    public static DomainResult<T> Ok(string module, string message, T data) =>
        new(true, module, message, data);

    public static DomainResult<T> Fail(string module, string message) =>
        new(false, module, message, default);
}
