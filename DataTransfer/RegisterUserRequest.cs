namespace TodoAPI.DataTransfer;

public sealed record RegisterUserRequest(
    string Email,
    string Name,
    string Password
);