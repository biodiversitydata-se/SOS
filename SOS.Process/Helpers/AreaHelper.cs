using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SOS.Lib.Enums;
using SOS.Lib.Models.DarwinCore;
using SOS.Lib.Models.Verbatim.Shared;
using SOS.Process.Repositories.Source.Interfaces;

namespace SOS.Process.Helpers
{
    public class AreaHelper : Interfaces.IAreaHelper
    {
        private readonly IAreaVerbatimRepository _areaVerbatimRepository;
        private readonly IDictionary<string, IEnumerable<Area>> _areaCache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="areaVerbatimRepository"></param>
        public AreaHelper(IAreaVerbatimRepository areaVerbatimRepository)
        {
            _areaVerbatimRepository = areaVerbatimRepository ?? throw new ArgumentNullException(nameof(areaVerbatimRepository));
            _areaCache = new ConcurrentDictionary<string, IEnumerable<Area>>();
        }

        public async Task AddAreaDataToDarwinCoreAsync(IEnumerable<DarwinCore<DynamicProperties>> darwinCoreModels)
        {
            if (!darwinCoreModels?.Any() ?? true)
            {
                return;
            }

            foreach (var dwcModel in darwinCoreModels)
            {
                if (dwcModel.Location == null || (dwcModel.Location.DecimalLatitude.Equals(0) && dwcModel.Location.DecimalLongitude.Equals(0)))
                {
                    continue;
                }

                // Round coordinates to 5 decimals (roughly 1m)
                var key = $"{Math.Round(dwcModel.Location.DecimalLongitude, 5)}-{Math.Round(dwcModel.Location.DecimalLatitude, 5)}";

                // Try to get areas from cache
                _areaCache.TryGetValue(key, out var areas);

                // If areas not found for that position, try to get from repository
                if (areas == null)
                {
                    areas = await _areaVerbatimRepository.GetAreasByCoordinatesAsync(
                        dwcModel.Location.DecimalLongitude, 
                        dwcModel.Location.DecimalLatitude);

                    _areaCache.Add(key, areas);
                }

                if (areas == null)
                {
                    continue;
                }

                foreach (var area in areas)
                {
                    switch (area.AreaType)
                    {
                        case AreaType.County:
                            dwcModel.Location.County = area.Name;
                            break;
                        case AreaType.Municipality:
                            dwcModel.Location.Municipality = area.Name;
                            break;
                        case AreaType.Parish:
                            if (dwcModel.DynamicProperties == null)
                            {
                                dwcModel.DynamicProperties = new DynamicProperties();
                            }
                            dwcModel.DynamicProperties.Parish = area.Name;
                            break;
                        case AreaType.Province:
                            dwcModel.Location.StateProvince = area.Name;
                            break;
                    }
                }
            }
        }
    }
}
