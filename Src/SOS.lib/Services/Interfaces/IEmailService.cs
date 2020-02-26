using System;
using SOS.Lib.Models.Email;

namespace SOS.Lib.Services.Interfaces
{
    /// <summary>
    /// Email service interface
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Send a message
        /// </summary>
        /// <param name="message"></param>
        void Send(EmailMessage message);
    }
}
