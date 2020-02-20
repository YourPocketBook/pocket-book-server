using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using PocketBookServer.Controllers.API;
using PocketBookServer.Models;
using PocketBookServer.Services;
using System;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace PocketBookServer.Tests.Controllers.API
{
    public class UserControllerTests
    {
        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public async Task ConfirmReturnsErrorIfCodeIsInvalid()
        {
            var userManager = TestHelpers.CreateUserManagerWithUser(out var testUser);
            var controller = CreateController(userManager);

            var res = await controller.Confirm(testUser.Id, _fixture.Create<string>());

            res.Should().BeIsInvalidResultForPath("code");
            (await userManager.IsEmailConfirmedAsync(testUser)).Should().BeFalse();
        }

        [Fact]
        public async Task ConfirmReturnsErrorIfNoCode()
        {
            await ConfirmCheckMissingInput(_fixture.Create<string>(), string.Empty, "code");
        }

        [Fact]
        public async Task ConfirmReturnsErrorIfNoUserId()
        {
            await ConfirmCheckMissingInput(string.Empty, _fixture.Create<string>(), "userId");
        }

        [Fact]
        public async Task ConfirmReturnsNoContentIfCodeIsValid()
        {
            var userManager = TestHelpers.CreateUserManagerWithUser(out var testUser);
            var controller = CreateController(userManager);
            var code = await userManager.GenerateEmailConfirmationTokenAsync(testUser);

            var res = await controller.Confirm(testUser.Id, code);

            res.Should().BeOfType<NoContentResult>();
            (await userManager.IsEmailConfirmedAsync(testUser)).Should().BeTrue();
        }

        [Fact]
        public async Task ConfirmReturnsNotFoundIfUserDoesNotExist()
        {
            var controller = CreateController();

            var res = await controller.Confirm(_fixture.Create<string>(), _fixture.Create<string>());

            res.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task ForgetPasswordReturnsErrorIfNoEmail()
        {
            var forgotPassword = new ForgotPassword { Email = string.Empty };
            var controller = CreateController();

            var res = await controller.ForgotPassword(forgotPassword);

            res.Should().BeIsBlankResultForPath("email");
        }

        [Fact]
        public async Task ForgetPasswordReturnsNoContentAndEmailsIfEmailExists()
        {
            var emailSender = CreateEmailSender();
            var userManager = TestHelpers.CreateUserManager();
            var user = _fixture.Build<ApplicationUser>().With(u => u.EmailConfirmed, true).Create();
            await userManager.CreateAsync(user, _fixture.Create<string>());

            emailSender.Setup(e => e.SendEmailAsync(user.Email, "Reset your Password", It.IsAny<string>()))
                .Returns(Task.FromResult<object>(null))
                .Verifiable();

            var forgotPassword = new ForgotPassword { Email = user.Email };
            var options = CreateOptions();

            var controller = new UserController(TestHelpers.CreateUserManager(), emailSender.Object, options, new NullLogger<UserController>(), TestHelpers.CreateTokenGenerator().Object);

            var res = await controller.ForgotPassword(forgotPassword);

            res.Should().BeOfType<NoContentResult>();

            emailSender.Verify();
        }

        [Fact]
        public async Task ForgetPasswordReturnsNoContentIfNoEmailExists()
        {
            var forgotPassword = _fixture.Create<ForgotPassword>();
            var controller = CreateController();

            var res = await controller.ForgotPassword(forgotPassword);

            res.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task LoginReturnsErrorIfNoEmail()
        {
            var controller = CreateController();
            var email = string.Empty;
            var password = "password1234";

            var res = await controller.Login(new LoginModel { Email = email, Password = password });

            res.Should().BeIsBlankResultForPath("email");
        }

        [Fact]
        public async Task LoginReturnsErrorIfNoPassword()
        {
            var controller = CreateController();
            var email = "test@test.com";
            var password = string.Empty;

            var res = await controller.Login(new LoginModel { Email = email, Password = password });

            res.Should().BeIsBlankResultForPath("password");
        }

        [Fact]
        public async Task LoginReturnsForbiddenIfEmailNotConfirmed()
        {
            var userManager = TestHelpers.CreateUserManager();

            var email = "test@test.com";
            var password = "password1234";
            var user = new ApplicationUser { Email = email, UserName = email, RealName = "Test Name", EmailConfirmed = false };
            await userManager.CreateAsync(user, password);

            var emailSender = CreateEmailSender();

            emailSender.Setup(e => e.SendEmailAsync(user.Email, "Confirm your email", It.IsAny<string>()))
                .Returns(Task.FromResult<object>(null))
                .Verifiable();

            var controller = new UserController(userManager, emailSender.Object, CreateOptions(), new NullLogger<UserController>(), TestHelpers.CreateTokenGenerator().Object);

            var res = await controller.Login(new LoginModel { Email = email, Password = password });

            res.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().BeEquivalentTo(new[] { new RequestError { Error = RequestErrorType.EmailNotConfirmed, Path = "email" } });
            res.As<ObjectResult>().StatusCode.Should().Be((int)HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task LoginReturnsTokensIfCorrect()
        {
            var password = _fixture.Create<string>();
            var userManager = TestHelpers.CreateUserManagerWithUser(out var user, true, password);

            var options = CreateOptions();
            var tokenGenerator = TestHelpers.CreateTokenGenerator();
            const string testToken = "abcdefghi";

            tokenGenerator.Setup(t => t.GetTokenAsync(It.Is<ApplicationUser>(u => u.Id == user.Id))).ReturnsAsync(testToken);

            var testContext = new TestHttpContext();

            var controller = new UserController(TestHelpers.CreateUserManager(), CreateEmailSender().Object, options, new NullLogger<UserController>(), tokenGenerator.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = testContext
                }
            };

            var res = await controller.Login(new LoginModel { Email = user.Email, Password = password });
            res.Should().BeOkResultWithValue(new TokenResult { Token = testToken });
        }

        [Fact]
        public async Task LoginReturnsUnauthorisedIfPasswordIsWrong()
        {
            TestHelpers.CreateUserManagerWithUser(out var user, true);
            var password = "password1234";

            var controller = CreateController();

            var res = await controller.Login(new LoginModel { Email = user.Email, Password = password });

            res.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task LoginReturnsUnauthorisedIfUserNotFound()
        {
            var controller = CreateController();

            var res = await controller.Login(_fixture.Create<LoginModel>());

            res.Should().BeOfType<UnauthorizedResult>();
        }

        [Fact]
        public async Task RegisterReturnsErrorIfEmailIsAlreadyRegistered()
        {
            var userManager = TestHelpers.CreateUserManager();
            var controller = new UserController(userManager, CreateEmailSender().Object, CreateOptions(), new NullLogger<UserController>(), TestHelpers.CreateTokenGenerator().Object);

            var testUser = new CreateUser
            {
                Name = "Old Name",
                Email = "test@sja.org.uk",
                Password = "newPassword"
            };

            await userManager.CreateAsync(
                new ApplicationUser { Email = testUser.Email, RealName = testUser.Name, UserName = testUser.Email },
                testUser.Password);

            var testUser2 = new CreateUser
            {
                Name = "Test Name",
                Email = "test@sja.org.uk",
                Password = "password1234"
            };

            var res = await controller.Register(testUser2);

            res.Should().BeOfType<ConflictObjectResult>();

            var bror = (ConflictObjectResult)res;

            bror.Value.Should().BeEquivalentTo(new[]
            {
                new RequestError {Error = RequestErrorType.IsInUse, Path = "email"},
            });
        }

        [Fact]
        public async Task RegisterReturnsErrorIfInvalidEmail()
        {
            var controller = CreateController();

            var testUser = new CreateUser
            {
                Name = "Test Name",
                Email = "test@gmail.uk",
                Password = "password1234"
            };

            var res = await controller.Register(testUser);

            res.Should().BeOfType<BadRequestObjectResult>();

            var bror = (BadRequestObjectResult)res;

            bror.Value.Should().BeEquivalentTo(new[] { new RequestError { Error = RequestErrorType.IsInvalid, Path = "email" } });
        }

        [Fact]
        public async Task RegisterReturnsErrorIfNoEmail()
        {
            var controller = CreateController();

            var testUser = new CreateUser
            {
                Name = "Test Name",
                Email = string.Empty,
                Password = "password1234"
            };

            var res = await controller.Register(testUser);

            res.Should().BeIsBlankResultForPath("email");
        }

        [Fact]
        public async Task RegisterReturnsErrorIfNoName()
        {
            var controller = CreateController();

            var testUser = new CreateUser
            {
                Name = string.Empty,
                Email = "test@sja.org.uk",
                Password = "password1234"
            };

            var res = await controller.Register(testUser);

            res.Should().BeIsBlankResultForPath("name");
        }

        [Fact]
        public async Task RegisterReturnsErrorIfNoPassword()
        {
            var controller = CreateController();

            var testUser = new CreateUser
            {
                Name = "Test Name",
                Email = "test@sja.org.uk",
                Password = string.Empty
            };

            var res = await controller.Register(testUser);

            res.Should().BeIsBlankResultForPath("password");
        }

        [Fact]
        public async Task RegisterReturnsMultipleErrorsIfMultipleDetailsAreMissing()
        {
            var controller = CreateController();

            var testUser = new CreateUser
            {
                Name = string.Empty,
                Email = string.Empty,
                Password = string.Empty
            };

            var res = await controller.Register(testUser);

            res.Should().BeIsBlankResultForPath(new[] { "name", "email", "password" });
        }

        [Fact]
        public async Task RegisterStoresValidUserInDatabaseAndConfirmsEmail()
        {
            var testUser = new CreateUser
            {
                Name = "Test Name",
                Email = "test@sja.org.uk",
                Password = "password1234",
                UpdateEmailConsentGiven = true
            };
            var userManager = TestHelpers.CreateUserManager();
            var emailSender = CreateEmailSender();

            emailSender.Setup(e => e.SendEmailAsync(testUser.Email, "Confirm your email", It.IsAny<string>()))
                .Returns(Task.FromResult<object>(null))
                .Verifiable();

            var controller = new UserController(userManager, emailSender.Object, CreateOptions(), new NullLogger<UserController>(), TestHelpers.CreateTokenGenerator().Object);

            var res = await controller.Register(testUser);

            res.Should().BeOfType<NoContentResult>();

            var user = await userManager.Users.FirstOrDefaultAsync(u => u.Email == testUser.Email);

            user.Should().NotBeNull();

            user.UserName.Should().Be(testUser.Email);
            user.RealName.Should().Be(testUser.Name);
            user.UpdateEmailConsentGiven.Should().Be(testUser.UpdateEmailConsentGiven);

            var passwordRes = await userManager.CheckPasswordAsync(user, testUser.Password);

            passwordRes.Should().BeTrue();
            emailSender.Verify();
        }

        [Fact]
        public async Task ResetPasswordReturnsBadRequestIfCodeIsInvalid()
        {
            var userManager = TestHelpers.CreateUserManagerWithUser(out var testUser);
            var controller = CreateController(userManager);

            var resetPassword = new ResetPassword
            {
                UserId = testUser.Id,
                NewPassword = "ijklmnop",
                Code = "bcdefgh"
            };

            var res = await controller.ResetPassword(resetPassword);

            res.Should().BeOfType<BadRequestResult>();

            (await userManager.CheckPasswordAsync(testUser, resetPassword.NewPassword)).Should().BeFalse();
        }

        [Fact]
        public async Task ResetPasswordReturnsErrorIfNoCode()
        {
            await ResetPasswordCheckMissingInput(c => c.Code, "code");
        }

        [Fact]
        public async Task ResetPasswordReturnsErrorIfNoPassword()
        {
            await ResetPasswordCheckMissingInput(c => c.NewPassword, "newPassword");
        }

        [Fact]
        public async Task ResetPasswordReturnsErrorIfNoUserId()
        {
            await ResetPasswordCheckMissingInput(c => c.UserId, "userId");
        }

        [Fact]
        public async Task ResetPasswordReturnsMultipleErrorsIfMultipleDetailsAreMissing()
        {
            var controller = CreateController();

            var resetPassword = new ResetPassword
            {
                UserId = string.Empty,
                NewPassword = string.Empty,
                Code = string.Empty
            };

            var res = await controller.ResetPassword(resetPassword);

            res.Should().BeIsBlankResultForPath(new[] { "userId", "newPassword", "code" });
        }

        [Fact]
        public async Task ResetPasswordReturnsNoContentIfCodeIsValid()
        {
            var userManager = TestHelpers.CreateUserManagerWithUser(out var testUser);
            var controller = CreateController(userManager);

            var code = await userManager.GeneratePasswordResetTokenAsync(testUser);

            var resetPassword = new ResetPassword
            {
                UserId = testUser.Id,
                NewPassword = "ijklmnop",
                Code = code
            };

            var res = await controller.ResetPassword(resetPassword);

            res.Should().BeOfType<NoContentResult>();

            (await userManager.CheckPasswordAsync(testUser, resetPassword.NewPassword)).Should().BeTrue();
        }

        [Fact]
        public async Task ResetPasswordReturnsNotFoundIfUserDoesNotExist()
        {
            var controller = CreateController();

            var resetPassword = new ResetPassword
            {
                UserId = "qrstuv",
                NewPassword = "ijklmnop",
                Code = "abcdefgh"
            };

            var res = await controller.ResetPassword(resetPassword);

            res.Should().BeOfType<NotFoundResult>();
        }

        private static async Task ConfirmCheckMissingInput(string userId, string userCode, string field, [CallerMemberName] string name = null)
        {
            var controller = CreateController(name: name);

            var res = await controller.Confirm(userId, userCode);

            res.Should().BeIsBlankResultForPath(field);
        }

        private static UserController CreateController(UserManager<ApplicationUser> userManager = null, [CallerMemberName] string name = null)
        {
            return new UserController(userManager ?? TestHelpers.CreateUserManager(name), CreateEmailSender().Object, CreateOptions(), new NullLogger<UserController>(), TestHelpers.CreateTokenGenerator().Object);
        }

        private static Mock<IEmailSender> CreateEmailSender()
        {
            return new Mock<IEmailSender>(MockBehavior.Strict);
        }

        private static IOptions<UserControllerOptions> CreateOptions()
        {
            var options = new UserControllerOptions
            {
                HostUrl = "TestUrl",
                TokenAudience = "TestAudience",
                TokenIssuer = "TestIssuer",
                TokenSecret = "TestSecretTestSecretTestSecret",
            };

            return new OptionsWrapper<UserControllerOptions>(options);
        }

        private async Task ResetPasswordCheckMissingInput(Expression<Func<ResetPassword, string>> fieldSelector, string field, [CallerMemberName] string name = null)
        {
            var controller = CreateController(name: name);

            var resetPassword = _fixture.Build<ResetPassword>().With(fieldSelector, "").Create();

            var res = await controller.ResetPassword(resetPassword);

            res.Should().BeIsBlankResultForPath(field);
        }
    }
}