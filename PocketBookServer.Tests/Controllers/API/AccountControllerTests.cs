using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PocketBookServer.Controllers.API;
using PocketBookServer.Models;
using PocketBookServer.Models.Account;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace PocketBookServer.Tests.Controllers.API
{
    public class AccountControllerTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public async Task DeleteUserReturnsNoContent()
        {
            TestHelpers.CreateUserManagerWithUser(out var testUser);

            var controller = CreateLoggedInController(testUser);

            var res = await controller.DeleteAccount();

            res.Should().BeOfType<NoContentResult>();

            var newUser = await TestHelpers.CreateUserManager().FindByIdAsync(testUser.Id);
            newUser.Should().BeNull();
        }

        [Fact]
        public async Task GetEmailSettingsReturnsUserEmailSettings()
        {
            TestHelpers.CreateUserManagerWithUser(out var testUser);

            var controller = CreateLoggedInController(testUser);

            var res = await controller.GetEmailSettings();

            res.Should().BeOkResultWithValue(new UpdateEmailSettings { UpdateEmailConsentGiven = testUser.UpdateEmailConsentGiven });
        }

        [Fact]
        public async Task UpdateEmailSettingsReturnsErrorIfNoConsentValue()
        {
            TestHelpers.CreateUserManagerWithUser(out var testUser);

            var controller = CreateLoggedInController(testUser);

            var model = new UpdateEmailSettings
            {
                UpdateEmailConsentGiven = null
            };

            var res = await controller.UpdateEmailSettings(model);
            res.Should().BeIsBlankResultForPath("updateEmailConsentGiven");

            var newUser = await TestHelpers.CreateUserManager().FindByIdAsync(testUser.Id);
            newUser.UpdateEmailConsentGiven.Should().Be(testUser.UpdateEmailConsentGiven);
        }

        [Fact]
        public async Task UpdateEmailSettingsUpdatesValue()
        {
            var userManger = TestHelpers.CreateUserManagerWithUser(out var testUser);

            testUser.UpdateEmailConsentGiven = true;
            await userManger.UpdateAsync(testUser);

            var controller = CreateLoggedInController(testUser);

            var model = new UpdateEmailSettings
            {
                UpdateEmailConsentGiven = false
            };

            var res = await controller.UpdateEmailSettings(model);

            res.Should().BeOfType<NoContentResult>();

            var newUser = await TestHelpers.CreateUserManager().FindByIdAsync(testUser.Id);
            newUser.UpdateEmailConsentGiven.Should().BeFalse();
        }

        [Fact]
        public async Task UpdateNameReturnsErrorIfNoName()
        {
            TestHelpers.CreateUserManagerWithUser(out var testUser);

            var controller = CreateLoggedInController(testUser);

            var model = new UpdateName
            {
                Name = string.Empty
            };

            var res = await controller.UpdateName(model);

            res.Should().BeIsBlankResultForPath("name");

            var newUser = await TestHelpers.CreateUserManager().FindByIdAsync(testUser.Id);
            newUser.RealName.Should().Be(testUser.RealName);
        }

        [Fact]
        public async Task UpdateNameReturnsNoContent()
        {
            TestHelpers.CreateUserManagerWithUser(out var testUser);
            var tokenGenerator = TestHelpers.CreateTokenGenerator();

            var testToken = _fixture.Create<string>();
            var model = _fixture.Create<UpdateName>();

            tokenGenerator.Setup(t => t.GetTokenAsync(It.Is<ApplicationUser>(a => a.RealName == model.Name && a.Id == testUser.Id))).ReturnsAsync(testToken);

            var controller = CreateLoggedInController(testUser);

            var res = await controller.UpdateName(model);

            res.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task UpdatePasswordReturnsErrorIfNoCurrentPassword()
        {
            await UpdatePasswordChecksInput(u => u.CurrentPassword, "currentPassword");
        }

        [Fact]
        public async Task UpdatePasswordReturnsErrorIfNoNewPassword()
        {
            await UpdatePasswordChecksInput(u => u.NewPassword, "newPassword");
        }

        [Fact]
        public async Task UpdatePasswordReturnsMultipleErrorsIfMultipleDetailsAreMissing()
        {
            var controller = new AccountController(TestHelpers.CreateUserManager(), new NullLogger<AccountController>());

            var model = new UpdatePassword
            {
                CurrentPassword = string.Empty,
                NewPassword = string.Empty
            };

            var res = await controller.UpdatePassword(model);

            res.Should().BeIsBlankResultForPath(new[] { "currentPassword", "newPassword" });
        }

        [Fact]
        public async Task UpdatePasswordSuccessChangePasswordAndReturnsNoContent()
        {
            var testCurrentPassword = _fixture.Create<string>();
            var testNewPassword = _fixture.Create<string>();
            TestHelpers.CreateUserManagerWithUser(out var testUser, true, testCurrentPassword);

            var model = new UpdatePassword
            {
                CurrentPassword = testCurrentPassword,
                NewPassword = testNewPassword
            };

            var controller = CreateLoggedInController(testUser);

            var res = await controller.UpdatePassword(model);

            res.Should().BeOfType<NoContentResult>();

            var newUser = await TestHelpers.CreateUserManager().FindByIdAsync(testUser.Id);

            (await TestHelpers.CreateUserManager().CheckPasswordAsync(newUser, testNewPassword)).Should().BeTrue();
        }

        [Fact]
        public async Task UpdatePasswordWrongCurrentPasswordReturnsBadRequest()
        {
            var testCurrentPassword = _fixture.Create<string>();
            TestHelpers.CreateUserManagerWithUser(out var testUser, true, testCurrentPassword);
            var testNewPassword = _fixture.Create<string>();

            var model = new UpdatePassword
            {
                CurrentPassword = _fixture.Create<string>(),
                NewPassword = testNewPassword
            };

            var controller = CreateLoggedInController(testUser);

            var res = await controller.UpdatePassword(model);

            res.Should().BeIsInvalidResultForPath("currentPassword");
        }

        private AccountController CreateLoggedInController(ApplicationUser user, [CallerMemberName] string name = null)
        {
            return new AccountController(TestHelpers.CreateUserManager(name), new NullLogger<AccountController>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                        new Claim (ClaimTypes.NameIdentifier, user.Id)
                    }))
                    }
                }
            };
        }

        private async Task UpdatePasswordChecksInput(Expression<Func<UpdatePassword, string>> fieldSelector, string field, [CallerMemberName] string name = null)
        {
            var controller = new AccountController(TestHelpers.CreateUserManager(name), new NullLogger<AccountController>());

            var model = _fixture.Build<UpdatePassword>().With(fieldSelector, string.Empty).Create();

            var res = await controller.UpdatePassword(model);

            res.Should().BeIsBlankResultForPath(field);
        }
    }
}