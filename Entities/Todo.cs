using TodoAPI.Enums;

namespace TodoAPI.Entities;

public sealed class Todo
{
    public required string Details { get; set; }
    public int Id { get; set; }
    public required TodoStatus Status { get; set; }

    public User User { get; set; } = null!;
    public int UserId { get; set; }
}