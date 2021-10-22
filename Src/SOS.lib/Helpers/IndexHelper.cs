using SOS.Lib.Extensions;

namespace SOS.Lib.Helpers
{
    public static class IndexHelper
    {
        /// <summary>
        /// Get name of a index
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="indexPrefix"></param>
        /// <param name="toggleable"></param>
        /// <param name="instance"></param>
        /// <param name="protectedObservations"></param>
        /// <returns></returns>
        public static string GetIndexName<TEntity>(string indexPrefix, bool toggleable, byte instance,
            bool protectedObservations) =>
            $"{(string.IsNullOrEmpty(indexPrefix) ? "" : $"{indexPrefix}-")}{GetInstanceName<TEntity>(toggleable, instance, protectedObservations)}"
                .ToLower();

        /// <summary>
        /// Get name of index
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="toggleable"></param>
        /// <param name="instance"></param>
        /// <param name="protectedObservations"></param>
        /// <returns></returns>
        public static string
            GetIndexName<TEntity>(bool toggleable, byte instance, bool protectedObservations) =>
            GetIndexName<TEntity>(null, toggleable, instance, protectedObservations);

        /// <summary>
        /// Get name of index
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="instance"></param>
        /// <param name="protectedObservations"></param>
        /// <returns></returns>
        public static string
            GetIndexName<TEntity>(byte instance, bool protectedObservations) =>
            GetIndexName<TEntity>(null, true, instance, protectedObservations);

        /// <summary>
        /// Get name of index
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <returns></returns>
        public static string GetIndexName<TEntity>() =>
            GetIndexName<TEntity>(null, false, 0, false);

        /// <summary>
        /// Get name of instance
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="toggleable"></param>
        /// <param name="instance"></param>
        /// <param name="protectedObservations"></param>
        /// <returns></returns>
        public static string GetInstanceName<TEntity>(bool toggleable, byte instance, bool protectedObservations)
        {
            var instanceName = $"{typeof(TEntity).Name.UntilNonAlfanumeric()}";
            if (protectedObservations)
            {
                instanceName += "-protected";
            }

            instanceName += $"{(toggleable ? $"-{instance}" : string.Empty)}";
            return instanceName;
        }
    }
}