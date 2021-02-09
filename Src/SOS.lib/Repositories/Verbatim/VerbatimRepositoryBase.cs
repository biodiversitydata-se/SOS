using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SOS.Lib.Database.Interfaces;
using SOS.Lib.Models.Interfaces;
using SOS.Lib.Repositories.Verbatim.Interfaces;

namespace SOS.Lib.Repositories.Verbatim
{
    /// <summary>
    ///     Species data service
    /// </summary>
    public class VerbatimRepositoryBase<TEntity, TKey> : RepositoryBase<TEntity, TKey>, IVerbatimRepositoryBase<TEntity, TKey> where TEntity : IEntity<TKey>
    {
        /// <summary>
        /// Store data in temporary collection and switch it on success 
        /// </summary>
        public bool TempMode { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="importClient"></param>
        /// <param name="tempMode"></param>
        /// <param name="logger"></param>
        protected VerbatimRepositoryBase(
            IVerbatimClient importClient,
            ILogger<VerbatimRepositoryBase<TEntity, TKey>> logger) : base(importClient, logger)
        {
        }

        /// <summary>
        /// Name of collection
        /// </summary>
        protected override string CollectionName => $"{base.CollectionName}{(TempMode ? "_temp" : "")}";

        /// <inheritdoc />
        public async Task<bool> PermanentizeCollectionAsync()
        {
            if (!TempMode || !await CheckIfCollectionExistsAsync())
            {
                return true;
            }

            // Switch off temp mode
            TempMode = false;
            var permanentCollectionName = CollectionName;

            // Check if permanent collection exists
            if (await CheckIfCollectionExistsAsync())
            {
                // Delete permanent collection
                await DeleteCollectionAsync();
            }

            // Re set temp mode
            TempMode = true;

            await Database.RenameCollectionAsync(CollectionName, permanentCollectionName);
            return true;
        }
    }
}