using FluentValidation;

namespace CodeDance.Account.Models
{
    public class SendResetPasswordEmailModel
    {
        public string Email { get; set; }
    }

    public class SendResetPasswordEmailModelValidator : AbstractValidator<SendResetPasswordEmailModel>
    {
        public SendResetPasswordEmailModelValidator()
        {
            RuleFor(x => x.Email).NotNull().NotEmpty().EmailAddress();
        }
    }
}
