using System;
using System.Threading.Tasks;
using Application.DTOs.Email;
using Application.Exceptions;
using Application.Interfaces.Services;
using Domain.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        public MailServiceSettings mailSettings { get; }
        public ILogger<EmailService> logger { get; }

        public EmailService(IOptions<MailServiceSettings> mailSettings, ILogger<EmailService> logger)
        {
            this.mailSettings = mailSettings.Value;
            this.logger = logger;
        }

        public async Task SendAsync(EmailRequest request)
        {
            try
            {
                var email = new MimeMessage();
                email.Sender = new MailboxAddress(this.mailSettings.DisplayName, request.From ?? this.mailSettings.EmailFrom);
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = request.Body;
                email.Body = bodyBuilder.ToMessageBody();

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(this.mailSettings.SmtpHost, this.mailSettings.SmtpPort, SecureSocketOptions.StartTls);
                    smtpClient.Authenticate(this.mailSettings.SmtpUser, this.mailSettings.SmtpPass);
                    await smtpClient.SendAsync(email);
                    smtpClient.Disconnect(true);
                }
            }
            catch (Exception exception)
            {
                this.logger.LogError(exception.Message, exception);
                throw new ApiException("EmailService Error", exception.Message);
            }
        }
    }
}