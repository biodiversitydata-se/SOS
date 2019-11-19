
using System.Threading.Tasks;

namespace SOS.Process.Services.Interfaces
{
    public interface IRuntimeService
    {
        /// <summary>
        /// Active data instance
        /// </summary>
        byte ActiveInstance { get; }

        /// <summary>
        /// Name of database to populate
        /// </summary>
        string DatabaseName { get; }

        /// <summary>
        /// Inactive data instance
        /// </summary>
        byte InactiveInstance { get; }

        /// <summary>
        /// Force update on this instance
        /// </summary>
        /// <param name="instance"></param>
        void OverrideInstance(byte instance);

        /// <summary>
        /// Initialize service
        /// </summary>
        bool Initialize();

        /// <summary>
        /// Toggle current data instance on success
        /// </summary>
        Task ToggleInstanceAsync();
    }
}
