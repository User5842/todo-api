using FluentValidation;
using TodoAPI.DataTransfer;

namespace TodoAPI.Validators;

public sealed class CreateTodoRequestValidator : AbstractValidator<CreateTodoRequest>
{
    public CreateTodoRequestValidator()
    {
        RuleFor(todo => todo.Details)
            .NotEmpty()
            .MaximumLength(100);
    }
}