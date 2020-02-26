using System;
using System.Linq;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using SOS.Lib.Configuration.Shared;
using SOS.Lib.Models.Email;

namespace SOS.Lib.Services
{
    /// <summary>
    /// Email service
    /// </summary>
    public class EmailService : Interfaces.IEmailService
    {
        private readonly EmailConfiguration _emailConfiguration;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="emailConfiguration"></param>
        public EmailService(EmailConfiguration emailConfiguration)
        {
            _emailConfiguration = emailConfiguration ?? throw new ArgumentNullException(nameof(emailConfiguration));
        }


        public void Send(EmailMessage emailMessage)
        {
			var message = new MimeMessage();
            message.To.AddRange(emailMessage.To.Select(to => new MailboxAddress(to)));
            message.From.Add(new MailboxAddress(_emailConfiguration.EmailFrom));

            message.Subject = emailMessage.Subject;
           
            // We will say we are sending HTML. But there are options for plaintext etc. 
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            using var emailClient = new SmtpClient();
            
            // Connect to smtp server
            emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, _emailConfiguration.UseSsl);

            // Remove any OAuth functionality as we won't be using it. 
            emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

            // Authenticate if we have credentials
            if (!string.IsNullOrEmpty(_emailConfiguration.SmtpUsername) &&
                !string.IsNullOrEmpty(_emailConfiguration.SmtpPassword))
            {
                emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
            }
            
            emailClient.Send(message);

            emailClient.Disconnect(true);
        }
	}
}
