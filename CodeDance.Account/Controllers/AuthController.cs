using CodeDance.Account.Models;
using CodeDance.Account.Services;
using CodeDance.EmailSender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CodeDance.Account.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailConfirmationServce _emailConfirmationServce;

        public AuthController(UserManager<IdentityUser> userManager, IEmailSender emailSender, ILogger<AuthController> logger, IEmailConfirmationServce emailConfirmationServce)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _emailConfirmationServce = emailConfirmationServce;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(Models.RegisterModel model)
        {
            var user = new IdentityUser { Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _emailConfirmationServce.SendConfirmationEmailAsync(user);

                return Ok();
            }

            foreach(var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpGet("resend-email-confirmation")]
        public async Task<IActionResult> ResendConfirmationEmail(ResendConfirmEmailModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NoContent();
            }
            
            await _emailConfirmationServce.SendConfirmationEmailAsync(user);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("email-confirmation")]
        public async Task<IActionResult> ConfirmEmail(ConfirmEmailModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, model.Token);

                if(result.Succeeded)
                {
                    return Ok();
                }
            }

            return BadRequest();
        }
    }
}