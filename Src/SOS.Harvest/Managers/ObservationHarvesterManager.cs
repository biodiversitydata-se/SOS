using SOS.Harvest.Harvesters.AquaSupport.FishData.Interfaces;
using SOS.Harvest.Harvesters.AquaSupport.Kul.Interfaces;
using SOS.Harvest.Harvesters.AquaSupport.Nors.Interfaces;
using SOS.Harvest.Harvesters.AquaSupport.Sers.Interfaces;
using SOS.Harvest.Harvesters.Artportalen.Interfaces;
using SOS.Harvest.Harvesters.Biologg.Interfaces;
using SOS.Harvest.Harvesters.DwC.Interfaces;
using SOS.Harvest.Harvesters.iNaturalist.Interfaces;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Harvest.Harvesters.Mvm.Interfaces;
using SOS.Harvest.Harvesters.ObservationDatabase.Interfaces;
using SOS.Harvest.Harvesters.Shark.Interfaces;
using SOS.Harvest.Harvesters.VirtualHerbarium.Interfaces;
using SOS.Harvest.Managers.Interfaces;
using SOS.Lib.Enums;

namespace SOS.Harvest.Managers
{
    public class ObservationHarvesterManager : IObservationHarvesterManager
    {
        private readonly IDictionary<DataProviderType, IObservationHarvester> _harvestersByType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="artportalenObservationHarvester"></param>
        /// <param name="biologgObservationHarvester"></param>
        /// <param name="dwcObservationHarvester"></param>
        /// <param name="fishDataObservationHarvester"></param>
        /// <param name="kulObservationHarvester"></param>
        /// <param name="mvmObservationHarvester"></param>
        /// <param name="norsObservationHarvester"></param>
        /// <param name="observationDatabaseHarvester"></param>
        /// <param name="sersObservationHarvester"></param>
        /// <param name="sharkObservationHarvester"></param>
        /// <param name="virtualHerbariumObservationHarvester"></param>
        /// <param name="iNaturalistObservationHarvester"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ObservationHarvesterManager(IArtportalenObservationHarvester artportalenObservationHarvester,
            IBiologgObservationHarvester biologgObservationHarvester,
            IDwcObservationHarvester dwcObservationHarvester,
            IFishDataObservationHarvester fishDataObservationHarvester,
            IKulObservationHarvester kulObservationHarvester,
            IMvmObservationHarvester mvmObservationHarvester,
            INorsObservationHarvester norsObservationHarvester,
            IObservationDatabaseHarvester observationDatabaseHarvester,
            ISersObservationHarvester sersObservationHarvester,
            ISharkObservationHarvester sharkObservationHarvester,
            IVirtualHerbariumObservationHarvester virtualHerbariumObservationHarvester,
            IiNaturalistObservationHarvester iNaturalistObservationHarvester)
        {
            if (artportalenObservationHarvester == null) throw new ArgumentNullException(nameof(artportalenObservationHarvester));
            if (biologgObservationHarvester == null) throw new ArgumentNullException(nameof(biologgObservationHarvester));
            if (dwcObservationHarvester == null) throw new ArgumentNullException(nameof(dwcObservationHarvester));
            if (fishDataObservationHarvester == null) throw new ArgumentNullException(nameof(fishDataObservationHarvester));
            if (kulObservationHarvester == null) throw new ArgumentNullException(nameof(kulObservationHarvester));
            if (mvmObservationHarvester == null) throw new ArgumentNullException(nameof(mvmObservationHarvester));
            if (norsObservationHarvester == null) throw new ArgumentNullException(nameof(norsObservationHarvester));
            if (observationDatabaseHarvester == null) throw new ArgumentNullException(nameof(observationDatabaseHarvester));
            if (sersObservationHarvester == null) throw new ArgumentNullException(nameof(sersObservationHarvester));
            if (sharkObservationHarvester == null) throw new ArgumentNullException(nameof(sharkObservationHarvester));
            if (virtualHerbariumObservationHarvester == null) throw new ArgumentNullException(nameof(virtualHerbariumObservationHarvester));
            if (iNaturalistObservationHarvester == null) throw new ArgumentNullException(nameof(iNaturalistObservationHarvester));

            _harvestersByType = new Dictionary<DataProviderType, IObservationHarvester>
            {
                {DataProviderType.ArtportalenObservations, artportalenObservationHarvester},
                {DataProviderType.BiologgObservations, biologgObservationHarvester},
                {DataProviderType.DwcA, dwcObservationHarvester},
                {DataProviderType.FishDataObservations, fishDataObservationHarvester},
                {DataProviderType.KULObservations, kulObservationHarvester},
                {DataProviderType.MvmObservations, mvmObservationHarvester},
                {DataProviderType.NorsObservations, norsObservationHarvester},
                {DataProviderType.ObservationDatabase, observationDatabaseHarvester},
                {DataProviderType.SersObservations, sersObservationHarvester},
                {DataProviderType.SharkObservations, sharkObservationHarvester},
                {DataProviderType.VirtualHerbariumObservations, virtualHerbariumObservationHarvester},
                {DataProviderType.iNaturalistObservations, iNaturalistObservationHarvester}
            };
        }

        /// <inheritdoc/>
        public IObservationHarvester GetHarvester(DataProviderType dataProvider)
        {
            return _harvestersByType[dataProvider];
        }

    }
}
