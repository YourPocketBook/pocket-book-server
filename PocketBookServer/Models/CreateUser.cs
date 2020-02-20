namespace PocketBookServer.Models
{
    public class CreateUser
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public bool UpdateEmailConsentGiven { get; set; }
    }
}