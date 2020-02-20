using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PocketBookServer.Data;
using PocketBookServer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace PocketBookServer.Controllers.API
{
    [ApiController, Authorize]
    public class MedicationsController : ControllerBase
    {
        private const string LastModifiedFormat = "ddd, dd MMM yyyy HH:mm:ss G\\MT";
        private readonly ApplicationDataContext _dataContext;

        public MedicationsController(ApplicationDataContext dataContext)
        {
            _dataContext = dataContext;
        }

        [HttpPost("/api/add-medication"), Authorize(Roles = "Admin"),
         ProducesResponseType(typeof(IEnumerable<RequestError>), 400),
         ProducesResponseType(201)]
        public async Task<IActionResult> AddMedication(
            [FromBody,
             Bind("AdviceIfDeclined", "AdviceIfTaken", "Dose", "ExclusionCriteria", "Form", "InclusionCriteria",
                 "Indications", "Name", "Route", "SideEffects")]
            Medication medication)
        {
            var errors = new List<RequestError>();

            errors.AddRange(ValidateMedication(medication));

            if (await _dataContext.Medications.AnyAsync(m => m.Name == medication.Name))
                errors.Add(new RequestError { Error = RequestErrorType.IsInUse, Path = "name" });

            if (errors.Any())
                return BadRequest(errors);

            medication.LastModified = DateTimeOffset.UtcNow;

            _dataContext.Medications.Add(medication);
            await _dataContext.SaveChangesAsync();

            return Created($"/api/get-medication/{medication.Id}", medication);
        }

        [HttpDelete("/api/delete-medication/{id}"), Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMedication([FromRoute]int id)
        {
            var item = await _dataContext.Medications.FirstOrDefaultAsync(m => m.Id == id);

            if (item != null)
            {
                _dataContext.Medications.Remove(item);
                await _dataContext.SaveChangesAsync();
            }

            return NoContent();
        }

        [HttpGet("/api/get-medications"), ProducesResponseType(typeof(IEnumerable<MedicationSummary>), 200)]
        public Task<IActionResult> GetAllMedications()
        {
            var items = _dataContext.Medications.Select(m => new MedicationSummary(m));

            return Task.FromResult<IActionResult>(Ok(items));
        }

        [HttpGet("/api/get-medication/{id}"),
         ProducesResponseType(typeof(Medication), 200),
         ProducesResponseType(404), ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any, VaryByHeader = "Accept-Encoding")]
        public async Task<IActionResult> GetMedication([FromRoute]int id)
        {
            var item = await _dataContext.Medications.FirstOrDefaultAsync(m => m.Id == id);

            if (item == null)
                return NotFound();

            var responseEtag = item.GetEtag();
            var requestEtag = Request.Headers["If-None-Match"].FirstOrDefault();
            var requestDateAvailable = DateTimeOffset.TryParseExact(Request.Headers["If-Modified-Since"].FirstOrDefault(), LastModifiedFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var requestLastModified);

            if (responseEtag.Equals(requestEtag))
                return StatusCode((int)HttpStatusCode.NotModified);

            // Allows a 2s tolerance, as it is very unlikely that a change will be made this close together and it
            // allows for rounding errors from the string format.
            if (requestDateAvailable && string.IsNullOrEmpty(requestEtag) && (item.LastModified.ToUniversalTime() - requestLastModified).TotalSeconds < 2)
                return StatusCode((int)HttpStatusCode.NotModified);

            Response.Headers["ETag"] = item.GetEtag();
            Response.Headers["Last-Modified"] = item.LastModified.ToUniversalTime().ToString(LastModifiedFormat);

            return Ok(item);
        }

        [HttpPut("/api/update-medication/{id}"), Authorize(Roles = "Admin"),
                                            ProducesResponseType(typeof(IEnumerable<RequestError>), 400),
            ProducesResponseType(200), ProducesResponseType(404)]
        public async Task<IActionResult> UpdateMedication([FromRoute]int id, [FromBody, Bind("AdviceIfDeclined", "AdviceIfTaken", "Dose", "ExclusionCriteria", "Form", "InclusionCriteria",
                 "Indications", "Name", "Route", "SideEffects")] Medication medication)
        {
            var errors = new List<RequestError>();

            errors.AddRange(ValidateMedication(medication));

            if (await _dataContext.Medications.AnyAsync(m => m.Name == medication.Name && m.Id != id))
                errors.Add(new RequestError { Error = RequestErrorType.IsInUse, Path = "name" });

            if (errors.Any())
                return BadRequest(errors);

            if (!_dataContext.Medications.AsNoTracking().Any(m => m.Id == id))
                return NotFound();

            medication.Id = id;
            medication.LastModified = DateTimeOffset.UtcNow;

            _dataContext.Medications.Update(medication);

            await _dataContext.SaveChangesAsync();

            return Ok(medication);
        }

        private static IEnumerable<RequestError> ValidateMedication(Medication medication)
        {
            if (string.IsNullOrWhiteSpace(medication.AdviceIfDeclined))
                yield return new RequestError { Error = RequestErrorType.IsBlank, Path = "adviceIfDeclined" };
            if (string.IsNullOrWhiteSpace(medication.AdviceIfTaken))
                yield return new RequestError { Error = RequestErrorType.IsBlank, Path = "adviceIfTaken" };
            if (string.IsNullOrWhiteSpace(medication.Dose))
                yield return new RequestError { Error = RequestErrorType.IsBlank, Path = "dose" };
            if (string.IsNullOrWhiteSpace(medication.ExclusionCriteria))
                yield return new RequestError { Error = RequestErrorType.IsBlank, Path = "exclusionCriteria" };
            if (string.IsNullOrWhiteSpace(medication.Form))
                yield return new RequestError { Error = RequestErrorType.IsBlank, Path = "form" };
            if (string.IsNullOrWhiteSpace(medication.InclusionCriteria))
                yield return new RequestError { Error = RequestErrorType.IsBlank, Path = "inclusionCriteria" };
            if (string.IsNullOrWhiteSpace(medication.Indications))
                yield return new RequestError { Error = RequestErrorType.IsBlank, Path = "indications" };
            if (string.IsNullOrWhiteSpace(medication.Name))
                yield return new RequestError { Error = RequestErrorType.IsBlank, Path = "name" };
            if (string.IsNullOrWhiteSpace(medication.Route))
                yield return new RequestError { Error = RequestErrorType.IsBlank, Path = "route" };
            if (string.IsNullOrWhiteSpace(medication.SideEffects))
                yield return new RequestError { Error = RequestErrorType.IsBlank, Path = "sideEffects" };
        }
    }
}