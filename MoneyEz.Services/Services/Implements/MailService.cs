using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MoneyEz.Services.BusinessModels.EmailModels;
using MoneyEz.Services.Services.Interfaces;
using MoneyEz.Services.Settings;
using SendGrid.Helpers.Mail;
using SendGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyEz.Services.Services.Implements
{
    public class MailService : IMailService
    {

        private readonly Settings.MailSettings _mailSettings;
        private readonly SendgridConfig _sendgridConfigs;

        public MailService(IOptions<Settings.MailSettings> mailSettings, IOptions<SendgridConfig> sendgridConfigs)
        {
            _mailSettings = mailSettings.Value;
            _sendgridConfigs = sendgridConfigs.Value;
        }
        public async Task SendEmailAsync(MailRequest mailRequest)
        {
            // setup mail
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_mailSettings.DisplayName, _mailSettings.Mail));
            email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
            email.Subject = mailRequest.Subject;
            var builder = new BodyBuilder();

            // attachment
            if (mailRequest.Attachments != null)
            {
                byte[] fileBytes;
                foreach (var file in mailRequest.Attachments)
                {
                    if (file.Length > 0)
                    {
                        using (var ms = new MemoryStream())
                        {
                            file.CopyTo(ms);
                            fileBytes = ms.ToArray();
                        }
                        builder.Attachments.Add(file.FileName, fileBytes, ContentType.Parse(file.ContentType));
                    }
                }
            }

            // body
            builder.HtmlBody = mailRequest.Body;
            email.Body = builder.ToMessageBody();
            using var smtp = new SmtpClient();
            smtp.Connect(_mailSettings.Host, _mailSettings.Port, SecureSocketOptions.StartTls);
            smtp.Authenticate(_mailSettings.Mail, _mailSettings.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }

        public async Task SendEmailAsync_v2(MailRequest mailRequest)
        {
            var apiKey = _sendgridConfigs.ApiKey;
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_sendgridConfigs.FromEmail, _sendgridConfigs.FromName);
            var subject = mailRequest.Subject;
            var to = new EmailAddress(mailRequest.ToEmail);
            var plainTextContent = "";
            var htmlContent = mailRequest.Body;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}
