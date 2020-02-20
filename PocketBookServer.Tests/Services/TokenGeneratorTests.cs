using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PocketBookServer.Models;
using PocketBookServer.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PocketBookServer.Tests.Services
{
    public class TokenGeneratorTests
    {
        [Fact]
        public async Task GetTokenReturnsValidAdminToken()
        {
            await GetTokenReturnsValidToken(true);
        }

        [Fact]
        public async Task GetTokenReturnsValidNonAdminToken()
        {
            await GetTokenReturnsValidToken(false);
        }

        [Fact]
        public async Task GetTokenThrowsWithNoUser()
        {
            var tokenGenerator = new TokenGenerator(TestHelpers.CreateUserManager(), CreateOptions(), await TestHelpers.CreateRoleManager());

            Func<Task> act = async () => await tokenGenerator.GetTokenAsync(null);
            act.Should().Throw<ArgumentNullException>();
        }

        private static IOptions<TokenGeneratorOptions> CreateOptions()
        {
            var options = new TokenGeneratorOptions
            {
                TokenAudience = "TestAudience",
                TokenIssuer = "TestIssuer",
                TokenSecret = "TestSecretTestSecretTestSecret"
            };

            return new OptionsWrapper<TokenGeneratorOptions>(options);
        }

        private static async Task SetUpAdministrator(UserManager<ApplicationUser> userManager, ApplicationUser user, [CallerMemberName] string name = null)
        {
            var roleManager = await TestHelpers.CreateRoleManager(name);

            var adminRole = await roleManager.FindByNameAsync("Admin");

            await userManager.AddToRoleAsync(user, adminRole.Name);
        }

        private static ClaimsPrincipal ValidateToken(ApplicationUser user, string token, string secret, string issuer, string audience)
        {
            var handler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(secret);
            var claims = handler.ValidateToken(token, new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
                ValidateLifetime = true
            }, out var valToken);

            claims.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
            claims.Claims.Should().Contain(c => c.Type == ClaimTypes.GivenName && c.Value == user.RealName);

            return claims;
        }

        private async Task GetTokenReturnsValidToken(bool isAdmin, [CallerMemberName] string name = null)
        {
            var userManager = TestHelpers.CreateUserManagerWithUser(out var user, true, name: name);
            var options = CreateOptions();
            if (isAdmin)
                await SetUpAdministrator(userManager, user, name);

            var tokenGenerator = new TokenGenerator(userManager, options, await TestHelpers.CreateRoleManager(name));
            var token = await tokenGenerator.GetTokenAsync(user);

            var claims = ValidateToken(user, token, options.Value.TokenSecret, options.Value.TokenIssuer, options.Value.TokenAudience);
            if (isAdmin)
                claims.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
            else
                claims.Claims.Should().NotContain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        }
    }
}