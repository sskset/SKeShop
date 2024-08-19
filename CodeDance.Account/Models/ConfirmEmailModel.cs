using FluentValidation;

namespace CodeDance.Account.Models
{
    public class ConfirmEmailModel
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }

    public class ConfirmEmailModelValidator : AbstractValidator<ConfirmEmailModel>
    {
        public ConfirmEmailModelValidator()
        {
            RuleFor(x=>x.Email).NotEmpty().NotNull();
            RuleFor(x=>x.Token).NotEmpty().NotNull();
        }
    }
}
