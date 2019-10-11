using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Import.Factories.Interfaces;


namespace SOS.Import.Services
{
    /// <summary>
    /// Main service
    /// </summary>
    public class ImportService : Interfaces.IImportService
    {
        private readonly ISpeciesPortalSightingFactory _speciesPortalSightingFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="speciesPortalSightingFactory"></param>
        public ImportService(ISpeciesPortalSightingFactory speciesPortalSightingFactory)
        {
            _speciesPortalSightingFactory = speciesPortalSightingFactory ?? throw new ArgumentNullException(nameof(speciesPortalSightingFactory));
        }

        /// <inheritdoc />
        public async Task<bool> ImportAsync(int sources)
        {
            // Create task list
            var importTasks = new List<Task<bool>>
            {
                _speciesPortalSightingFactory.AggregateAreasAsync() // Make sure we have the latest areas
            };

            // Add species portal import if first bit is set
            if ((sources & 1) > 0)
            {
                importTasks.Add(_speciesPortalSightingFactory.AggregateAsync());
            }

            // Run all tasks async
            await Task.WhenAll(importTasks);

            // return result of all imports
            return importTasks.All(t => t.Result);
        }
    }
}
