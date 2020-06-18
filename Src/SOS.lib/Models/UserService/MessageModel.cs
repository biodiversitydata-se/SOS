using System.Net;

namespace SOS.Lib.Models.UserService
{
    public class MessageModel
    {
        public HttpStatusCode StatusCode{ get;set; }

        public string Text { get; set; }
    }
}
