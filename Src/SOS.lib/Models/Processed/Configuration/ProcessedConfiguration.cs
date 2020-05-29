using SOS.Lib.Models.Interfaces;

namespace SOS.Lib.Models.Processed.Configuration
{
    /// <summary>
    ///     Process configuration
    /// </summary>
    public class ProcessedConfiguration : IEntity<byte>
    {
        /// <summary>
        ///     Active instance 0 or 1
        /// </summary>
        public byte ActiveInstance { get; set; }

        /// <summary>
        ///     Id of configuration (always 0)
        /// </summary>
        public byte Id { get; set; }
    }
}