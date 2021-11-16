using System;
using System.Threading.Tasks;
using Application.DTOs.Email;
using Application.Exceptions;
using Application.Interfaces;
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
        public MailSettings _mailSettings { get; }
        public ILogger<EmailService> _logger { get; }

        public EmailService(IOptions<MailSettings> mailSettings, ILogger<EmailService> logger)
        {
            _mailSettings = mailSettings.Value;
            _logger = logger;
        }

        public async Task SendAsync(EmailRequest request)
        {
            try
            {
                var email = new MimeMessage();
                email.Sender = new MailboxAddress(_mailSettings.MailBoxName, request.From ?? _mailSettings.EmailFrom);
                email.To.Add(MailboxAddress.Parse(request.To));
                email.Subject = request.Subject;
                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = request.Body;
                email.Body = bodyBuilder.ToMessageBody();

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Connect(_mailSettings.SmtpHost, _mailSettings.SmtpPort, SecureSocketOptions.StartTls);
                    smtpClient.Authenticate(_mailSettings.SmtpUser, _mailSettings.SmtpPass);
                    await smtpClient.SendAsync(email);
                    smtpClient.Disconnect(true);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception.Message, exception);
                throw new ApiException("EmailService Error", exception.Message);
            }
        }
    }
}