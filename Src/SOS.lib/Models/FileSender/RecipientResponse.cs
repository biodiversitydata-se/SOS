
namespace SOS.Lib.Models.FileSender
{
    public class RecipientResponse
    {
        /// <summary>
        /// File download url
        /// </summary>
        public string Download_url { get; set; }

        /// <summary>
        /// Recipient email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Recipient id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Token
        /// </summary>
        public string Token { get; set; }
    }
}
