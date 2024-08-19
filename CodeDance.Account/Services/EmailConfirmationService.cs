
using CodeDance.EmailSender;
using EmailSender.SendGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace CodeDance.Account.Services
{
    public interface IEmailConfirmationServce
    {
        Task SendConfirmationEmailAsync(string email);
        Task SendConfirmationEmailAsync(IdentityUser user);
    }
    public class EmailConfirmationService : IEmailConfirmationServce
    {
        private readonly IEmailSender _emailSender;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _contextAccessor;

        public EmailConfirmationService(IEmailSender emailSender, UserManager<IdentityUser> userManager, IHttpContextAccessor contextAccessor)
        {
            _emailSender = emailSender;
            _userManager = userManager;
            _contextAccessor = contextAccessor;
        }

        public async Task SendConfirmationEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                await SendConfirmationEmailAsync(user); 
            }
        }

        public async Task SendConfirmationEmailAsync(IdentityUser user)
        {
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedEmail = Uri.EscapeDataString(user.Email);
            var encodedToken = Uri.EscapeDataString(emailConfirmationToken);
            var emailConfirmationUrl = $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}/api/auth/email-confirmation?email={encodedEmail}&token={encodedToken}";

            await _emailSender.SendAsync(EmailTemplate.EmailConfirmationTemplateId, user.Email, new
            {
                Email = user.Email,
                Link = emailConfirmationUrl
            });
        }
    }
}
