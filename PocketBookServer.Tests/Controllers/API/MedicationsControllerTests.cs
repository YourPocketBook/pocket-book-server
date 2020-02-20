using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using PocketBookServer.Controllers.API;
using PocketBookServer.Data;
using PocketBookServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        public async Task CreateReturnsAddsItemOnGoodRequest()
        {
            PopulateDatabaseWithItems();

            var controller = new MedicationsController(CreateDatabase());

            var testItem = _fixture.Create<Medication>();

            var result = await controller.AddMedication(testItem);

            var newItem = await CreateDatabase().Medications.FirstOrDefaultAsync(m => m.Name == testItem.Name);
            testItem.Id = newItem.Id;
            newItem.Should().BeEquivalentTo(testItem);

            result.Should().BeCreatedResultWithValue($"/api/get-medication/{testItem.Id}", newItem);
        }

        [Fact]
        public async Task CreateReturnsBadRequestOnDuplicationName()
        {
            var items = PopulateDatabaseWithItems();

            var controller = new MedicationsController(CreateDatabase());

            var testItem = _fixture.Build<Medication>().With(m => m.Name, items.First().Name).Create();

            var result = await controller.AddMedication(testItem);

            result.Should().BeIsInUseResultForPath("name");
        }

        [Fact]
        public Task CreateReturnsBadRequestOnEmptyAdviceIfDeclined()
        {
            return TestEmptyValidation(m => m.AdviceIfDeclined, "adviceIfDeclined");
        }

        [Fact]
        public Task CreateReturnsBadRequestOnEmptyAdviceIfTaken()
        {
            return TestEmptyValidation(m => m.AdviceIfTaken, "adviceIfTaken");
        }

        [Fact]
        public Task CreateReturnsBadRequestOnEmptyDose()
        {
            return TestEmptyValidation(m => m.Dose, "dose");
        }

        [Fact]
        public Task CreateReturnsBadRequestOnEmptyExclusionCriteria()
        {
            return TestEmptyValidation(m => m.ExclusionCriteria, "exclusionCriteria");
        }

        [Fact]
        public Task CreateReturnsBadRequestOnEmptyForm()
        {
            return TestEmptyValidation(m => m.Form, "form");
        }

        [Fact]
        public Task CreateReturnsBadRequestOnEmptyInclusionCriteria()
        {
            return TestEmptyValidation(m => m.InclusionCriteria, "inclusionCriteria");
        }

        [Fact]
        public Task CreateReturnsBadRequestOnEmptyIndications()
        {
            return TestEmptyValidation(m => m.Indications, "indications");
        }

        [Fact]
        public Task CreateReturnsBadRequestOnEmptyName()
        {
            return TestEmptyValidation(m => m.Name, "name");
        }

        [Fact]
        public Task CreateReturnsBadRequestOnEmptyRoute()
        {
            return TestEmptyValidation(m => m.Route, "route");
        }

        [Fact]
        public Task CreateReturnsBadRequestOnEmptySideEffects()
        {
            return TestEmptyValidation(m => m.SideEffects, "sideEffects");
        }

        [Fact]
        public async Task DeleteMedicationRemovesItemOnGoodRequest()
        {
            var items = PopulateDatabaseWithItems();

            var controller = new MedicationsController(CreateDatabase());

            var result = await controller.DeleteMedication(items.First().Id);

            result.Should().BeOfType<NoContentResult>();

            (await CreateDatabase().Medications.AnyAsync(m => m.Id == items.First().Id)).Should().BeFalse();
        }

        [Fact]
        public async Task DeleteMedicationReturnsNoContentOnBadRequest()
        {
            var items = PopulateDatabaseWithItems();

            var controller = new MedicationsController(CreateDatabase());

            var result = await controller.DeleteMedication(-1);

            result.Should().BeOfType<NoContentResult>();

            CreateDatabase().Medications.Should().BeEquivalentTo(items);
        }

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

        [Fact]
        public async Task UpdateReturnsBadRequestOnDuplicateName()
        {
            await UpdateHandlesInputModel(duplicateName: true);
        }

        [Fact]
        public Task UpdateReturnsBadRequestOnEmptyAdviceIfDeclined()
        {
            return TestUpdateValidation(m => m.AdviceIfDeclined, "adviceIfDeclined");
        }

        [Fact]
        public Task UpdateReturnsBadRequestOnEmptyAdviceIfTaken()
        {
            return TestUpdateValidation(m => m.AdviceIfTaken, "adviceIfTaken");
        }

        [Fact]
        public Task UpdateReturnsBadRequestOnEmptyDose()
        {
            return TestUpdateValidation(m => m.Dose, "dose");
        }

        [Fact]
        public Task UpdateReturnsBadRequestOnEmptyExclusionCriteria()
        {
            return TestUpdateValidation(m => m.ExclusionCriteria, "exclusionCriteria");
        }

        [Fact]
        public Task UpdateReturnsBadRequestOnEmptyForm()
        {
            return TestUpdateValidation(m => m.Form, "form");
        }

        [Fact]
        public Task UpdateReturnsBadRequestOnEmptyInclusionCriteria()
        {
            return TestUpdateValidation(m => m.InclusionCriteria, "inclusionCriteria");
        }

        [Fact]
        public Task UpdateReturnsBadRequestOnEmptyIndications()
        {
            return TestUpdateValidation(m => m.Indications, "indications");
        }

        [Fact]
        public Task UpdateReturnsBadRequestOnEmptyName()
        {
            return TestUpdateValidation(m => m.Name, "name");
        }

        [Fact]
        public Task UpdateReturnsBadRequestOnEmptyRoute()
        {
            return TestUpdateValidation(m => m.Route, "route");
        }

        [Fact]
        public Task UpdateReturnsBadRequestOnEmptySideEffects()
        {
            return TestUpdateValidation(m => m.SideEffects, "sideEffects");
        }

        [Fact]
        public async Task UpdateReturnsNotFoundOnBadId()
        {
            await UpdateHandlesInputModel(goodId: false);
        }

        [Fact]
        public async Task UpdateReturnsOkOnGoodRequest()
        {
            await UpdateHandlesInputModel();
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

        private async Task TestEmptyValidation(Expression<Func<Medication, string>> fieldSelector, string field, [CallerMemberName] string name = null)
        {
            var controller = new MedicationsController(CreateDatabase(name));

            var testItem = _fixture.Build<Medication>().With(fieldSelector, "").Create();

            var result = await controller.AddMedication(testItem);

            result.Should().BeIsBlankResultForPath(field);
        }

        private async Task TestUpdateValidation(Expression<Func<Medication, string>> fieldSelector, string field, [CallerMemberName] string name = null)
        {
            var items = PopulateDatabaseWithItems(name);
            var controller = new MedicationsController(CreateDatabase(name));

            var testItem = items.First();

            var updateItem = _fixture.Build<Medication>().With(fieldSelector, "").Create();

            var result = await controller.UpdateMedication(testItem.Id, updateItem);

            result.Should().BeIsBlankResultForPath(field);
        }

        private async Task UpdateHandlesInputModel(bool goodId = true, bool duplicateName = false, [CallerMemberName] string name = null)
        {
            var items = PopulateDatabaseWithItems(name);

            var controller = new MedicationsController(CreateDatabase(name));

            var testItem = _fixture.Create<Medication>();

            var oldItem = items.OrderBy(m => m.Id).First();
            var secondItem = items.OrderBy(m => m.Id).Skip(1).First();

            var oldId = goodId ? oldItem.Id : -1;

            if (duplicateName)
                testItem.Name = secondItem.Name;

            var result = await controller.UpdateMedication(oldId, testItem);

            var newItem = await CreateDatabase(name).Medications.FirstOrDefaultAsync(m => m.Id == oldItem.Id);

            if (goodId && !duplicateName)
            {
                testItem.Id = oldId;

                newItem.Should().BeEquivalentTo(testItem);

                result.Should().BeOkResultWithValue(newItem);
            }
            else
            {
                newItem.Should().BeEquivalentTo(oldItem);

                if (duplicateName)
                    result.Should().BeIsInUseResultForPath("name");
                else
                    result.Should().BeOfType<NotFoundResult>();
            }
        }
    }
}