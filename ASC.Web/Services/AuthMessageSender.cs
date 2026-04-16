using ASC.Web.Configuration;
using ASC.Web.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ASC.Solution.Services
{
    public class AuthMessageSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, ISmsSender
    {
        private readonly IOptions<ApplicationSettings> _settings;

        public AuthMessageSender(IOptions<ApplicationSettings> settings)
        {
            _settings = settings;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            var emailMessage = new MimeMessage();

            // Người gửi lấy từ SMTPAccount trong AppSettings
            emailMessage.From.Add(new MailboxAddress("ASC Admin", _settings.Value.SMTPAccount));

            // Người nhận
            emailMessage.To.Add(new MailboxAddress("", email));

            emailMessage.Subject = subject;
            // Dùng "html" để render được link reset mật khẩu
            emailMessage.Body = new TextPart("html") { Text = message };

            using (var client = new SmtpClient())
            {
                // Sử dụng SMTPServer (smtp.gmail.com) và SMTPPort (587) từ config
                await client.ConnectAsync(_settings.Value.SMTPServer, _settings.Value.SMTPPort, MailKit.Security.SecureSocketOptions.StartTls);

                // Sử dụng App Password vừa cập nhật
                await client.AuthenticateAsync(_settings.Value.SMTPAccount, _settings.Value.SMTPPassword);

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
        }

        public Task SendSmsAsync(string number, string message)
        {
            return Task.FromResult(0);
        }
    }
}