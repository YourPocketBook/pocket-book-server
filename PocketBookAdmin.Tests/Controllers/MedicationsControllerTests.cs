using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PocketBookAdmin.Controllers;
using PocketBookAdmin.ViewModels;
using PocketBookModel;
using PocketBookModel.Services;
using System.Threading.Tasks;
using Xunit;

namespace PocketBookAdmin.Tests
{
    public class MedicationsControllerTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void GetCreateReturnsView()
        {
            var controller = new MedicationsController(null);

            var res = controller.Create();

            res.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task GetDeleteReturnsNotFoundOnBadRequest()
        {
            var testItem = _fixture.Create<Medication>();
            var service = new Mock<IMedicationService>();
            service.Setup(m => m.GetAsync(testItem.Id))
                .Returns(Task.FromResult<Medication>(null));

            var controller = new MedicationsController(service.Object);

            var res = await controller.Delete(testItem.Id);

            res.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetDeleteReturnsViewOnGoodRequest()
        {
            var testItem = _fixture.Create<Medication>();
            var service = new Mock<IMedicationService>();
            service.Setup(m => m.GetAsync(testItem.Id)).ReturnsAsync(testItem);

            var controller = new MedicationsController(service.Object);

            var res = await controller.Delete(testItem.Id);

            res.Should().BeOfType<ViewResult>().Which.Model.Should().Be(testItem);
        }

        [Fact]
        public async Task GetEditReturnsNotFoundOnBadRequest()
        {
            var testItem = _fixture.Create<Medication>();
            var service = new Mock<IMedicationService>();
            service.Setup(m => m.GetAsync(testItem.Id))
                .Returns(Task.FromResult<Medication>(null));

            var controller = new MedicationsController(service.Object);

            var res = await controller.Edit(testItem.Id);

            res.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetEditReturnsViewOnGoodRequest()
        {
            var testItem = _fixture.Create<Medication>();
            var service = new Mock<IMedicationService>();
            service.Setup(m => m.GetAsync(testItem.Id)).ReturnsAsync(testItem);

            var controller = new MedicationsController(service.Object);

            var res = await controller.Edit(testItem.Id);

            res.Should().BeOfType<ViewResult>().Which.Model.Should().Be(testItem);
        }

        [Fact]
        public async Task PostCreateReturnsRedirectOnGoodRequest()
        {
            var testItem = _fixture.Create<EditMedication>();
            var service = new Mock<IMedicationService>();
            service.Setup(m => m.CheckMedicationName(testItem.Name, null)).Returns(Task.FromResult(true));
            service.Setup(m => m.AddAsync(testItem)).Returns(Task.CompletedTask);

            var controller = new MedicationsController(service.Object);

            var result = await controller.Create(testItem);

            service.Verify(m => m.AddAsync(testItem));

            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task PostCreateReturnsViewOnBadValidation()
        {
            var testItem = _fixture.Create<EditMedication>();
            var service = new Mock<IMedicationService>();
            service.Setup(m => m.CheckMedicationName(testItem.Name, null)).Returns(Task.FromResult(true));

            var controller = new MedicationsController(service.Object);
            controller.ModelState.AddModelError("", "Test Error");

            var result = await controller.Create(testItem);

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task PostCreateReturnsViewOnDuplicateName()
        {
            var testItem = _fixture.Create<EditMedication>();
            var service = new Mock<IMedicationService>();
            service.Setup(m => m.CheckMedicationName(testItem.Name, null)).Returns(Task.FromResult(false));

            var controller = new MedicationsController(service.Object);

            var result = await controller.Create(testItem);

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task PostDeleteReturnsRedirectOnRequest()
        {
            var testItem = _fixture.Create<Medication>();
            var service = new Mock<IMedicationService>();
            service.Setup(m => m.DeleteAsync(testItem.Id)).Returns(Task.CompletedTask);

            var controller = new MedicationsController(service.Object);

            var result = await controller.DeletePost(testItem.Id);

            service.Verify(m => m.DeleteAsync(testItem.Id));

            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task PostEditReturnsRedirectOnGoodRequest()
        {
            var testId = 42;
            var testItem = _fixture.Create<EditMedication>();
            var testModel = (Medication)testItem;
            testModel.Id = testId;

            var service = new Mock<IMedicationService>();
            service.Setup(m => m.CheckMedicationName(testItem.Name, testId)).Returns(Task.FromResult(true));
            service.Setup(m => m.UpdateAsync(testModel)).Returns(Task.CompletedTask);

            var controller = new MedicationsController(service.Object);

            var result = await controller.Edit(testId, testItem);

            service.Verify(m => m.UpdateAsync(testModel));

            result.Should().BeOfType<RedirectToActionResult>()
                .Which.ActionName.Should().Be("Index");
        }

        [Fact]
        public async Task PostEditReturnsViewOnBadValidation()
        {
            var testId = 42;
            var testItem = _fixture.Create<EditMedication>();
            var service = new Mock<IMedicationService>();
            service.Setup(m => m.CheckMedicationName(testItem.Name, testId)).Returns(Task.FromResult(true));

            var controller = new MedicationsController(service.Object);
            controller.ModelState.AddModelError("", "Test Error");

            var result = await controller.Edit(testId, testItem);

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task PostEditReturnsViewOnDuplicateName()
        {
            var testId = 42;
            var testItem = _fixture.Create<EditMedication>();
            var service = new Mock<IMedicationService>();
            service.Setup(m => m.CheckMedicationName(testItem.Name, testId)).Returns(Task.FromResult(false));

            var controller = new MedicationsController(service.Object);

            var result = await controller.Edit(testId, testItem);

            result.Should().BeOfType<ViewResult>();
        }
    }
}
