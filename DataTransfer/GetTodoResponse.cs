using TodoAPI.Enums;

namespace TodoAPI.DataTransfer;

public sealed record GetTodoResponse(
    int Id,
    string Details,
    TodoStatus Status
);