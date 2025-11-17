using SOS.Harvest.Managers.Interfaces;
using SOS.Harvest.Processors.Artportalen.Interfaces;
using SOS.Harvest.Processors.DarwinCoreArchive.Interfaces;
using SOS.Harvest.Processors.FishData.Interfaces;
using SOS.Harvest.Processors.iNaturalist;
using SOS.Harvest.Processors.iNaturalist.Interfaces;
using SOS.Harvest.Processors.Interfaces;
using SOS.Harvest.Processors.Kul.Interfaces;
using SOS.Harvest.Processors.Mvm.Interfaces;
using SOS.Harvest.Processors.Nors.Interfaces;
using SOS.Harvest.Processors.ObservationDatabase.Interfaces;
using SOS.Harvest.Processors.Sers.Interfaces;
using SOS.Harvest.Processors.Shark.Interfaces;
using SOS.Harvest.Processors.VirtualHerbarium.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Harvest.Managers;

public class ObservationProcessorManager : IObservationProcessorManager
{
    private readonly Dictionary<DataProviderType, IObservationProcessor> _processorByType;


    public ObservationProcessorManager(
        IArtportalenObservationProcessor artportalenObservationProcessor,
        IDwcaObservationProcessor dwcaObservationProcessor,
        IFishDataObservationProcessor fishDataObservationProcessor,
        IKulObservationProcessor kulObservationProcessor,
        IMvmObservationProcessor mvmObservationProcessor,
        INorsObservationProcessor norsObservationProcessor,
        IObservationDatabaseProcessor observationDatabaseProcessor,
        ISersObservationProcessor sersObservationProcessor,
        ISharkObservationProcessor sharkObservationProcessor,
        IVirtualHerbariumObservationProcessor virtualHerbariumObservationProcessor,
        IiNaturalistObservationProcessor iNaturalistObservationProcessor
        )
    {
        if (artportalenObservationProcessor == null) throw new ArgumentNullException(nameof(artportalenObservationProcessor));
        if (dwcaObservationProcessor == null) throw new ArgumentNullException(nameof(dwcaObservationProcessor));
        if (fishDataObservationProcessor == null) throw new ArgumentNullException(nameof(fishDataObservationProcessor));
        if (kulObservationProcessor == null) throw new ArgumentNullException(nameof(kulObservationProcessor));
        if (mvmObservationProcessor == null) throw new ArgumentNullException(nameof(mvmObservationProcessor));
        if (norsObservationProcessor == null) throw new ArgumentNullException(nameof(norsObservationProcessor));
        if (sersObservationProcessor == null) throw new ArgumentNullException(nameof(sersObservationProcessor));
        if (sharkObservationProcessor == null) throw new ArgumentNullException(nameof(sharkObservationProcessor));
        if (virtualHerbariumObservationProcessor == null) throw new ArgumentNullException(nameof(virtualHerbariumObservationProcessor));
        if (iNaturalistObservationProcessor == null) throw new ArgumentNullException(nameof(iNaturalistObservationProcessor));

        _processorByType = new Dictionary<DataProviderType, IObservationProcessor>
        {
            {DataProviderType.ArtportalenObservations, artportalenObservationProcessor},
            {DataProviderType.DwcA, dwcaObservationProcessor},
            {DataProviderType.BiologgObservations, dwcaObservationProcessor},
            {DataProviderType.FishDataObservations, fishDataObservationProcessor},
            {DataProviderType.KULObservations, kulObservationProcessor},
            {DataProviderType.MvmObservations, mvmObservationProcessor},
            {DataProviderType.NorsObservations, norsObservationProcessor},
            {DataProviderType.ObservationDatabase, observationDatabaseProcessor},
            {DataProviderType.SersObservations, sersObservationProcessor},
            {DataProviderType.SharkObservations, sharkObservationProcessor},
            {DataProviderType.VirtualHerbariumObservations, virtualHerbariumObservationProcessor},
            {DataProviderType.iNaturalistObservations, iNaturalistObservationProcessor}
        };
    }

    /// <inheritdoc/>
    public IObservationProcessor GetProcessor(DataProviderType dataProvider)
    {
        return _processorByType[dataProvider];
    }

}
