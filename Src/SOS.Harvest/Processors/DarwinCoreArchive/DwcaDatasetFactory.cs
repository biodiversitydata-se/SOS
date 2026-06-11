using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Lib.Configuration.Process;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.DataStewardship.Dataset;
using SOS.Lib.Models.Shared;

namespace SOS.Harvest.Processors.DarwinCoreArchive;

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

    public Dataset? CreateProcessedDataset(DwcVerbatimDataset verbatimDataset)
    {
        try
        {
            if (verbatimDataset == null)
            {
                return null;
            }

            return CleanVerbatimDataset(verbatimDataset);
        }
        catch (Exception e)
        {
            throw new Exception($"Error when processing DwC verbatim dataset with Identifier={verbatimDataset.Identifier ?? "null"}", e);
        }
    }

    public Dataset? CreateProcessedDataset(Dataset verbatimDataset)
    {
        try
        {
            return CleanDataset(verbatimDataset);
        }
        catch (Exception e)
        {
            throw new Exception($"Error when processing DwC verbatim dataset with Identifier={verbatimDataset.Identifier ?? "null"}", e);
        }
    }

    public static Dataset? CleanDataset(Dataset? dataset)
    {
        if (dataset == null)
        {
            return null;
        }

        return new Dataset
        {
            AccessRights = dataset.AccessRights,
            Assigner = dataset.Assigner,
            Creator = dataset.Creator,
            DataStewardship = dataset.DataStewardship?.Clean(),
            Description = dataset.Description?.Clean(),
            EndDate = dataset.EndDate.HasValue ? dataset.EndDate.Value.ToUniversalTime() : null,
            EventIds = dataset.EventIds,
            Identifier = dataset.Identifier,
            Language = dataset.Language?.Clean(),
            Metadatalanguage = dataset.Metadatalanguage?.Clean(),
            Methodology = dataset.Methodology,
            OwnerinstitutionCode = dataset.OwnerinstitutionCode,
            Project = dataset.Project,
            Publisher = dataset.Publisher,
            Purpose = dataset.Purpose,
            ProgrammeArea = dataset.ProgrammeArea,
            Spatial = dataset.Spatial?.Clean(),
            StartDate = dataset.StartDate.HasValue ? dataset.StartDate.Value.ToUniversalTime() : null,
            Title = dataset.Title?.Clean(),
            DescriptionAccessRights = dataset.DescriptionAccessRights?.Clean(),
            License = dataset.License?.Clean(),
        };
    }

    private static Dataset? CleanVerbatimDataset(DwcVerbatimDataset? dataset)
    {
        if (dataset == null)
        {
            return null;
        }

        return new Dataset
        {
            AccessRights = dataset.AccessRights,
            Assigner = dataset.Assigner,
            Creator = dataset.Creator,
            DataStewardship = dataset.DataStewardship?.Clean(),
            Description = dataset.Description?.Clean(),
            EndDate = dataset.EndDate.HasValue ? dataset.EndDate.Value.ToUniversalTime() : null,
            EventIds = dataset.EventIds,
            Identifier = dataset.Identifier,
            Language = dataset.Language?.Clean(),
            Metadatalanguage = dataset.Metadatalanguage?.Clean(),
            Methodology = dataset.Methodology,
            OwnerinstitutionCode = dataset.OwnerinstitutionCode,
            Project = dataset.Project,
            ProgrammeArea = dataset.ProgrammeArea,
            Publisher = dataset.Publisher,
            Purpose = dataset.Purpose,
            Spatial = dataset.Spatial?.Clean(),
            StartDate = dataset.StartDate.HasValue ? dataset.StartDate.Value.ToUniversalTime() : null,
            Title = dataset.Title?.Clean(),
            DescriptionAccessRights = dataset.DescriptionAccessRights?.Clean(),
            License = dataset.License?.Clean(),
        };
    }
}
