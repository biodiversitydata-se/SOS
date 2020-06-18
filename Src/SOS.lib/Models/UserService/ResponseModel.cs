using System.Collections.Generic;

namespace SOS.Lib.Models.UserService
{
    public class ResponseModel<T>
    {
        public bool Success { get; set; }

        public T Result { get; set; }
       
        public IEnumerable<MessageModel> Messages { get; set; }
    }
}
