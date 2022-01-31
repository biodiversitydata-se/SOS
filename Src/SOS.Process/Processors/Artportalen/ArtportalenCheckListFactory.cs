using System;
using System.Linq;
using NetTopologySuite.Geometries;
using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.CheckList;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Lib.Models.Verbatim.Artportalen;
using SOS.Process.Processors.Interfaces;
using Area = SOS.Lib.Models.Processed.Observation.Area;
using Location = SOS.Lib.Models.Processed.Observation.Location;

namespace SOS.Process.Processors.Artportalen
{
    public class ArtportalenCheckListFactory : CheckListFactoryBase, ICheckListFactory<ArtportalenCheckListVerbatim>
    {
        private readonly DataProvider _dataProvider;

        /// <summary>
        /// Cast verbatim area to processed area
        /// </summary>
        /// <param name="area"></param>
        /// <returns></returns>
        private static Area CastToArea(GeographicalArea area)
        {
            if (area == null)
            {
                return null;
            }

            return new Area
            {
                FeatureId = area.FeatureId,
                Name = area.Name
            };
        }

        /// <summary>
        /// Create location object
        /// </summary>
        /// <param name="verbatim"></param>
        /// <returns></returns>
        private Location CreateLocation(ArtportalenCheckListVerbatim verbatim)
        {
            var location = new Location();

            if (verbatim.Site == null)
            {
                location.Locality = verbatim.Name;
                AddPositionData(location, verbatim.OccurrenceXCoord, verbatim.OccurrenceYCoord,
                    CoordinateSys.Rt90_25_gon_v, 0, 0);

                return location;
            }

            var point = (Point)verbatim.Site?.Point?.ToGeometry();

            var site = verbatim.Site;
            location.Attributes.CountyPartIdByCoordinate = site.CountyPartIdByCoordinate;
            location.Attributes.ProvincePartIdByCoordinate = site.ProvincePartIdByCoordinate;
            location.County = CastToArea(site?.County);
            location.Locality = site.Name.Trim();
            location.LocationId = $"urn:lsid:artportalen.se:site:{site?.Id}";
            location.Municipality = CastToArea(site?.Municipality);
            location.Parish = CastToArea(site?.Parish);
            location.Province = CastToArea(site?.Province);
            AddPositionData(location, site.XCoord,
                site.YCoord,
                CoordinateSys.WebMercator,
                point,
                site.PointWithBuffer,
                site.Accuracy,
                0);

            return location;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dataProvider"></param>
        public ArtportalenCheckListFactory(
            DataProvider dataProvider)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
        }

        /// <summary>
        ///     Cast verbatim check list to processed data model
        /// </summary>
        /// <param name="verbatimCheckList"></param>
        /// <returns></returns>
        public CheckList CreateProcessedCheckList(ArtportalenCheckListVerbatim verbatimCheckList)
        {
            try
            {
                if (verbatimCheckList == null)
                {
                    return null;
                }

                var id = $"urn:lsid:artportalen.se:Checklist:{verbatimCheckList.Id}";
                return new CheckList
                {
                    ArtportalenInternal = new ApInternal()
                    {
                        CheckListId = verbatimCheckList.Id,
                        ParentTaxonId = verbatimCheckList.ParentTaxonId,
                        UserId = verbatimCheckList.ControlingUserId
                    },
                    DataProviderId = _dataProvider.Id,
                    Id = id,
                    Event = new Event
                    {
                        EventId = id,
                        EndDate = verbatimCheckList.EndDate,
                        StartDate = verbatimCheckList.StartDate
                    },
                    Location = CreateLocation(verbatimCheckList),
                    Modified = verbatimCheckList.EditDate,
                    Name = verbatimCheckList.Name,
                    OccurrenceIds =
                        verbatimCheckList.SightingIds?.Select(sId => $"urn:lsid:artportalen.se:Sighting:{sId}"),
                    Project = ArtportalenFactoryHelper.CreateProcessedProject(verbatimCheckList.Project),
                    RecordedBy = verbatimCheckList.ControllingUser,
                    RegisterDate = verbatimCheckList.RegisterDate,
                    TaxonIds = verbatimCheckList.TaxonIds,
                    TaxonIdsFound = verbatimCheckList.TaxonIdsFound
                };
            }
            catch (Exception e)
            {
                throw new Exception($"Error when processing Artportalen verbatim check list with Id={verbatimCheckList.Id}", e);
            }
        }
    }
}