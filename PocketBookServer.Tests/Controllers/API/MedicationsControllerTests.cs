using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using PocketBookServer.Controllers.API;
using PocketBookServer.Data;
using PocketBookServer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace PocketBookServer.Tests.Controllers.API
{
    public class MedicationsControllerTests
    {
        private const string ModifiedDateFormat = "ddd, dd MMM yyyy HH:mm:ss G\\MT";
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public async Task GetAllReturnsAllItemsAsASummary()
        {
            var items = PopulateDatabaseWithItems();

            var controller = new MedicationsController(CreateDatabase());

            var result = await controller.GetAllMedications();

            result.Should().BeOkResultWithValue(items.Select(m => new MedicationSummary(m)));
        }

        [Fact]
        public async Task GetReturnsItemWhenEtagWrong()
        {
            await GetReturnsItem(etag: false);
        }

        [Fact]
        public async Task GetReturnsItemWhenEtagWrongIgnoringNotModifiedSince()
        {
            await GetReturnsItem(etag: false, lastModified: true);
        }

        [Fact]
        public async Task GetReturnsItemWhenLastModifiedWrong()
        {
            await GetReturnsItem(lastModified: false);
        }

        [Fact]
        public async Task GetReturnsItemWithHeaderDetails()
        {
            await GetReturnsItem();
        }

        [Fact]
        public async Task GetReturnsNotFoundWithBadId()
        {
            PopulateDatabaseWithItems();

            var controller = new MedicationsController(CreateDatabase());

            var result = await controller.GetMedication(-1);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetReturnsNotModifiedWhenEtagRight()
        {
            await GetReturnsItem(etag: true);
        }

        [Fact]
        public async Task GetReturnsNotModifiedWhenEtagRightAndLastModifiedWrong()
        {
            await GetReturnsItem(etag: true, lastModified: false);
        }

        [Fact]
        public async Task GetReturnsNotModifiedWhenLastModifiedRight()
        {
            await GetReturnsItem(lastModified: true);
        }

        private static ApplicationDataContext CreateDatabase([CallerMemberName] string name = null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDataContext>()
                .UseInMemoryDatabase($"{nameof(MedicationsControllerTests)}.{name}")
                .Options;

            return new ApplicationDataContext(options);
        }

        private async Task GetReturnsItem(bool? etag = null, bool? lastModified = null, [CallerMemberName] string name = null)
        {
            var items = PopulateDatabaseWithItems(name);
            var testItem = items.First();

            var controller = new MedicationsController(CreateDatabase(name))
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            if (etag == true)
                controller.Request.Headers["If-None-Match"] = testItem.GetEtag();
            else if (etag == false)
                controller.Request.Headers["If-None-Match"] = _fixture.Create<string>();

            if (lastModified == true)
                controller.Request.Headers["If-Modified-Since"] = testItem.LastModified.ToString(ModifiedDateFormat);
            else if (lastModified == false)
                controller.Request.Headers["If-Modified-Since"] = testItem.LastModified.AddDays(-1).ToString(ModifiedDateFormat);

            var result = await controller.GetMedication(testItem.Id);

            if (etag == true || (lastModified == true && etag == null))
            {
                result.Should().BeOfType<StatusCodeResult>()
                    .Which.StatusCode.Should().Be((int)HttpStatusCode.NotModified);
            }
            else
            {
                result.Should().BeOkResultWithValue(testItem);

                controller.Response.Headers.Should().Contain(KeyValuePair.Create("ETag", new StringValues(testItem.GetEtag())));
                controller.Response.Headers.Should().Contain(KeyValuePair.Create("Last-Modified", new StringValues(testItem.LastModified.ToUniversalTime().ToString(ModifiedDateFormat))));
            }
        }

        private IEnumerable<Medication> PopulateDatabaseWithItems([CallerMemberName] string name = null)
        {
            var database = CreateDatabase(name);

            var items = _fixture.CreateMany<Medication>().ToList();

            database.Medications.AddRange(items);
            database.SaveChanges();

            foreach (var i in items)
                database.Entry(i).State = EntityState.Detached;

            return items;
        }
    }
}