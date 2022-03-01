using SOS.Lib.Models.Processed.Observation;

namespace SOS.Harvest.Managers.Interfaces
{
    public interface IDiffusionManager
    {
        /// <summary>
        ///  Diffuse a observation
        /// </summary>
        /// <param name="observation"></param>
        /// <returns></returns>
        void DiffuseObservation(Observation observation);
    }
}
