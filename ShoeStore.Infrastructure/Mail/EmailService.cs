using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using ShoeStore.Application.Interfaces.Services;
using System;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ShoeStore.Infrastructure.Mail
{
    public class EmailService : IEmailService
    {
        private readonly MailSettings _settings;

        public EmailService(IOptions<MailSettings> options)
        {
            _settings = options.Value;
        }

        // Kiểm tra email hợp lệ
        private bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;
            try
            {
                var addr = MailboxAddress.Parse(email.Trim());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            if (!IsValidEmail(to))
            {
                // Không throw, chỉ log warning
                Console.WriteLine($"[Warning] Email không hợp lệ: '{to}'");
                return;
            }

            var email = new MimeMessage();
            email.Sender = new MailboxAddress(_settings.DisplayName, _settings.Mail);
            email.From.Add(new MailboxAddress(_settings.DisplayName, _settings.Mail));
            email.To.Add(MailboxAddress.Parse(to.Trim()));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlMessage };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_settings.Host, _settings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_settings.Mail, _settings.Password);
                await smtp.SendAsync(email);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Error] Gửi email thất bại: {ex.Message}");
                // Lưu file .eml như fallback
                System.IO.Directory.CreateDirectory("mailssave");
                var emailSaveFile = $"mailssave/{Guid.NewGuid()}.eml";
                await email.WriteToAsync(emailSaveFile);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}
