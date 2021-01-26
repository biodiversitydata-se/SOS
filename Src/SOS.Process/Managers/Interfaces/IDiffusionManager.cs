using SOS.Lib.Models.Processed.Observation;

namespace SOS.Process.Managers.Interfaces
{
    public interface IDiffusionManager
    {
        /// <summary>
        ///  Diffuse a observation
        /// </summary>
        /// <param name="observation"></param>
        /// <param name="protectionLevel"></param>
        /// <returns></returns>
        Observation DiffuseObservation(Observation observation, int protectionLevel);
    }
}
