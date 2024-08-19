using FluentValidation;

namespace CodeDance.Account.Models
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class LoginModelValidator : AbstractValidator<LoginModel>
    {
        public LoginModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(6);
        }
    }
}
