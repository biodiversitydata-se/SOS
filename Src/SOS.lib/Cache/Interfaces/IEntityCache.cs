namespace SOS.Lib.Cache.Interfaces
{
    /// <summary>
    /// Class holding a entity 
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IEntityCache<TEntity>
    {
        /// <summary>
        /// Get cached entity
        /// </summary>
        /// <returns></returns>
        TEntity Get();

        /// <summary>
        /// Set entity cache
        /// </summary>
        /// <param name="entity"></param>
        void Set(TEntity entity);
    }
}
