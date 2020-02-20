namespace PocketBookServer.Models.Account
{
    public class UpdatePassword
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}