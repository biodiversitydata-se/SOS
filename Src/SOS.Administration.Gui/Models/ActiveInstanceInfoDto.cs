namespace SOS.Administration.Gui.Models
{
    public class ActiveInstanceInfoDto
    { 
        /// <summary>
        ///     Active instance 0 or 1
        /// </summary>
        public byte ActiveInstance { get; set; }

        /// <summary>
        ///     Id of configuration (always 0)
        /// </summary>
        public string Id { get; set; }
    }
}
