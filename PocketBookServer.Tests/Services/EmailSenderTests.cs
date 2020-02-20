using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PocketBookServer.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;
using Xunit;

namespace PocketBookServer.Tests.Services
{
    public class EmailSenderTests
    {
        [Fact]
        public async Task SendEmailCallsSendGrid()
        {
            var client = new Mock<ISendGridClient>();

            var sender = new EmailSender(client.Object, NullLogger<EmailSender>.Instance);

            const string TestEmail = "test@test.com";
            const string TestSubject = "Test Subject";
            const string TestMessage = "<p>Hello World</p>";

            await sender.SendEmailAsync(TestEmail, TestSubject, TestMessage);

            client.Verify(c => c.SendEmailAsync(It.Is<SendGridMessage>(m =>
               m.From.Email == "no-reply@yourpocketbook.uk"
                && m.From.Name == "PocketBook"
                && m.Subject == TestSubject
                && m.HtmlContent == TestMessage
                && m.TrackingSettings.ClickTracking.Enable == false
            ), default));
        }
    }
}