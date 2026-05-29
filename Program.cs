using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TodoAPI.Data;
using TodoAPI.DataTransfer;
using TodoAPI.Entities;
using TodoAPI.Enums;
using TodoAPI.Validators;

var builder = WebApplication.CreateBuilder(args);

var issuer = builder.Configuration["Jwt:Issuer"];
var audience = builder.Configuration["Jwt:Audience"];
var signingKey = builder.Configuration["Jwt:SigningKey"];

if (string.IsNullOrWhiteSpace(issuer) ||
    string.IsNullOrWhiteSpace(audience) ||
    string.IsNullOrWhiteSpace(signingKey))
{
    throw new InvalidOperationException("JWT configuration is missing.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,

            ValidateAudience = true,
            ValidAudience = audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(signingKey)
            ),

            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddDbContext<TodoContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("TodoDb")));

builder.Services.AddScoped<PasswordHasher<User>>();
builder.Services.AddScoped<IValidator<CreateTodoRequest>, CreateTodoRequestValidator>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/todos", async Task<Results<Ok<IReadOnlyList<GetTodoResponse>>, UnauthorizedHttpResult>> (
    ClaimsPrincipal user,
    TodoContext db
) =>
{
    var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!int.TryParse(userIdValue, out var userId))
    {
        return TypedResults.Unauthorized();
    }

    IReadOnlyList<GetTodoResponse> userTodos = await db.Todos
        .Where(todo => todo.UserId == userId)
        .OrderBy(todo => todo.Id)
        .Select(todo => new GetTodoResponse(todo.Id, todo.Details, todo.Status))
        .ToListAsync();

    return TypedResults.Ok(userTodos);
}).RequireAuthorization();

app.MapPost("/todos", async Task<Results<Created<CreatedTodoResponse>, ValidationProblem, UnauthorizedHttpResult>> (
    CreateTodoRequest request,
    ClaimsPrincipal user,
    TodoContext db,
    IValidator<CreateTodoRequest> validator
) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return TypedResults.ValidationProblem(validationResult.ToDictionary());
    }

    var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!int.TryParse(userIdValue, out var userId))
    {
        return TypedResults.Unauthorized();
    }

    var todo = new Todo
    {
        Details = request.Details.Trim(),
        Status = TodoStatus.NotStarted,
        UserId = userId
    };

    db.Todos.Add(todo);
    await db.SaveChangesAsync();

    return TypedResults.Created(
        $"/todos/{todo.Id}",
        new CreatedTodoResponse(todo.Id, todo.Details, todo.Status)
    );

}).RequireAuthorization();

app.MapPost("/login", async Task<Results<Ok<LoginUserResponse>, UnauthorizedHttpResult>> (
    LoginUserRequest request,
    TodoContext db,
    PasswordHasher<User> passwordHasher,
    IConfiguration configuration) =>
{
    var normalizedEmail = request.Email.Trim().ToLowerInvariant();

    var user = await db.Users.FirstOrDefaultAsync(user => user.Email == normalizedEmail);

    if (user is null)
    {
        return TypedResults.Unauthorized();
    }

    var result = passwordHasher.VerifyHashedPassword(
        user,
        user.PasswordHash,
        request.Password
    );

    if (result == PasswordVerificationResult.Failed)
    {
        return TypedResults.Unauthorized();
    }

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString())
    };

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));

    var credentials = new SigningCredentials(
        securityKey,
        SecurityAlgorithms.HmacSha256
    );

    var token = new JwtSecurityToken(
        issuer: issuer,
        audience: audience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: credentials
    );

    var tokenValue = new JwtSecurityTokenHandler().WriteToken(token);

    return TypedResults.Ok(
        new LoginUserResponse(user.Id, user.Email, user.Name, tokenValue)
    );
});

app.MapPost("/register", async Task<Results<Created<RegisterUserResponse>, Conflict>> (
    RegisterUserRequest request,
    TodoContext db,
    PasswordHasher<User> passwordHasher) =>
{
    var normalizedEmail = request.Email.Trim().ToLowerInvariant();

    var emailExists = await db.Users.AnyAsync(user => user.Email == normalizedEmail);

    if (emailExists)
    {
        return TypedResults.Conflict();
    }

    var newUser = new User
    {
        Email = normalizedEmail,
        Name = request.Name.Trim(),
        PasswordHash = string.Empty
    };

    var hashedPassword = passwordHasher.HashPassword(newUser, request.Password);
    newUser.PasswordHash = hashedPassword;

    db.Users.Add(newUser);
    await db.SaveChangesAsync();

    return TypedResults.Created(
        "/users/me",
        new RegisterUserResponse(newUser.Id, newUser.Email, newUser.Name)
    );
});

app.Run();
