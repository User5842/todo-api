namespace TodoAPI.DataTransfer;

public sealed record RegisterUserResponse(
    int Id,
    string Email,
    string Name
);