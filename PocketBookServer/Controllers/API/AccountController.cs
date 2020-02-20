using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PocketBookServer.Logging;
using PocketBookServer.Models;
using PocketBookServer.Models.Account;
using PocketBookServer.Services;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PocketBookServer.Controllers.API
{
    [ApiController, Authorize]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [HttpDelete("api/delete-account")]
        public async Task<IActionResult> DeleteAccount()
        {
            var user = await _userManager.GetUserAsync(User);

            await _userManager.DeleteAsync(user);

            return NoContent();
        }

        [HttpGet("api/get-email-settings")]
        public async Task<IActionResult> GetEmailSettings()
        {
            var user = await _userManager.GetUserAsync(User);

            return Ok(new UpdateEmailSettings
            {
                UpdateEmailConsentGiven = user.UpdateEmailConsentGiven
            });
        }

        [HttpPut("api/update-email-settings")]
        public async Task<IActionResult> UpdateEmailSettings([FromBody]UpdateEmailSettings model)
        {
            var errors = new List<RequestError>();

            if (model.UpdateEmailConsentGiven == null)
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "updateEmailConsentGiven" });

            if (errors.Any())
            {
                _logger.LogWarning(EventIds.ValidationFailure, "Update Enail Settings: failed validation : {0}", string.Join(";", errors.Select(e => e.Path + "," + e.Error)));
                return BadRequest(errors);
            }

            var user = await _userManager.GetUserAsync(User);

            user.UpdateEmailConsentGiven = model.UpdateEmailConsentGiven.Value;

            var res = await _userManager.UpdateAsync(user);

            if (res.Succeeded)
            {
                return NoContent();
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPut("/api/update-name")]
        public async Task<IActionResult> UpdateName([FromBody]UpdateName model)
        {
            var errors = new List<RequestError>();

            if (string.IsNullOrWhiteSpace(model.Name))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "name" });

            if (errors.Any())
            {
                _logger.LogWarning(EventIds.ValidationFailure, "Update Name: failed validation : {0}", string.Join(";", errors.Select(e => e.Path + "," + e.Error)));
                return BadRequest(errors);
            }

            var user = await _userManager.GetUserAsync(User);

            user.RealName = model.Name;

            var res = await _userManager.UpdateAsync(user);

            if (res.Succeeded)
            {
                _logger.LogInformation(EventIds.ItemUpdated, "Update Name: Success");
                return NoContent();
            }

            return StatusCode((int)HttpStatusCode.InternalServerError);
        }

        [HttpPut("/api/update-password")]
        public async Task<IActionResult> UpdatePassword([FromBody]UpdatePassword model)
        {
            var errors = new List<RequestError>();

            if (string.IsNullOrWhiteSpace(model.NewPassword))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "newPassword" });
            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
                errors.Add(new RequestError { Error = RequestErrorType.IsBlank, Path = "currentPassword" });

            if (errors.Any())
            {
                _logger.LogWarning(EventIds.ValidationFailure, "Update Password: failed validation : {0}", string.Join(";", errors.Select(e => e.Path + "," + e.Error)));
                return BadRequest(errors);
            }

            var user = await _userManager.GetUserAsync(User);
            var res = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (res.Succeeded)
            {
                _logger.LogInformation(EventIds.ItemUpdated, "Update Password: Success");
                return NoContent();
            }

            errors.Add(new RequestError { Error = RequestErrorType.IsInvalid, Path = "currentPassword" });
            return BadRequest(errors);
        }
    }
}