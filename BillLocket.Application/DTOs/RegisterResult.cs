namespace BillLocket.Application.DTOs;

public sealed class RegisterResult
{
    private RegisterResult(
        bool succeeded,
        RegisterResponse? response,
        IReadOnlyCollection<string> errors)
    {
        Succeeded = succeeded;
        Response = response;
        Errors = errors;
    }

    public bool Succeeded { get; }

    public RegisterResponse? Response { get; }

    public IReadOnlyCollection<string> Errors { get; }

    public static RegisterResult Success(RegisterResponse response) =>
        new(true, response, Array.Empty<string>());

    public static RegisterResult Failure(IEnumerable<string> errors) =>
        new(false, null, errors.ToArray());
}
