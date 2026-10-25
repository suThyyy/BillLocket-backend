namespace BillLocket.Application.DTOs;

public sealed class LoginResult
{
    private LoginResult(
        bool succeeded,
        LoginResponse? response,
        string? error)
    {
        Succeeded = succeeded;
        Response = response;
        Error = error;
    }

    public bool Succeeded { get; }

    public LoginResponse? Response { get; }

    public string? Error { get; }

    public static LoginResult Success(LoginResponse response) =>
        new(true, response, null);

    public static LoginResult Failure(string error) =>
        new(false, null, error);
}
