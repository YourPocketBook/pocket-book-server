using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PocketBookServer.Logging;
using PocketBookServer.Models;
using PocketBookServer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace PocketBookServer.Controllers.API
{
    public class TokenResult
    {
        public string Token { get; set; }
    }

    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IEmailSender _emailSender;
        private readonly ILogger<UserController> _logger;
        private readonly UserControllerOptions _options;
        private readonly ITokenGenerator _tokenGenerator;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, IOptions<UserControllerOptions> optionsAccessor, ILogger<UserController> logger, ITokenGenerator tokenGenerator)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
            _options = optionsAccessor.Value;
            _tokenGenerator = tokenGenerator;
        }

        [HttpPost("/api/confirm-user"),
         ProducesResponseType(typeof(IEnumerable<RequestError>), 400),
         ProducesResponseType(404), ProducesResponseType(200)]
        public async Task<IActionResult> Confirm(string userId, string code)
        {
            var errors = new List<RequestError>();

            if (string.IsNullOrWhiteSpace(code))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "code" });

            if (string.IsNullOrWhiteSpace(userId))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "userId" });

            if (errors.Any())
            {
                _logger.LogWarning(EventIds.ValidationFailure, "Email Confirmation: failed validation : {0}", string.Join(";", errors.Select(e => e.Path + "," + e.Error)));
                return BadRequest(errors);
            }

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                _logger.LogWarning(EventIds.UserNotFound, "Email Confirmation: user not found");
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                _logger.LogInformation(EventIds.EmailConfirmed, "Email Confirmation: success");
                return NoContent();
            }

            _logger.LogWarning(EventIds.ValidationFailure, "Email Confirmation: failed validation : code,IsInvalid");
            return BadRequest(new[] { new RequestError { Error = RequestErrorType.IsInvalid, Path = "code" } });
        }

        [HttpPost("/api/forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody]ForgotPassword forgotPassword)
        {
            var errors = new List<RequestError>();

            if (string.IsNullOrWhiteSpace(forgotPassword.Email))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "email" });

            if (errors.Any())
            {
                _logger.LogWarning(EventIds.ValidationFailure, "Forgot Password: failed validation : {0}", string.Join(";", errors.Select(e => e.Path + "," + e.Error)));
                return BadRequest(errors);
            }

            var user = await _userManager.FindByEmailAsync(forgotPassword.Email);

            if (user != null)
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);

                var callbackUrl = $"{_options.HostUrl}reset-password?userId={user.Id}&code={HttpUtility.UrlEncode(code)}";
                await _emailSender.SendEmailAsync(user.Email, "Reset your Password",
                    $"<p>Someone has requested a password reset for your PocketBook account.</p><p>If if was you, please confirm by <a href='{callbackUrl}'>clicking here</a>.</p><p>If it wasn't you, it is safe to ignore this email.</p><p>The PocketBook Team</p>");
                _logger.LogInformation(EventIds.EmailSent, "Forgot Password: user exists");
            }
            else
            {
                _logger.LogInformation(EventIds.UserNotFound, "Forgot Password: user does not exist");
            }

            return NoContent();
        }

        [HttpPost("/api/login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var errors = new List<RequestError>();

            if (string.IsNullOrWhiteSpace(model.Email))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "email" });

            if (string.IsNullOrWhiteSpace(model.Password))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "password" });

            if (errors.Any())
            {
                _logger.LogWarning(EventIds.ValidationFailure, "Login: failed validation : {0}", string.Join(";", errors.Select(e => e.Path + "," + e.Error)));
                return BadRequest(errors);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user != null && !user.EmailConfirmed)
            {
                _logger.LogWarning(EventIds.EmailNotConfirmed, "Login: user email is not confirmed");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var callbackUrl = $"{_options.HostUrl}confirm-email?userId={user.Id}&code={HttpUtility.UrlEncode(code)}";
                await _emailSender.SendEmailAsync(user.Email, "Confirm your email",
                    $"<p>Thanks for registering for Pocket Book</p><p>Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.</p><p>The PocketBook Team</p>");

                return StatusCode((int)HttpStatusCode.Forbidden,
                  new[] { new RequestError { Error = RequestErrorType.EmailNotConfirmed, Path = "email" } });
            }

            if (user == null)
                return Unauthorized();

            var passwordGood = await _userManager.CheckPasswordAsync(user, model.Password);

            if (passwordGood)
            {
                var tokenString = await _tokenGenerator.GetTokenAsync(user);

                return Ok(new TokenResult { Token = tokenString });
            }

            return Unauthorized();
        }

        [HttpPost("/api/register-user"),
         ProducesResponseType(typeof(IEnumerable<RequestError>), 400),
         ProducesResponseType(typeof(IEnumerable<RequestError>), 409),
         ProducesResponseType(201), ProducesResponseType(500)]
        public async Task<IActionResult> Register(CreateUser user)
        {
            var errors = new List<RequestError>();

            if (string.IsNullOrWhiteSpace(user.Name))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "name" });

            if (string.IsNullOrWhiteSpace(user.Email))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "email" });
            else if (!user.Email.EndsWith("@sja.org.uk"))
                errors.Add(new RequestError { Error = RequestErrorType.IsInvalid, Path = "email" });

            if (string.IsNullOrWhiteSpace(user.Password))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "password" });

            if (errors.Any())
            {
                _logger.LogWarning(EventIds.ValidationFailure, "Register: failed validation : {0}", string.Join(";", errors.Select(e => e.Path + "," + e.Error)));
                return BadRequest(errors);
            }

            if (await _userManager.FindByEmailAsync(user.Email) != null)
            {
                _logger.LogWarning(EventIds.ValidationFailure, "Register: email already in use");
                return Conflict(new[] { new RequestError { Error = RequestErrorType.IsInUse, Path = "email" } });
            }

            var newUser = new ApplicationUser
            {
                Email = user.Email,
                UserName = user.Email,
                RealName = user.Name,
                UpdateEmailConsentGiven = user.UpdateEmailConsentGiven
            };

            var res = await _userManager.CreateAsync(newUser, user.Password);

            if (!res.Succeeded)
            {
                _logger.LogError(EventIds.UnknownError, "Register: unknown error");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(newUser);

            var callbackUrl = $"{_options.HostUrl}confirm-email?userId={newUser.Id}&code={HttpUtility.UrlEncode(code)}";
            await _emailSender.SendEmailAsync(newUser.Email, "Confirm your email",
                $"<p>Thanks for registering for Pocket Book</p><p>Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.</p><p>The PocketBook Team</p>");
            _logger.LogInformation(EventIds.UserCreated, "Register: user created");

            return NoContent();
        }

        [HttpPost("/api/reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPassword resetPassword)
        {
            var errors = new List<RequestError>();

            if (string.IsNullOrWhiteSpace(resetPassword.Code))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "code" });
            if (string.IsNullOrWhiteSpace(resetPassword.NewPassword))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "newPassword" });
            if (string.IsNullOrWhiteSpace(resetPassword.UserId))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "userId" });

            if (errors.Any())
            {
                _logger.LogWarning(EventIds.ValidationFailure, "Password Reset: failed validation : {0}", string.Join(";", errors.Select(e => e.Path + "," + e.Error)));
                return BadRequest(errors);
            }

            var user = await _userManager.FindByIdAsync(resetPassword.UserId);

            if (user != null)
            {
                var res = await _userManager.ResetPasswordAsync(user, resetPassword.Code, resetPassword.NewPassword);

                if (res.Succeeded)
                {
                    _logger.LogInformation(EventIds.PasswordChanged, "Reset Password: password changed");
                    return NoContent();
                }
                _logger.LogError(EventIds.UnknownError, "Reset Password: change failed");
                return BadRequest();
            }
            else
            {
                _logger.LogError(EventIds.UserNotFound, "Reset Password: user does not exist");
                return NotFound();
            }
        }
    }

    public class UserControllerOptions
    {
        public string HostUrl { get; set; }
        public string TokenAudience { get; set; }
        public string TokenIssuer { get; set; }
        public string TokenSecret { get; set; }
    }
}