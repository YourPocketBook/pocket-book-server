using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PocketBookModel;
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
    }
}
