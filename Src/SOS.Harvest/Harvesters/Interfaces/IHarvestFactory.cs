namespace SOS.Harvest.Harvesters.Interfaces
{
    /// <summary>
    /// Harvest factory interface
    /// </summary>
    public interface IHarvestFactory<in TE, TV>
    {
        /// <summary>
        /// Cast entities to verbatim
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task<IEnumerable<TV>> CastEntitiesToVerbatimsAsync(TE entities);
    }
}
