namespace TodoAPI.DataTransfer;

public sealed record LoginUserResponse(
    int Id,
    string Email,
    string Name,
    string Token
);