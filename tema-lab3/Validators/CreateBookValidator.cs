using FluentValidation;
using tema_lab3.Features.Books;

namespace tema_lab3.Validators;

public class CreateBookValidator : AbstractValidator<CreateBookRequest>
{
    public CreateBookValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");
        
        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required")
            .MaximumLength(100).WithMessage("Author must not exceed 100 characters");
        
        RuleFor(x => x.Year)
            .InclusiveBetween(1000, 2100).WithMessage("Year must be between 1000 and 2100");
    }
}