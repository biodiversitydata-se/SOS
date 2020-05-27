using SOS.Lib.Enums;
using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Shared
{
    public class UserInternal
    {
        /// <summary>
        /// Constructor
        /// </summary>        
        public UserInternal()
        {
            
        }

        /// <summary>
        /// User Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User alias
        /// </summary>
        public string UserAlias { get; set; }        
    }
}
