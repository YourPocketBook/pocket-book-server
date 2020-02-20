using Microsoft.AspNetCore.Identity;

namespace PocketBookServer.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string RealName { get; set; }
        public bool UpdateEmailConsentGiven { get; set; }
    }
}