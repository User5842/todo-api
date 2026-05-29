using TodoAPI.Enums;

namespace TodoAPI.DataTransfer;

public sealed record CreateTodoRequest(
    string Details,
    TodoStatus Status
);