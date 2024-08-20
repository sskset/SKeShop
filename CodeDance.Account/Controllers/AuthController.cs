using CodeDance.Account.Models;
using CodeDance.Account.Services;
using CodeDance.EmailSender;
using EmailSender.SendGrid;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
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
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IHttpContextAccessor _contextAccessor;

        public AuthController(UserManager<IdentityUser> userManager, IEmailSender emailSender, ILogger<AuthController> logger, IEmailConfirmationServce emailConfirmationServce, IJwtTokenService jwtTokenService, IHttpContextAccessor contextAccessor)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _emailConfirmationServce = emailConfirmationServce;
            _jwtTokenService = jwtTokenService;
            _contextAccessor = contextAccessor;
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

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }

            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpGet("resend-email-confirmation")]
        public async Task<IActionResult> ResendConfirmationEmail([FromQuery] ResendConfirmEmailModel model)
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
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, model.Token);

                if (result.Succeeded)
                {
                    return Ok();
                }
            }

            return BadRequest();
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest();
            }

            var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
            if (!isEmailConfirmed)
            {
                return BadRequest("Please confirm your email first.");
            }

            var isPasswordMatched = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordMatched)
            {
                return BadRequest();
            }

            var token = _jwtTokenService.GenerateToken(user);

            return Ok(new { token });
        }

        [AllowAnonymous]
        [HttpPost("send-password-reset-email")]
        public async Task<IActionResult> SendPasswordResetEmail([FromBody] SendResetPasswordEmailModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                token = Uri.EscapeDataString(token);
                var email = Uri.EscapeDataString(user.Email);

                var url = $"{_contextAccessor.HttpContext.Request.Scheme}://{_contextAccessor.HttpContext.Request.Host}/api/auth/password-reset?email={email}&token={token}";
                await _emailSender.SendAsync(
                    EmailTemplate.ResetPassword,
                    user.Email,
                    new
                    {
                        Email = user.Email,
                        Url = url
                    });
            }

            return Ok("A password reset link will be sent if this email is valid.");
        }

        [AllowAnonymous]
        [HttpGet("password-reset")]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }

                    return BadRequest(ModelState);
                }
            }

            return Ok();
        }
    }
}