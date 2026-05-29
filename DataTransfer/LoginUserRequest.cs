namespace TodoAPI.DataTransfer;

public sealed record LoginUserRequest(
    string Email,
    string Password
);