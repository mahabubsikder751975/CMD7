using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace Cloud7CMS.Models
{
    public class EmailSender : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailSender(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (var client = new SmtpClient(_smtpSettings.Host, _smtpSettings.Port))
            {
                client.EnableSsl = _smtpSettings.EnableSsl;
                client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);

                var message = new MailMessage(_smtpSettings.FromAddress, email, subject, htmlMessage)
                {
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
            }
        }

        //public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        //{
        //    using (var client = new SmtpClient())
        //    {
        //        client.Host = _smtpSettings.Host;
        //        client.Port = _smtpSettings.Port;
        //        client.EnableSsl = _smtpSettings.EnableSsl;
        //        client.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);

        //        var message = new MailMessage(_smtpSettings.FromAddress, email, subject, htmlMessage)
        //        {
        //            IsBodyHtml = true
        //        };

        //        await client.SendMailAsync(message);
        //    }
        //}
    }

}
