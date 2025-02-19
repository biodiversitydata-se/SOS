﻿//using SOS.Lib.Models.Shared;
//using SOS.Harvest.Managers.Interfaces;
//using SOS.Harvest.Processors.Interfaces;
//using SOS.Lib.Configuration.Process;
//using SOS.Lib.Models.Processed.DataStewardship.Dataset;

//namespace SOS.Harvest.Processors.Artportalen
//{
//    /// <summary>
//    ///     Artportalen dataset factory.
//    /// </summary>
//    public class ArtportalenDatasetFactory : DatasetFactoryBase, IDatasetFactory<DwcVerbatimDataset>
//    {        
//        /// <summary>
//        /// Constructor
//        /// </summary>
//        /// <param name="dataProvider"></param>        
//        public ArtportalenDatasetFactory(
//            DataProvider dataProvider,            
//            IProcessTimeManager processTimeManager,
//            ProcessConfiguration processConfiguration) : base(dataProvider, processTimeManager, processConfiguration)
//        {

//        }

//        public Dataset CreateProcessedDataset(DwcVerbatimDataset verbatimDataset)
//        {
//            try
//            {
//                if (verbatimDataset == null)
//                {
//                    return null;
//                }

//                var id = $"urn:lsid:{DataProvider.ChecklistIdentifier}:Dataset:{verbatimDataset.Identifier}";

//                var observationDataset = new Dataset
//                {
//                    Id = id, // verbatimDataset.Identifier,
//                    AccessRights = verbatimDataset.AccessRights,
//                    Assigner = verbatimDataset.Assigner,
//                    Creator = verbatimDataset.Creator,
//                    DataStewardship = verbatimDataset.DataStewardship,
//                    Description = verbatimDataset.Description,
//                    EndDate = verbatimDataset.EndDate.HasValue ? verbatimDataset.EndDate.Value.ToUniversalTime() : null,
//                    EventIds = verbatimDataset.EventIds,                    
//                    Identifier = verbatimDataset.Identifier,
//                    Language = verbatimDataset.Language,
//                    Metadatalanguage = verbatimDataset.Metadatalanguage,
//                    Methodology = verbatimDataset.Methodology,
//                    OwnerinstitutionCode = verbatimDataset.OwnerinstitutionCode,
//                    ProjectCode = verbatimDataset.ProjectCode,
//                    ProjectId = verbatimDataset.ProjectId,
//                    Publisher = verbatimDataset.Publisher,
//                    Purpose = verbatimDataset.Purpose,
//                    Spatial = verbatimDataset.Spatial,
//                    StartDate = verbatimDataset.StartDate.HasValue ? verbatimDataset.StartDate.Value.ToUniversalTime() : null,
//                    Title = verbatimDataset.Title
//                };

//                return observationDataset;
//            }
//            catch (Exception e)
//            {
//                throw new Exception($"Error when processing DwC verbatim dataset with Identifier={verbatimDataset.Identifier ?? "null"}", e);
//            }
//        }       
//    }
//}