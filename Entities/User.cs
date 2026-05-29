namespace TodoAPI.Entities;

public sealed class User
{
    public required string Email { get; set; }
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string PasswordHash { get; set; }

    public ICollection<Todo> Todos { get; set; } = [];
}