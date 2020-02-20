using Microsoft.Extensions.Logging;
using PocketBookServer.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace PocketBookServer.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }

    public class EmailSender : IEmailSender
    {
        private readonly ISendGridClient _client;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ISendGridClient client, ILogger<EmailSender> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var msg = new SendGridMessage
            {
                From = new EmailAddress("no-reply@yourpocketbook.uk", "PocketBook"),
                Subject = subject,
                HtmlContent = htmlMessage
            };
            msg.AddTo(new EmailAddress(email));

            // Disable click tracking
            msg.SetClickTracking(false, false);

            await _client.SendEmailAsync(msg);

            _logger.LogInformation(EventIds.EmailSent, "Send Email : email sent");
        }
    }

    public class EmailSenderOptions
    {
        public string SendGridKey { get; set; }
    }
}