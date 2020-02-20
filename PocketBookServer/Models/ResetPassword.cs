namespace PocketBookServer.Models
{
    public class ResetPassword
    {
        public string UserId { get; set; }
        public string Code { get; set; }
        public string NewPassword { get; set; }
    }
}
