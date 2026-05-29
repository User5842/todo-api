using TodoAPI.Enums;

namespace TodoAPI.DataTransfer;

public sealed record CreatedTodoResponse(
    int Id,
    string Details,
    TodoStatus Status
);