using Microsoft.EntityFrameworkCore;
using TodoAPI.Entities;

namespace TodoAPI.Data;

public sealed class TodoContext(DbContextOptions<TodoContext> options) : DbContext(options)
{
    public DbSet<Todo> Todos => Set<Todo>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Email)
            .IsUnique();
    }
}