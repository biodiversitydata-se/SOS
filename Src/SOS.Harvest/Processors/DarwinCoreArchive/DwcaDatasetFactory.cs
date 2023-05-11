using SOS.Lib.Models.Shared;
using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Extensions;

namespace SOS.Harvest.Processors.DarwinCoreArchive
{
    /// <summary>
    ///     DwC-A dataset factory.
    /// </summary>
    public class DwcaDatasetFactory : DatasetFactoryBase, IDatasetFactory<DwcVerbatimDataset>
    {        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>        
        public DwcaDatasetFactory(
            DataProvider dataProvider,            
            IProcessTimeManager processTimeManager,
            ProcessConfiguration processConfiguration) : base(dataProvider, processTimeManager, processConfiguration)
        {
            
        }

        public Dataset CreateProcessedDataset(DwcVerbatimDataset verbatimDataset)
        {
            try
            {
                if (verbatimDataset == null)
                {
                    return null;
                }
                
                var observationDataset = new Dataset
                {                    
                    AccessRights = verbatimDataset.AccessRights,
                    Assigner = verbatimDataset.Assigner,
                    Creator = verbatimDataset.Creator,
                    DataStewardship = verbatimDataset.DataStewardship?.Clean(),
                    Description = verbatimDataset.Description?.Clean(),
                    EndDate = verbatimDataset.EndDate.HasValue ? verbatimDataset.EndDate.Value.ToUniversalTime() : null,
                    EventIds = verbatimDataset.EventIds,                    
                    Identifier = verbatimDataset.Identifier,
                    //Identifier = $"urn:lsid:{DataProvider.Identifier}:Dataset:{verbatimDataset.Identifier}",
                    Language = verbatimDataset.Language?.Clean(),
                    Metadatalanguage = verbatimDataset.Metadatalanguage?.Clean(),
                    Methodology = verbatimDataset.Methodology,
                    OwnerinstitutionCode = verbatimDataset.OwnerinstitutionCode,
                    Project = verbatimDataset.Project,                    
                    Publisher = verbatimDataset.Publisher,
                    Purpose = verbatimDataset.Purpose,
                    Spatial = verbatimDataset.Spatial?.Clean(),
                    StartDate = verbatimDataset.StartDate.HasValue ? verbatimDataset.StartDate.Value.ToUniversalTime() : null,
                    Title = verbatimDataset.Title?.Clean(),
                    DescriptionAccessRights = verbatimDataset.DescriptionAccessRights?.Clean(),
                };

                return observationDataset;
            }
            catch (Exception e)
            {
                throw new Exception($"Error when processing DwC verbatim dataset with Identifier={verbatimDataset.Identifier ?? "null"}", e);
            }
        }
    }
}