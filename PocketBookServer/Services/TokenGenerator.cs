using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PocketBookServer.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace PocketBookServer.Services
{
    public interface ITokenGenerator
    {
        Task<string> GetTokenAsync(ApplicationUser user);
    }

    public class TokenGenerator : ITokenGenerator
    {
        private readonly TokenGeneratorOptions _options;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenGenerator(UserManager<ApplicationUser> userManager, IOptions<TokenGeneratorOptions> optionsAccessor, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _options = optionsAccessor.Value;
            _roleManager = roleManager;
        }

        public async Task<string> GetTokenAsync(ApplicationUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_options.TokenSecret);
            var userClaims = await _userManager.GetClaimsAsync(user);
            var subject = new ClaimsIdentity(userClaims);
            subject.AddClaims(new[]
                {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.GivenName, user.RealName),
                    });

            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                var role = await _roleManager.FindByNameAsync("Admin");
                subject.AddClaims(await _roleManager.GetClaimsAsync(role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = _options.TokenIssuer,
                Audience = _options.TokenAudience
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }
    }

    public class TokenGeneratorOptions
    {
        public string TokenAudience { get; set; }
        public string TokenIssuer { get; set; }
        public string TokenSecret { get; set; }
    }
}