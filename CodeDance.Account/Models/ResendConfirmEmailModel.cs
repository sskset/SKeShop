using FluentValidation;

namespace CodeDance.Account.Models
{
    public class ResendConfirmEmailModel
    {
        public string Email { get; set; }
    }

    public class ResendConfirmEmailModelValidator : AbstractValidator<ResendConfirmEmailModel>
    {
        public ResendConfirmEmailModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotNull();
        }
    }
}
