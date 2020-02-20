using AutoFixture;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PocketBookServer.Data;
using PocketBookServer.Models;
using PocketBookServer.Services;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PocketBookServer.Tests
{
    internal static class TestHelpers
    {
        public static string BadHeaderToken { get; } = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJuYW1laWQiOiI3NjBjZTA0NC00Njc1LTRkODMtODMzYS1hY2YwODMzNDRhOGIiLCJnaXZlbl9uYW1lIjoiVG9ueSBSaWNoYXJkcyIsIm5iZiI6MTU2NjIzNjEzNiwiZXhwIjoxNTY2MjM2MTM2LCJpYXQiOjE1NjcyMzYxMzYsImlzcyI6IlRlc3RJc3N1ZXIiLCJhdWQiOiJUZXN0QXVkaWVuY2UifQ.12";
        public static string InvalidOutOfDateToken { get; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI3NjBjZTA0NC00Njc1LTRkODMtODMzYS1hY2YwODMzNDRhOGIiLCJnaXZlbl9uYW1lIjoiVG9ueSBSaWNoYXJkcyIsIm5iZiI6MTU2NjIzNjEzNiwiZXhwIjoxNTY2MjM2MTM2LCJpYXQiOjE1NjcyMzYxMzYsImlzcyI6IlRlc3RJc3N1ZXIiLCJhdWQiOiJUZXN0QXVkaWVuY2UifQ.u7fAsC6W9NB4";
        public static string TestSecret { get; } = "TestSecretTestSecretTestSecret";

        // This is the ID embedded into the test token
        public static string TestUserId { get; } = "760ce044-4675-4d83-833a-acf083344a8b";

        public static string ValidOutOfDateToken { get; } = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiI3NjBjZTA0NC00Njc1LTRkODMtODMzYS1hY2YwODMzNDRhOGIiLCJnaXZlbl9uYW1lIjoiVG9ueSBSaWNoYXJkcyIsIm5iZiI6MTU2NjIzNjEzNiwiZXhwIjoxNTY2MjM2MTM2LCJpYXQiOjE1NjcyMzYxMzYsImlzcyI6IlRlc3RJc3N1ZXIiLCJhdWQiOiJUZXN0QXVkaWVuY2UifQ.u7fAsC6W9NB4wiSBnAfTLBCiUxF8OWZ7qp41b4YwBCY";

        public static ApplicationDataContext CreateDatabase([CallerMemberName] string name = null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDataContext>()
                .UseInMemoryDatabase(name)
                .Options;

            return new ApplicationDataContext(options);
        }

        public static async Task<RoleManager<IdentityRole>> CreateRoleManager([CallerMemberName] string name = null)
        {
            var database = CreateDatabase(name);
            var store = new RoleStore<IdentityRole>(database);
            var roleManager = new RoleManager<IdentityRole>(store, Enumerable.Empty<IRoleValidator<IdentityRole>>(), null, new IdentityErrorDescriber(), new NullLogger<RoleManager<IdentityRole>>());

            if (await roleManager.FindByNameAsync("Admin") == null)
            {
                var role = new IdentityRole() { Name = "Admin" };
                await roleManager.CreateAsync(role);
                await roleManager.AddClaimAsync(role, new Claim(ClaimTypes.Role, "Admin"));
            }

            return roleManager;
        }

        public static Mock<ITokenGenerator> CreateTokenGenerator()
        {
            return new Mock<ITokenGenerator>(MockBehavior.Strict);
        }

        public static UserManager<ApplicationUser> CreateUserManager([CallerMemberName] string name = null)
        {
            var database = CreateDatabase(name);
            var store = new UserStore<ApplicationUser>(database);
            var tokenGenerator = new EmailTokenProvider<ApplicationUser>();

            var userManager = new UserManager<ApplicationUser>(store, null, new PasswordHasher<ApplicationUser>(), null,
                new[] { new PasswordValidator<ApplicationUser>(new IdentityErrorDescriber()) }, null, new IdentityErrorDescriber(), null,
                new NullLogger<UserManager<ApplicationUser>>());
            userManager.Options.Password.RequireDigit = false;
            userManager.Options.Password.RequireLowercase = false;
            userManager.Options.Password.RequireNonAlphanumeric = false;
            userManager.Options.Password.RequireUppercase = false;
            userManager.Options.Password.RequiredLength = 1;
            userManager.RegisterTokenProvider(TokenOptions.DefaultProvider, tokenGenerator);

            return userManager;
        }

        public static UserManager<ApplicationUser> CreateUserManagerWithUser(out ApplicationUser user, bool emailConfirmed = false, string password = null, [CallerMemberName] string name = null)
        {
            var fixture = new Fixture();
            var userManager = CreateUserManager(name);
            var testUser = fixture.Build<ApplicationUser>().With(u => u.Id, TestUserId).With(u => u.EmailConfirmed, emailConfirmed).Create();

            userManager.CreateAsync(testUser, password ?? fixture.Create<string>()).Wait();

            var result = CreateUserManager(name);
            user = result.FindByIdAsync(testUser.Id).Result;
            return result;
        }
    }
}