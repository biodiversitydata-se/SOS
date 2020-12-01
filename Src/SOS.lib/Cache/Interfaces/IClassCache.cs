namespace SOS.Lib.Cache.Interfaces
{
    /// <summary>
    /// Class holding a entity 
    /// </summary>
    /// <typeparam name="TClass"></typeparam>
    public interface IClassCache<TClass>
    {
        /// <summary>
        /// Get cached entity
        /// </summary>
        /// <returns></returns>
        TClass Get();

        /// <summary>
        /// Set entity cache
        /// </summary>
        /// <param name="entity"></param>
        void Set(TClass entity);
    }
}
