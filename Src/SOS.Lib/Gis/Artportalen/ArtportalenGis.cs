//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using SOS.Lib.Models.Gis;
//using SOS.Lib.Services;
//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace SOS.Lib.Gis;

//internal class ArtportalenGis
//{

//    /// <summary>
//    /// Get view for viewing sightings search in map
//    /// </summary>
//    /// <param name="zoomlevel">
//    /// The zoom level
//    /// </param>
//    /// <param name="bbox">
//    /// The bbox.
//    /// </param>   
//    /// <param name="showCluster">
//    /// Show clusters on map.
//    /// </param>   
//    /// <param name="coordSys">
//    /// The coord Sys.
//    /// </param>
//    /// <returns>
//    /// somethingjfd ølaf
//    /// </returns>    
//    public ActionResult GetViewSightingAsMapGeoJsonSites(int zoomlevel, string bbox, bool? showCluster, int? coordSys)
//    {
//        SearchPresentation searchPresentation = GetSearchPresentationFromSession();
//        SearchCriterias searchCriterias = searchPresentation.StoredSearchCriterias.SearchCriterias;
//        searchCriterias.SightingTypeSearchGroupFlag = SightingTypeSearchGroupFlag.AggregatedOrReplacementOrOrdinaryOrAssessmentChild;

//        if (showCluster == null)
//        {
//            showCluster = true;
//        }

//        // Create model
//        bool includeProtectedSightings = searchCriterias.ShowProtectedSightings;
//        MapExtent extent = MapExtent.FromBboxString(bbox);

//        var sites = GetSites(searchCriterias, extent, includeProtectedSightings);

//        foreach (var site in sites)
//        {
//            if (site.Diffusion != 0)
//            {
//                site.IsPrivate = true;
//                site.SiteType = SiteType.Diffused;
//            }
//            else if (site.SiteType != SiteType.NotRecovered && site.SiteType != SiteType.NotPresent && site.SiteType != SiteType.Protected)
//            {
//                site.SiteType = 0;
//                site.IsPrivate = true;
//                ////site.ParentSiteId = 0;
//            }
//        }

//        bool completeResults = sites.Count < maxSiteResults;

//        IList<SiteMapView> adjustedSites = new List<SiteMapView>();
//        foreach (var site in sites)
//        {
//            bool lessImportant = false;
//            foreach (var sit in sites)
//            {
//                if (site.SiteId == sit.SiteId)
//                {
//                    if ((sit.SiteType == 0 &&
//                         site.SiteType != 0) ||
//                        (sit.SiteType == SiteType.NotRecovered &&
//                         site.SiteType != 0 &&
//                         site.SiteType != SiteType.NotRecovered) ||
//                        (sit.SiteType == SiteType.NotPresent &&
//                         site.SiteType != 0 &&
//                         site.SiteType != SiteType.NotRecovered &&
//                         site.SiteType != SiteType.NotPresent))
//                    {
//                        lessImportant = true;
//                    }
//                }
//            }

//            if (!lessImportant)
//            {
//                adjustedSites.Add(site);
//            }
//        }

//        int noSites = adjustedSites.Count();
//        return GetGeoJsonClusterSites(adjustedSites, (bool)showCluster, extent, zoomlevel, true, false, false, false, false, completeResults, noSites);
//    }

//    /// <summary>
//    /// Gets a GeoJsonSitesresult
//    /// </summary>
//    /// <param name="sites">
//    /// The non clustered sites
//    /// </param>
//    /// <param name="showCluster">
//    /// The site geometries.
//    /// </param>
//    /// <param name="extent">
//    /// The extent
//    /// </param>
//    /// <param name="zoomlevel">
//    /// The zoom level
//    /// </param> 
//    /// <param name="dividByYear">
//    /// Divide by year or not
//    /// </param>             
//    /// <param name="dividByValid">
//    /// Divide by validation or not
//    /// </param>             
//    /// <param name="dividByFlora">
//    /// Divide by flora guardian or not
//    /// </param>          
//    /// <param name="dividByFloraByYear">
//    /// Divide by flora guardian and year or not 
//    /// </param>   
//    /// <param name="dividByProportion">
//    /// Divide by proportion or not 
//    /// </param>  
//    /// <param name="completeResult">
//    /// Results are complete, i.e. less than max number of findings from db
//    /// </param>        
//    /// <param name="noSites">
//    /// Number of sites collected from db
//    /// </param>                            
//    /// <param name="ignoreSiteGeometry">
//    /// Divide by proportion or not tada 
//    /// </param>                                     
//    /// <returns>
//    /// GeoJeson thing
//    /// </returns>
//    private ActionResult GetGeoJsonClusterSites(IList<SiteMapView> sites, bool showCluster, MapExtent extent, int zoomlevel, bool dividByYear, bool dividByValid, bool dividByFlora, bool dividByFloraByYear, bool dividByProportion, bool completeResult, int noSites, bool ignoreSiteGeometry = false)
//    {
//        if (sites.Count <= 0)
//        {
//            return Content(string.Empty);
//        }
//        else
//        {
//            IList<SiteGeometry> siteGeomtriesToReturn = new List<SiteGeometry>();
//            if (showCluster)
//            {
//                var clusterResult = new SiteClusterer().ClusterSites(sites.ToList(), extent, zoomlevel);

//                if (!ignoreSiteGeometry)
//                {
//                    siteGeomtriesToReturn = SiteRepository.GetSiteGeometries(
//                        clusterResult.NonClusteredSites.Select(x => x.SiteId).ToArray());
//                }

//                return sites.Count <= 0 ? Content(string.Empty) : GetGeoJsonSites(clusterResult.NonClusteredSites, clusterResult.ClusteredSites, siteGeomtriesToReturn, dividByYear, dividByValid, dividByFlora, dividByFloraByYear, dividByProportion, completeResult, noSites);
//            }
//            else
//            {
//                IList<SiteCluster> tmp = new List<SiteCluster>();
//                if (!ignoreSiteGeometry)
//                {
//                    siteGeomtriesToReturn =
//                        SiteRepository.GetSiteGeometries(sites.ToList().Select(x => x.SiteId).ToArray());
//                }

//                return sites.Count <= 0 ? Content(string.Empty) : GetGeoJsonSites(sites.ToList(), tmp, siteGeomtriesToReturn, dividByYear, dividByValid, dividByFlora, dividByFloraByYear, dividByProportion, completeResult, noSites);
//            }
//        }
//    }

//    /// <summary>
//    /// Gets a GeoJsonSitesresult
//    /// </summary>
//    /// <param name="nonClusteredSites">
//    /// The non clustered sites
//    /// </param>
//    /// <param name="clusteredSites">
//    /// The clustered sites
//    /// </param>
//    /// <param name="siteGeometries">
//    /// The site geometries.
//    /// </param>
//    /// <param name="dividByYear">
//    /// Divide by year or not
//    /// </param>             
//    /// <param name="dividByValid">
//    /// Divide by validation or not
//    /// </param>             
//    /// <param name="dividByFlora">
//    /// Divide by flora guardian or not
//    /// </param>          
//    /// <param name="dividByFloraByYear">
//    /// Divide by flora guardian and year or not 
//    /// </param>    
//    /// <param name="dividByProportion">
//    /// Divide by proportion or not 
//    /// </param>  
//    /// <param name="completeResult">
//    /// Results are complete, i.e. less than max number of findings from db
//    /// </param>   
//    /// <param name="noSites">
//    /// Number of sites collected from db
//    /// </param>                  
//    /// <returns>
//    /// GeoJeson thing
//    /// </returns>
//    private ActionResult GetGeoJsonSites(IList<SiteMapView> nonClusteredSites, IList<SiteCluster> clusteredSites, IList<SiteGeometry> siteGeometries, bool dividByYear, bool dividByValid, bool dividByFlora, bool dividByFloraByYear, bool dividByProportion, bool completeResult, int noSites)
//    {
//        var databaseCoordinateSystemName = SiteService.GetDatabaseCoordinateSystemName();
//        var coordSystName = databaseCoordinateSystemName;

//        var siteIDs = (from sg in siteGeometries select sg.Site.Id).ToArray();

//        var geoJsonSite = (from cluster in clusteredSites
//                           orderby cluster.Accuracy descending
//                           select
//                           new
//                           {
//                               type = "Feature",
//                               id = -1,
//                               geometry = new { type = "Point", coordinates = new[] { cluster.XCoord, cluster.YCoord } },
//                               properties =
//                               new
//                               {
//                                   siteName = "Map_ZoomInToChooseOneSite".Localize().ToString(),
//                                   siteAreaDescription = string.Empty,
//                                   siteAreaName = string.Empty,
//                                   projectAndExternalIdDescription = string.Empty,
//                                   projectAndExternalId = string.Empty,
//                                   coordSystemName = coordSystName,
//                                   accuracyDescription = "Shared_Accuracy".Localize().ToString(),
//                                   accuracy = cluster.Accuracy,
//                                   parentId = 0,
//                                   siteType = SiteType.Cluster,
//                                   dividedByYearUntil = false,
//                                   dividedByYearAfter = false,
//                                   dividedByYearNotRecoveredUntil = false,
//                                   dividedByYearNotRecoveredAfter = false,
//                                   dividedByValid = false,
//                                   dividedByValidReject = false,
//                                   dividedByValidInProgress = false,
//                                   dividedByValidApproved = false,
//                                   dividedByUnValidInterest = false,
//                                   dividedByUnValidWarning = false,
//                                   dividedByFloraGuardian = false,
//                                   dividedByFloraGuardianOrdinary = false,
//                                   dividedByFloraGuardianNotRecovered = false,
//                                   dividedByFloraGuardianOrdinaryNotRecovered = false,
//                                   dividedByFloraGuardianCurrentYear = false,
//                                   dividedByFloraGuardian1To3YearsAgo = false,
//                                   dividedByFloraGuardianOld = false,
//                                   dividedByFloraGuardianCurrentYearNotRecovered = false,
//                                   dividedByFloraGuardian1To3YearsAgoNotRecovered = false,
//                                   dividedByFloraGuardianOldNotRecovered = false,
//                                   dividedByFloraGuardianArchivedSite = false,
//                                   dividedByProportionSize = false,
//                                   colorString = "#0099ff",
//                                   siteCoordinateStringPresentation = string.Empty,
//                                   siteIDs = string.Join(",", cluster.Sites.Select(x => x.SiteId).ToArray()),
//                                   proportionSize = 0,
//                                   proportionRotation = "0",
//                                   proportionHeight = "0",
//                                   proportionColour = string.Empty,
//                                   numberOfSightings = 0,
//                                   date = string.Empty,
//                                   diffusion = 0
//                               }
//                           }).Union(
//            from site in nonClusteredSites
//            orderby site.Accuracy descending
//            where !siteIDs.Contains(site.SiteId)
//            select
//            new
//            {
//                type = "Feature",
//                id = site.SiteId,
//                geometry =
//                new { type = "Point", coordinates = new[] { site.XCoord, site.YCoord } },
//                properties =
//                new
//                {
//                    siteName = string.Empty,
//                    siteAreaDescription = "Shared_Municipality".Localize().ToString(),
//                    siteAreaName = string.Empty,
//                    projectAndExternalIdDescription = "Shared_AssociatedProjects".Localize().ToString(),
//                    projectAndExternalId = string.Empty,
//                    coordSystemName = coordSystName,
//                    accuracyDescription = "Shared_Accuracy".Localize().ToString(),
//                    accuracy = site.Accuracy,
//                    parentId =
//                    site.ParentSiteId != null ? site.ParentSiteId.Value : SiteType.Private,
//                    siteType = site.SiteType >= SiteType.Private && site.SiteType <=
//                               SiteType.Parent
//                        ? SiteType.Private
//                        : site.SiteType,
//                    dividedByYearUntil = site.SiteType == SiteType.DividedByYearUntil && dividByYear,
//                    dividedByYearAfter = site.SiteType == SiteType.DividedByYearAfter && dividByYear,
//                    dividedByYearNotRecoveredUntil = site.SiteType == SiteType.DividedByYearUntilNotRecovered && dividByYear,
//                    dividedByYearNotRecoveredAfter = site.SiteType == SiteType.DividedByYearAfterNotRecovered && dividByYear,
//                    dividedByValid = dividByValid,
//                    dividedByValidReject = site.SiteType == SiteType.DividedByValidReject && dividByValid,
//                    dividedByValidInProgress = site.SiteType == SiteType.DividedByValidInProgress && dividByValid,
//                    dividedByValidApproved = site.SiteType == SiteType.DividedByValidApproved && dividByValid,
//                    dividedByUnValidInterest = site.SiteType == SiteType.DividedByUnValidInterest && dividByValid,
//                    dividedByUnValidWarning = site.SiteType == SiteType.DividedByUnValidWarning && dividByValid,
//                    dividedByFloraGuardian = site.SiteType == SiteType.DividedByFloraGuardian && dividByFlora,
//                    dividedByFloraGuardianOrdinary = site.SiteType == SiteType.DividedByFloraGuardianOrdinary && dividByFlora,
//                    dividedByFloraGuardianNotRecovered = site.SiteType == SiteType.DividedByFloraGuardianNotRecovered && dividByFlora,
//                    dividedByFloraGuardianOrdinaryNotRecovered = site.SiteType == SiteType.DividedByFloraGuardianOrdinaryNotRecovered && dividByFlora,
//                    dividedByFloraGuardianCurrentYear = site.SiteType == SiteType.DividedByFloraGuardianCurrentYear && dividByFloraByYear,
//                    dividedByFloraGuardian1To3YearsAgo = site.SiteType == SiteType.DividedByFloraGuardian1To3YearsAgo && dividByFloraByYear,
//                    dividedByFloraGuardianOld = site.SiteType == SiteType.DividedByFloraGuardianOld && dividByFloraByYear,
//                    dividedByFloraGuardianCurrentYearNotRecovered = site.SiteType == SiteType.DividedByFloraGuardianCurrentYearNotRecovered && dividByFloraByYear,
//                    dividedByFloraGuardian1To3YearsAgoNotRecovered = site.SiteType == SiteType.DividedByFloraGuardian1To3YearsAgoNotRecovered && dividByFloraByYear,
//                    dividedByFloraGuardianOldNotRecovered = site.SiteType == SiteType.DividedByFloraGuardianOldNotRecovered && dividByFloraByYear,
//                    dividedByFloraGuardianArchivedSite = site.ProjectId == WebConfig.FloraGuardianArchivedSitesProjectId,
//                    dividedByProportionSize = site.SiteType == SiteType.DividedByProportionSize && dividByProportion,
//                    colorString = site.SiteType >= SiteType.Private && site.SiteType <=
//                                  SiteType.Parent
//                        ? "#ffff00"
//                        : site.ColorCode,
//                    siteCoordinateStringPresentation = string.Empty,
//                    siteIDs = string.Empty,
//                    proportionSize = site.SizeOfPoint,
//                    proportionRotation = site.MigrationRotation,
//                    proportionHeight = site.MigrationHeight,
//                    proportionColour = site.Colour,
//                    numberOfSightings = site.NoSightings,
//                    date = site.Date,
//                    diffusion = site.Diffusion
//                }
//            });

//        foreach (var siteG in siteGeometries)
//        {
//            foreach (var site in nonClusteredSites)
//            {
//                if (siteG.Site.Id == site.SiteId)
//                {
//                    siteG.SiteType = site.SiteType;
//                    siteG.ProportionSize = site.SizeOfPoint;
//                    siteG.ProportionRotation = site.MigrationRotation;
//                    siteG.ProportionHeight = site.MigrationHeight;
//                    siteG.ProportionColour = site.Colour;
//                    siteG.NumberOfSightings = site.NoSightings;
//                }
//            }
//        }

//        var geoJsonSiteGeometries = from siteG in siteGeometries
//                                    select
//                                    new
//                                    {
//                                        type = "Feature",
//                                        id = siteG.Site.Id,
//                                        geometry =
//                                        new { type = "Polygon", coordinates = siteG.GetCoordinateArray },
//                                        properties =
//                                        new
//                                        {
//                                            siteName = siteG.Site.Name,
//                                            siteAreaDescription = "Shared_Municipality".Localize().ToString(),
//                                            siteAreaName = siteG.Site.GetAreaName(AreaDatasetId.MunicipalityIds),
//                                            projectAndExternalIdDescription =
//                                            "Shared_AssociatedProjects".Localize().ToString(),
//                                            projectAndExternalId = siteG.Site.GetProjectAndExternalId(),
//                                            coordSystemName = databaseCoordinateSystemName,
//                                            accuracyDescription = "Shared_Accuracy".Localize().ToString(),
//                                            accuracy = siteG.Site.Accuracy,
//                                            parentId = siteG.Site.Parent != null ? siteG.Site.Parent.Id : 0,
//                                            siteId = siteG.Site.Id,
//                                            siteType = siteG.SiteType >= SiteType.Private && siteG.SiteType <=
//                                                       SiteType.Parent
//                                                ? SiteType.Private
//                                                : siteG.SiteType,
//                                            dividedByYearUntil = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByYearUntil && dividByYear,
//                                            dividedByYearAfter = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByYearAfter && dividByYear,
//                                            dividedByYearNotRecoveredUntil = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByYearUntilNotRecovered && dividByYear,
//                                            dividedByYearNotRecoveredAfter = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByYearAfterNotRecovered && dividByYear,
//                                            dividedByValidReject = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByValidReject && dividByValid,
//                                            dividedByValidInProgress = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByValidInProgress && dividByValid,
//                                            dividedByValidApproved = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByValidApproved && dividByValid,
//                                            dividedByUnValidInterest = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByUnValidInterest && dividByValid,
//                                            dividedByUnValidWarning = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByUnValidWarning && dividByValid,
//                                            dividedByFloraGuardian = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByFloraGuardian && dividByFlora,
//                                            dividedByFloraGuardianOrdinary = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByFloraGuardianOrdinary && dividByFlora,
//                                            dividedByFloraGuardianNotRecovered = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByFloraGuardianNotRecovered && dividByFlora,
//                                            dividedByFloraGuardianOrdinaryNotRecovered = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByFloraGuardianOrdinaryNotRecovered && dividByFlora,
//                                            dividedByFloraGuardianCurrentYear = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByFloraGuardianCurrentYear && dividByFloraByYear,
//                                            dividedByFloraGuardian1To3YearsAgo = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByFloraGuardian1To3YearsAgo && dividByFloraByYear,
//                                            dividedByFloraGuardianOld = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByFloraGuardianOld && dividByFloraByYear,
//                                            dividedByFloraGuardianCurrentYearNotRecovered = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByFloraGuardianCurrentYearNotRecovered && dividByFloraByYear,
//                                            dividedByFloraGuardian1To3YearsAgoNotRecovered = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByFloraGuardian1To3YearsAgoNotRecovered && dividByFloraByYear,
//                                            dividedByFloraGuardianOldNotRecovered = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByFloraGuardianOldNotRecovered && dividByFloraByYear,
//                                            dividedByFloraGuardianArchivedSite = siteG.Site.GetProjectId() == WebConfig.FloraGuardianArchivedSitesProjectId,
//                                            dividedByProportionSize = (nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByProportionSize || nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType == SiteType.DividedByProportionSizeMigrate) && dividByProportion,
//                                            colorString = nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType >= SiteType.Child &&
//                                                          nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).SiteType <= SiteType.Parent ?
//                                                "#ffff00" : nonClusteredSites.First(s => s.SiteId == siteG.Site.Id).ColorCode,
//                                            siteCoordinateStringPresentation = SiteService.SiteHelper().GetSiteCoordinateStringPresentation(siteG.Site, CurrentUser, true,
//                                                AuthorizationService.IsUserAllowedToViewOriginalSite(siteG.Site)),
//                                            proportionSize = siteG.ProportionSize,
//                                            proportionRotation = siteG.ProportionRotation,
//                                            proportionHeight = siteG.ProportionHeight,
//                                            proportionColour = siteG.ProportionColour,
//                                            numberOfSightings = siteG.NumberOfSightings
//                                        }
//                                    };

//        var result =
//            new
//            {
//                points = new { type = "FeatureCollection", features = geoJsonSite },
//                polygons = new { type = "FeatureCollection", features = geoJsonSiteGeometries },
//                completeResult,
//                noSites
//            };

//        return SerializeJson(result);
//    }
//}


//public enum LocationTypeAp
//{
//    /// <summary>
//    /// At least one observation is present (obs.occurrence.isPositiveObservation==true).
//    /// </summary>
//    Present = 0,

//    /// <summary>
//    /// All observations are present (obs.occurrence.isPositiveObservation==true) 
//    /// and all are sensitive (obs.sensitive==true).
//    /// </summary>
//    PresentSensitive = 1,

//    /// <summary>
//    /// No observations are present (obs.occurrence.isPositiveObservation!=true) but at least one observation is not
//    /// rediscovered (obs.occurrence.isNotRediscoveredObservation==true).
//    /// </summary>
//    NotRecovered = 2,

//    /// <summary>
//    /// No observations are present (obs.occurrence.isPositiveObservation!=true) and no observation is not rediscovered
//    /// (obs.occurrence.isNotRediscoveredObservation!=true) but at least one observation is not present
//    /// (obs.occurrence.isNeverFoundObservation==true).
//    /// This is also used when cutoff year is used and all observations is not present (obs.occurrence.isPositiveObservation!=true).
//    /// </summary>
//    NotPresent = 3,

//    /// <summary>
//    /// When cutoff year is used and at least one observation is present (obs.occurrence.isPositiveObservation==true)
//    /// with end year (obs.event.endYear) equal to or larger than cutoff year.
//    /// </summary>
//    DividedByYearAfter = 4,

//    /// <summary>
//    /// When cutoff year and its not DividedByYearAfter, but there is at least one not rediscovered
//    /// (obs.occurrence.isNotRediscoveredObservation==true) with end year (obs.event.endYear) equal to or larger
//    /// than cutoff year.
//    /// </summary>
//    DividedByYearAfterNotRecovered = 5,

//    /// <summary>
//    /// When cutoff year is used and its not DividedByYearAfter or DividedByYearAfterNotRecovered, but there is at least
//    /// one present observation (obs.occurrence.isPositiveObservation==true) 
//    /// with end year (obs.event.endYear) smaller than cutoff year.
//    /// </summary>
//    DividedByYearUntil = 6,

//    /// <summary>
//    /// When cutoff year is used and its not DividedByYearAfter or DividedByYearAfterNotRecovered or DividedByYearUntil,
//    /// but there is at least one not rediscovered (obs.occurrence.isNotRediscoveredObservation==true) 
//    /// with end year (obs.event.endYear) smaller than cutoff year.
//    /// </summary>
//    DividedByYearUntilNotRecovered = 7,

//    /// <summary>
//    /// Multiple locations are clustered together because they are close to each other. 
//    /// </summary>
//    Cluster = 8,
//}

//using NetTopologySuite.Geometries;
//using NetTopologySuite.IO;
//using SOS.Lib.Enums;
//using SOS.Lib.Models.Verbatim.Artportalen;
//using System.ComponentModel.DataAnnotations;
//using System.Globalization;

//namespace SOS.Lib.Gis.Artportalen;

///// <summary>
///// A class to cluster sites when there are so many sites that it may take too much time to show all in the map.
///// It takes the sites and a number indicating the maximal number of resulting map elements (that is the sum of
///// clustered sites and nonclustered sites). 
///// The cluster algorithm first finds a cluster radius r to achive this. It then starts with the first site
///// clustering together with it all the sites within distance r of this site. If this results in a cluster of 
///// less than three sites, the site remains nonclustered unless it is clustered together with one of the remaining sites.
///// We then go to the next nonclustered site and repeat the process until all sites have either become
///// part of a cluster or remains nonclustered.
///// </summary>
//public class SiteClusterer
//{
//    private const int MinimalNumberOfSitesInOneCluster = 4;

//    private static int minLocationCountBeforeClustering;
//    private static int maxNumberOfSitesAfterClustering;
//    private static int maxNumberOfSitesToCluster;
//    private int idOfFirstSiteFailedToCluster = -1;

//    /// <summary>
//    /// Initializes a new instance of the <see cref="SiteClusterer"/> class. 
//    /// </summary>
//    public SiteClusterer()
//    {
//        if (minLocationCountBeforeClustering == 0 || maxNumberOfSitesAfterClustering == 0)
//        {
//            minLocationCountBeforeClustering = 100;
//            maxNumberOfSitesAfterClustering = 400;
//            maxNumberOfSitesToCluster = 10000;
//        }
//    }

//    /// <summary>
//    /// Initializes a new instance of the <see cref="SiteClusterer"/> class. 
//    /// Only meant to be used for testing varies values of MinLocationCountBeforeClustering and MaxNumberOfSitesAfterClustering
//    /// which are normally read from config-file.
//    /// </summary>
//    /// <param name="minLocationCountBeforeClustering">
//    /// The cluster threshold
//    /// </param>
//    /// <param name="maxNumberOfSitesAfterClustering">
//    /// the max number of sites after clustering
//    /// </param>
//    public SiteClusterer(int minLocationCountBeforeClustering, int maxNumberOfSitesAfterClustering)
//    {
//        SiteClusterer.minLocationCountBeforeClustering = minLocationCountBeforeClustering;
//        SiteClusterer.maxNumberOfSitesAfterClustering = maxNumberOfSitesAfterClustering;
//        maxNumberOfSitesToCluster = 10000;
//    }

//    /// <summary>
//    /// Cluster all sites such that there are all together at most MaxNumberOfSitesAfterClustering sites and clustered
//    /// sites after clustering.
//    /// If the number of sites is less than ClusterTreshold, don't cluster.
//    /// </summary>
//    /// <param name="sites">
//    /// The sites to cluster.
//    /// </param>
//    /// <param name="extent">
//    /// The map extent
//    /// </param>
//    /// <param name="zoomLevel">
//    /// The zoom level
//    /// </param>
//    /// <returns>
//    /// An object containing all clusters and all nonclustered sites
//    /// </returns>
//    public SiteClusterResult ClusterSites(List<SiteMapView> sites, MapExtent extent, int zoomLevel)
//    {
//        var sitesToCluster = new FilterSitesToCluster(maxNumberOfSitesToCluster).Filter(sites);
//        double mapDiagonal = ComputeMapDiagonal(extent, zoomLevel);
//        double sitesDiagonal = ComputeSitesDiagonal(sitesToCluster);
//        int clusterRadius = (int)(Math.Max(mapDiagonal, sitesDiagonal) / 100);
//        var result = new SiteClusterResult();
//        if (sitesToCluster.Count > minLocationCountBeforeClustering)
//        {
//            do
//            {
//                this.idOfFirstSiteFailedToCluster = -1;
//                result = new SiteClusterResult
//                {
//                    ClusteredSites = this.MakeClusters(sitesToCluster, clusterRadius)
//                };
//                clusterRadius = (int)(1.5 * clusterRadius);
//                result.NonClusteredSites = ComputeNonClusteredSites(result.ClusteredSites, sitesToCluster);
//            }
//            while (result.ClusteredSites.Count + result.NonClusteredSites.Count > maxNumberOfSitesAfterClustering);
//        }
//        else
//        {
//            result.ClusteredSites = new List<SiteCluster>();
//            result.NonClusteredSites = sitesToCluster.ToList();
//        }

//        return result;
//    }

//    private static List<SiteMapView> ComputeNonClusteredSites(IEnumerable<SiteCluster> clusteredSites, IEnumerable<SiteMapView> sites)
//    {
//        List<SiteMapView> nonClusteredSites = sites.ToList();

//        foreach (var clusteredSite in clusteredSites)
//        {
//            foreach (var site in clusteredSite.Sites)
//            {
//                nonClusteredSites.Remove(site);
//            }
//        }

//        return nonClusteredSites;
//    }

//    private static double ComputeMapDiagonal(MapExtent extent, int zoomLevel)
//    {
//        double realDiagonal = Math.Sqrt(Math.Pow(extent.MaxX - extent.MinX, 2) + Math.Pow(extent.MaxY - extent.MinY, 2));
//        double adjustedDiagonal = realDiagonal;
//        if (zoomLevel < 5)
//        {
//            adjustedDiagonal = realDiagonal / Math.Pow(2, 5 - zoomLevel);
//        }

//        return adjustedDiagonal;
//    }

//    /// <summary>
//    /// Computes the cluster radius 
//    /// </summary>
//    /// <param name="sites">
//    /// The sites to be clustered.
//    /// </param>
//    /// <returns>
//    /// The radius to use for clustering.
//    /// </returns>
//    private static double ComputeSitesDiagonal(List<SiteMapView> sites)
//    {
//        if (sites == null || sites.Count == 0)
//        {
//            return 0.0;
//        }

//        int minXForAllSites = sites.Min(site => site.XCoord);
//        int maxXForAllSites = sites.Max(site => site.XCoord);
//        int minYForAllSites = sites.Min(site => site.YCoord);
//        int maxYForAllSites = sites.Max(site => site.YCoord);

//        return Math.Sqrt(Math.Pow(maxXForAllSites - minXForAllSites, 2) + Math.Pow(maxYForAllSites - minYForAllSites, 2));
//    }

//    private static void RemoveClusteredSitesFromSitesToBeClustered(SiteCluster cluster, List<SiteMapView> remainingSitesToBeClustered)
//    {
//        foreach (var clusteredSite in cluster.Sites)
//        {
//            remainingSitesToBeClustered.Remove(clusteredSite);
//        }
//    }

//    private List<SiteMapView> LimitNumberOfSitesToCluster(List<SiteMapView> sites, MapExtent extent)
//    {
//        const int MaxNumberOfSitesToCluster = 10000;
//        if (sites.Count < MaxNumberOfSitesToCluster)
//        {
//            return sites;
//        }

//        var proportionToGet = (double)MaxNumberOfSitesToCluster / sites.Count;
//        var random = new Random();

//        return sites.Where(site => random.NextDouble() < proportionToGet).ToList();
//    }

//    private bool NoMoreClustersToBeMade(List<SiteMapView> remainingSitesToCluster)
//    {
//        return this.NotEnoughRemainingSitesToMakeCluster(remainingSitesToCluster)
//               || this.HaveTriedToClusterNextSiteBefore(remainingSitesToCluster);
//    }

//    private bool NotEnoughRemainingSitesToMakeCluster(List<SiteMapView> remainingSitesToCluster)
//    {
//        return remainingSitesToCluster.Count < MinimalNumberOfSitesInOneCluster;
//    }

//    private bool HaveTriedToClusterNextSiteBefore(List<SiteMapView> remainingSitesToCluster)
//    {
//        var nextSiteToCluster = remainingSitesToCluster[0];
//        return nextSiteToCluster.SiteId == this.idOfFirstSiteFailedToCluster;
//    }

//    private List<SiteCluster> MakeClusters(List<SiteMapView> sites, int clusterRadius)
//    {
//        var clusteredSites = new List<SiteCluster>();
//        if (sites.Count == 0)
//        {
//            return clusteredSites;
//        }

//        List<SiteMapView> remainingSitesToBeClustered = sites.ToList();

//        bool clusteringFinished = false;
//        int maxIterations = sites.Count;

//        while (!clusteringFinished)
//        {
//            if (this.NoMoreClustersToBeMade(remainingSitesToBeClustered))
//            {
//                clusteringFinished = true;
//            }
//            else
//            {
//                var cluster = SiteCluster.MakeCluster(remainingSitesToBeClustered[0], remainingSitesToBeClustered, clusterRadius);
//                if (cluster.Sites.Count >= MinimalNumberOfSitesInOneCluster)
//                {
//                    if (cluster.Sites.SingleOrDefault(s => s.SiteId == this.idOfFirstSiteFailedToCluster) != null)
//                    {
//                        this.idOfFirstSiteFailedToCluster = -1;
//                    }

//                    RemoveClusteredSitesFromSitesToBeClustered(cluster, remainingSitesToBeClustered);
//                    clusteredSites.Add(cluster);
//                }
//                else
//                {
//                    this.PutFailedClusterSiteAtTheEndOfSitesToCluster(remainingSitesToBeClustered);
//                }
//            }

//            maxIterations--;

//            if (maxIterations == 0)
//            {
//                clusteringFinished = true;
//            }
//        }

//        return clusteredSites;
//    }

//    private void PutFailedClusterSiteAtTheEndOfSitesToCluster(List<SiteMapView> remainingSitesToBeClustered)
//    {
//        if (this.idOfFirstSiteFailedToCluster == -1)
//        {
//            this.idOfFirstSiteFailedToCluster = remainingSitesToBeClustered[0].SiteId;
//        }

//        remainingSitesToBeClustered.Remove(remainingSitesToBeClustered[0]);
//        remainingSitesToBeClustered.Add(remainingSitesToBeClustered[0]);
//    }
//}

///// <summary>
///// Class to filter out the sites to cluster in case of too many sites for effective clustering.
///// </summary>
//public class FilterSitesToCluster
//{
//    private readonly int maxNumberOfSitesForClustering;

//    /// <summary>
//    /// Initializes a new instance of the <see cref="FilterSitesToCluster"/> class. 
//    /// </summary>
//    /// <param name="maxNumberOfSitesForClustering">
//    /// The max number of sites for clustering.
//    /// </param>
//    public FilterSitesToCluster(int maxNumberOfSitesForClustering)
//    {
//        this.maxNumberOfSitesForClustering = maxNumberOfSitesForClustering;
//    }

//    /// <summary>
//    /// Filters out the sites to cluster
//    /// </summary>
//    /// <param name="sites">
//    /// All the sites 
//    /// </param>
//    /// <returns>
//    /// The sites to cluster
//    /// </returns>
//    public List<SiteMapView> Filter(List<SiteMapView> sites)
//    {
//        if (sites.Count < this.maxNumberOfSitesForClustering)
//        {
//            return sites;
//        }

//        var proportionToGet = (double)this.maxNumberOfSitesForClustering / sites.Count;
//        var random = new Random();

//        return sites.Where(site => random.NextDouble() < proportionToGet).ToList();
//    }
//}

///// <summary>
///// Class for containing result of clustering sites
///// </summary>
//public class SiteClusterResult
//{
//    /// <summary>
//    /// Gets or sets ClusteredSites.
//    /// </summary>
//    public List<SiteCluster> ClusteredSites { get; set; }

//    /// <summary>
//    /// Gets or sets NonClusteredSites.
//    /// </summary>
//    public List<SiteMapView> NonClusteredSites { get; set; }

//    /// <summary>
//    /// Remove all siteGeometries associated to sites that are clustered
//    /// </summary>
//    /// <param name="siteGeometries">
//    /// The site geometries.
//    /// </param>
//    public void RemoveSiteGeometriesAssociatedToClusteredSites(IList<SiteGeometry> siteGeometries)
//    {
//        // TODO: test denne
//        this.ClusteredSites
//            .ForEach(cluster =>
//                     cluster.Sites
//                         .ForEach(
//                             site =>
//                             siteGeometries.Remove(
//                                 siteGeometries.Where(sg => sg.Site.Id == site.SiteId).FirstOrDefault())));
//    }
//}

///// <summary>
///// Representing a cluster of sites.
///// </summary>
//public class SiteCluster
//{
//    private readonly List<SiteMapView> sites = new List<SiteMapView>();
//    private readonly SiteMapView clusterSite;
//    private readonly int clusterRadius;

//    /// <summary>
//    /// Initializes a new instance of the <see cref="SiteCluster"/> class.
//    /// </summary>
//    /// <param name="clusterSite">
//    /// The site to cluster about
//    /// </param>
//    /// <param name="clusterRadius">
//    /// The cluster radius
//    /// </param>
//    public SiteCluster(SiteMapView clusterSite, int clusterRadius)
//    {
//        this.clusterSite = clusterSite;
//        this.clusterRadius = clusterRadius;
//    }

//    /// <summary>
//    /// Gets the sites in the cluster
//    /// </summary>
//    public List<SiteMapView> Sites
//    {
//        get { return this.sites; }
//    }

//    /// <summary>
//    /// Gets the resulting XCoord of all clustered sites.
//    /// </summary>
//    public int XCoord
//    {
//        get
//        {
//            return this.clusterSite.XCoord;
//        }
//    }

//    /// <summary>
//    /// Gets the resulting YCoord of all clustered sites.
//    /// </summary>
//    public int YCoord
//    {
//        get
//        {
//            return this.clusterSite.YCoord;
//        }
//    }

//    /// <summary>
//    /// Gets the resulting accuracy for the cluster
//    /// </summary>
//    public int Accuracy
//    {
//        get
//        {
//            return (int)(5 * this.clusterRadius / 12);
//        }
//    }

//    /// <summary>
//    /// Make cluster of all sites within clusterRadius of clusterSite
//    /// </summary>
//    /// <param name="clusterSite">
//    /// The cluster Site.
//    /// </param>
//    /// <param name="sitesToBeClustered">
//    /// The sites to be clustered
//    /// </param>
//    /// <param name="clusterRadius">
//    /// The cluster Radius.
//    /// </param>
//    /// <returns>
//    /// A cluster containing all sites within distance clusterRadius of clusterSite
//    /// </returns>
//    public static SiteCluster MakeCluster(SiteMapView clusterSite, List<SiteMapView> sitesToBeClustered, int clusterRadius)
//    {
//        var result = new SiteCluster(clusterSite, clusterRadius);
//        result.ClusterSitesWithinClusterRadiusOfClusterSite(sitesToBeClustered);
//        return result;
//    }

//    /// <summary>
//    /// Checks whether a site is within clusterradius
//    /// </summary>
//    /// <param name="site">
//    /// The site to check
//    /// </param>
//    /// <returns>
//    /// true or false
//    /// </returns>
//    public bool IsSiteWithinClusterRadius(SiteMapView site)
//    {
//        return
//            Math.Sqrt(Math.Pow(site.XCoord - this.clusterSite.XCoord, 2)
//                      + Math.Pow(site.YCoord - this.clusterSite.YCoord, 2)) < this.clusterRadius;
//    }

//    private void ClusterSitesWithinClusterRadiusOfClusterSite(IEnumerable<SiteMapView> remainingSitesToBeClustered)
//    {
//        foreach (var siteToBeClustered in remainingSitesToBeClustered)
//        {
//            if (this.IsSiteWithinClusterRadius(siteToBeClustered))
//            {
//                this.Add(siteToBeClustered);
//            }
//        }
//    }

//    private void Add(SiteMapView site)
//    {
//        this.sites.Add(site);
//    }
//}

///// <summary>
///// This view represents data needed to display sites on a map.
///// </summary>
//public class SiteMapView
//{
//    private int siteType;

//    /// <summary>
//    /// Gets or sets SiteId.
//    /// </summary>
//    public int SiteId { get; set; }

//    /// <summary>
//    /// Gets or sets the parent site id.
//    /// </summary>
//    public int? ParentSiteId { get; set; }

//    /// <summary>
//    /// Gets or sets XCoord.
//    /// </summary>
//    public int XCoord { get; set; }

//    /// <summary>
//    /// Gets or sets YCoord.
//    /// </summary>
//    public int YCoord { get; set; }

//    /// <summary>
//    /// Gets or sets true XCoord.
//    /// </summary>
//    public int TrueXCoord { get; set; }

//    /// <summary>
//    /// Gets or sets true YCoord.
//    /// </summary>
//    public int TrueYCoord { get; set; }

//    /// <summary>
//    /// Gets or sets Accuracy.
//    /// </summary>
//    public int Accuracy { get; set; }

//    /// <summary>
//    /// Gets or sets Diffusion.
//    /// </summary>
//    public int Diffusion { get; set; }

//    /// <summary>
//    /// Gets or sets a value indicating whether is private.
//    /// </summary>
//    public bool IsPrivate { get; set; }

//    /// <summary>
//    /// Gets or sets Area id - for assessment sites
//    /// </summary>
//    public int? AreaId { get; set; }

//    /// <summary>
//    /// Gets or sets ValidationStatus id - for assessment sites
//    /// </summary>
//    public int? ValidationStatusId { get; set; }

//    /// <summary>
//    /// ProjectId connected to Site
//    /// </summary>        
//    public int? ProjectId { get; set; }

//    /// <summary>
//    /// Gets or sets a value indicating whether ProtectedBySystem. Least protected sighting sets the value.
//    /// </summary>
//    public int LeastProtected { get; set; }

//    /// <summary>
//    /// Gets or sets a value indicating whether to ignore protected sites in get SiteType.
//    /// </summary>
//    public bool IgnoreProtecedSites { get; set; }

//    /// <summary>
//    /// Gets or sets HiddenByProvider. Least protected sighting sets the value.
//    /// </summary>
//    public int LeastHiddenByProvider { get; set; }

//    /// <summary>
//    /// Gets or sets a value indicating whether NotRecovered. Least not recovered sets the value.
//    /// </summary>
//    public bool LeastNotRecovered { get; set; }

//    /// <summary>
//    /// Gets or sets a value indicating whether NotPresent. Least not present sets the value.
//    /// </summary>
//    public bool LeastNotPresent { get; set; }

//    /// <summary>
//    /// Gets or sets the proportional size of the point.
//    /// </summary>
//    public int SizeOfPoint { get; set; }

//    /// <summary>
//    /// Gets or sets NoSightings. The number of sightings at the site.
//    /// </summary>
//    public int NoSightings { get; set; }

//    /// <summary>
//    /// Gets or sets Date. The date of the chosen sighting at the site.
//    /// </summary>
//    public string Date { get; set; }

//    /// <summary>
//    /// Gets or sets MigrationRotation. The rotation of migration icon.
//    /// </summary>
//    public string MigrationRotation { get; set; }

//    /// <summary>
//    /// Gets or sets MigrationHeight. The height of migration icon.
//    /// </summary>
//    public string MigrationHeight { get; set; }

//    /// <summary>
//    /// Gets or sets Colour. The colour of the site on map.
//    /// </summary>
//    public string Colour { get; set; }

//    /// <summary>
//    /// Gets or sets sitetype
//    /// </summary>
//    /// <returns>
//    /// returns a number like 0, 1, 2
//    /// </returns>
//    public int SiteType
//    {
//        get
//        {
//            if (!this.IgnoreProtecedSites && (this.LeastProtected > 0 || this.LeastHiddenByProvider > 0))
//            {
//                return ApSiteType.Protected;
//            }

//            if (this.LeastNotRecovered)
//            {
//                return ApSiteType.NotRecovered;
//            }

//            if (this.LeastNotPresent || this.siteType == ApSiteType.NotPresent)
//            {
//                return ApSiteType.NotPresent;
//            }

//            if (this.siteType == ApSiteType.DividedByYearUntil ||
//                this.siteType == ApSiteType.DividedByYearAfter ||
//                this.siteType == ApSiteType.DividedByValidReject ||
//                this.siteType == ApSiteType.DividedByValidInProgress ||
//                this.siteType == ApSiteType.DividedByValidApproved ||
//                this.siteType == ApSiteType.DividedByUnValidInterest ||
//                this.siteType == ApSiteType.DividedByUnValidWarning ||
//                this.siteType == ApSiteType.DividedByUnValidNotRecovered ||
//                this.siteType == ApSiteType.DividedByFloraGuardian ||
//                this.siteType == ApSiteType.DividedByFloraGuardianNotRecovered ||
//                this.siteType == ApSiteType.DividedByFloraGuardianOrdinary ||
//                this.siteType == ApSiteType.DividedByFloraGuardianOrdinaryNotRecovered ||
//                this.siteType == ApSiteType.DividedByFloraGuardianCurrentYear ||
//                this.siteType == ApSiteType.DividedByFloraGuardian1To3YearsAgo ||
//                this.siteType == ApSiteType.DividedByFloraGuardianOld ||
//                this.siteType == ApSiteType.DividedByFloraGuardianCurrentYearNotRecovered ||
//                this.siteType == ApSiteType.DividedByFloraGuardian1To3YearsAgoNotRecovered ||
//                this.siteType == ApSiteType.DividedByFloraGuardianOldNotRecovered ||
//                this.siteType == ApSiteType.DividedByProportionSize ||
//                this.siteType == ApSiteType.DividedByProportionSizeMigrate ||
//                this.siteType == ApSiteType.DividedByProportionRotationMigrate ||
//                this.siteType == ApSiteType.DividedByProportionHeightMigrate ||
//                this.siteType == ApSiteType.DividedByProportionXOffsetMigrate ||
//                this.siteType == ApSiteType.DividedByProportionYOffsetMigrate ||
//                this.siteType == ApSiteType.DividedByYearUntilNotRecovered ||
//                this.siteType == ApSiteType.DividedByYearAfterNotRecovered ||
//                this.siteType <= ApSiteType.DividedByYearAfterNotRecovered)
//            {
//                return this.siteType;
//            }

//            if (this.AreaId.HasValue)
//            {
//                return ApSiteType.AssessmentSite;
//            }

//            if (this.Diffusion != 0)
//            {
//                return this.siteType;
//            }

//            if (this.IsPrivate)
//            {
//                return ApSiteType.Private;
//            }

//            return this.ParentSiteId == null ? ApSiteType.Parent : ApSiteType.Child;
//        }

//        set
//        {
//            this.siteType = value;
//        }
//    }

//    /// <summary>
//    /// Gets site color for gui presentation
//    /// </summary>
//    /// <returns>
//    /// color code
//    /// </returns>
//    public virtual string ColorCode
//    {
//        get
//        {
//            if (this.LeastProtected > 0 || this.LeastHiddenByProvider > 0)
//            {
//                return SiteColorString.Protected;
//            }

//            if (this.IsPrivate && this.ParentSiteId != null)
//            {
//                return SiteColorString.PrivateWithParent;
//            }

//            if (this.IsPrivate)
//            {
//                return SiteColorString.Private;
//            }

//            return this.ParentSiteId != null ? SiteColorString.Child : SiteColorString.Parent;
//        }
//    }
//}

///// <summary>
///// Color of sites when shown in map
///// </summary>
//public static class SiteColorString
//{
//    /// <summary>
//    /// Gets Private site color string
//    /// </summary>
//    public static string Private
//    {
//        get
//        {
//            return "#ffff00";
//        }
//    }

//    /// <summary>
//    /// Gets the color representing a private site that's connected to a public site
//    /// </summary>
//    public static string PrivateWithParent
//    {
//        get { return "#ffa500"; }
//    }

//    /// <summary>
//    /// Gets Parent site color string
//    /// </summary>
//    public static string Parent
//    {
//        get
//        {
//            return "#006600";
//        }
//    }

//    /// <summary>
//    /// Gets Child site color string
//    /// </summary>
//    public static string Child
//    {
//        get
//        {
//            return "#00ff00";
//        }
//    }

//    /// <summary>
//    /// Gets Protected site color string
//    /// </summary>
//    public static string Protected
//    {
//        get
//        {
//            return "#ff0000";
//        }
//    }

//    /// <summary>
//    /// Gets the color representing a main site for positions (Ortnamn)
//    /// </summary>
//    public static string MainSiteForPositions
//    {
//        get { return "#ffffff"; }
//    }
//}

///// <summary>
///// All possible values in table SightingType
///// </summary>
//public static class ApSiteType
//{
//    /// <summary>
//    /// Gets sighting type id for a private site.
//    /// </summary>
//    public static int Private
//    {
//        get
//        {
//            return 0;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for an child site.
//    /// </summary>
//    public static int Child
//    {
//        get
//        {
//            return 1;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for an parent site.
//    /// </summary>
//    public static int Parent
//    {
//        get
//        {
//            return 2;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a childless site.
//    /// </summary>
//    public static int Childless
//    {
//        get
//        {
//            return 3;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a cluster site.
//    /// </summary>
//    public static int Cluster
//    {
//        get
//        {
//            return 4;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a not recovered site.
//    /// </summary>
//    public static int NotRecovered
//    {
//        get
//        {
//            return 5;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a not recovered site.
//    /// </summary>
//    public static int AssessmentSite
//    {
//        get
//        {
//            return 6;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a protected site.
//    /// </summary>
//    public static int Protected
//    {
//        get
//        {
//            return 7;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a site not present.
//    /// </summary>
//    public static int NotPresent
//    {
//        get
//        {
//            return 8;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a divided by year site until break year.
//    /// </summary>
//    public static int DividedByYearUntil
//    {
//        get
//        {
//            return 10;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a divided by year site after break year.
//    /// </summary>
//    public static int DividedByYearAfter
//    {
//        get
//        {
//            return 11;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a rejected site.
//    /// </summary>
//    public static int DividedByValidReject
//    {
//        get
//        {
//            return 12;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for an inprogress validating site.
//    /// </summary>
//    public static int DividedByValidInProgress
//    {
//        get
//        {
//            return 13;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for an approved validated site.
//    /// </summary>
//    public static int DividedByValidApproved
//    {
//        get
//        {
//            return 14;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for an intresting unvalidated site.
//    /// </summary>
//    public static int DividedByUnValidInterest
//    {
//        get
//        {
//            return 15;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a warned unvalidated site.
//    /// </summary>
//    public static int DividedByUnValidWarning
//    {
//        get
//        {
//            return 16;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a unvalidated not recovered site.
//    /// </summary>
//    public static int DividedByUnValidNotRecovered
//    {
//        get
//        {
//            return 17;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a flora guardian site.
//    /// </summary>
//    public static int DividedByFloraGuardian
//    {
//        get
//        {
//            return 18;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a flora guardian site where sighiting is not recovered.
//    /// </summary>
//    public static int DividedByFloraGuardianNotRecovered
//    {
//        get
//        {
//            return 19;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a flora guardian site which are ordinary sites.
//    /// </summary>
//    public static int DividedByFloraGuardianOrdinary
//    {
//        get
//        {
//            return 20;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a flora guardian site which are ordinary sites where sighiting is not recovered.
//    /// </summary>
//    public static int DividedByFloraGuardianOrdinaryNotRecovered
//    {
//        get
//        {
//            return 21;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a ssite for flora guardian, current year.
//    /// </summary>
//    public static int DividedByFloraGuardianCurrentYear
//    {
//        get
//        {
//            return 22;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a site for flora guardian, the two years before last year.
//    /// </summary>
//    public static int DividedByFloraGuardian1To3YearsAgo
//    {
//        get
//        {
//            return 24;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a site for flora guardian, the years before 2To3 years (see above).
//    /// </summary>
//    public static int DividedByFloraGuardianOld
//    {
//        get
//        {
//            return 25;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a ssite for flora guardian, current year where sighiting is not recovered.
//    /// </summary>
//    public static int DividedByFloraGuardianCurrentYearNotRecovered
//    {
//        get
//        {
//            return 26;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a site for flora guardian, the two years before last year where sighiting is not recovered.
//    /// </summary>
//    public static int DividedByFloraGuardian1To3YearsAgoNotRecovered
//    {
//        get
//        {
//            return 28;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a site for flora guardian, the years before 2To3 years (see above) where sighiting is not recovered.
//    /// </summary>
//    public static int DividedByFloraGuardianOldNotRecovered
//    {
//        get
//        {
//            return 29;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a site with proportional size.
//    /// </summary>
//    public static int DividedByProportionSize
//    {
//        get
//        {
//            return 30;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a migrated site with proportional size.
//    /// </summary>
//    public static int DividedByProportionSizeMigrate
//    {
//        get
//        {
//            return 31;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a migrated site with rotation.
//    /// </summary>
//    public static int DividedByProportionRotationMigrate
//    {
//        get
//        {
//            return 32;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a migrated site with height.
//    /// </summary>
//    public static int DividedByProportionHeightMigrate
//    {
//        get
//        {
//            return 33;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a migarated site with xoffset.
//    /// </summary>
//    public static int DividedByProportionXOffsetMigrate
//    {
//        get
//        {
//            return 34;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a migarated site with yoffset.
//    /// </summary>
//    public static int DividedByProportionYOffsetMigrate
//    {
//        get
//        {
//            return 35;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a divided by year site until break year where sighting is not recovered.
//    /// </summary>
//    public static int DividedByYearUntilNotRecovered
//    {
//        get
//        {
//            return 36;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a divided by year site after break year where sighting is not recovered.
//    /// </summary>
//    public static int DividedByYearAfterNotRecovered
//    {
//        get
//        {
//            return 37;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a divided by year site after break year where sighting is not recovered.
//    /// </summary>
//    public static int SearchPolygon
//    {
//        get
//        {
//            return 38;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a diffused site.
//    /// </summary>
//    public static int Diffused
//    {
//        get
//        {
//            return 39;
//        }
//    }

//    /// <summary>
//    /// Gets sighting type id for a undiffused site.
//    /// </summary>
//    public static int UnDiffused
//    {
//        get
//        {
//            return 40;
//        }
//    }
//}

///// <summary>
///// A geographic boundary
///// </summary>
//[Serializable]
//public class MapExtent
//{
//    #region Properties

//    /// <summary>
//    /// Gets or sets right X value
//    /// </summary>
//    public int MaxX { get; set; }

//    /// <summary>
//    /// Gets or sets top Y value
//    /// </summary>
//    public int MaxY { get; set; }

//    /// <summary>
//    /// Gets or sets left X value
//    /// </summary>
//    public int MinX { get; set; }

//    /// <summary>
//    /// Gets or sets bottom Y value
//    /// </summary>
//    public int MinY { get; set; }

//    #endregion

//    #region Public Methods

//    /// <summary>
//    /// generates a MapExtent from a WMS BBOX string
//    /// </summary>
//    /// <param name="bboxValue">
//    /// String representation of boundingbox
//    /// </param>
//    /// <returns>
//    /// An extent generated from 
//    /// </returns>
//    /// <exception cref="FormatException">
//    /// Error in input value
//    /// </exception>
//    public static MapExtent FromBboxString(string bboxValue)
//    {
//        var theExtent = new MapExtent();
//        if (string.IsNullOrEmpty(bboxValue))
//        {
//            throw new FormatException("Nothing in input value");
//        }

//        var values = bboxValue.Split(',');
//        if (values.Length != 4)
//        {
//            throw new FormatException("Wrong number of numbers in input value");
//        }

//        try
//        {
//            theExtent.MinX =
//                Convert.ToInt32(double.Parse(CorrectDecimalSeparator(values[0]), CultureInfo.CurrentCulture));
//            theExtent.MinY =
//                Convert.ToInt32(double.Parse(CorrectDecimalSeparator(values[1]), CultureInfo.CurrentCulture));
//            theExtent.MaxX =
//                Convert.ToInt32(double.Parse(CorrectDecimalSeparator(values[2]), CultureInfo.CurrentCulture));
//            theExtent.MaxY =
//                Convert.ToInt32(double.Parse(CorrectDecimalSeparator(values[3]), CultureInfo.CurrentCulture));
//        }
//        catch (Exception ex)
//        {
//            throw new FormatException("Invalid numbers in input value", ex);
//        }

//        theExtent.EnsureCorrectCoordinateOrder();
//        return theExtent;
//    }

//    /// <summary>
//    /// Generates a matching extent for input parameters
//    /// </summary>
//    /// <param name="x">
//    /// The x coordinate
//    /// </param>
//    /// <param name="y">
//    /// The y coordinate
//    /// </param>
//    /// <param name="width">
//    /// The width.
//    /// </param>
//    /// <param name="height">
//    /// The height.
//    /// </param>
//    /// <param name="precision">
//    /// The precision.
//    /// </param>
//    /// <returns>
//    /// A mapExtent
//    /// </returns>
//    public static MapExtent FromCoordinateAndPrecision(int x, int y, int width, int height, int precision)
//    {
//        var theExtent = new MapExtent { MaxX = x + width, MinX = x - width, MaxY = y + height, MinY = y - height };
//        theExtent.SetScale(50000, width, height);
//        return theExtent;
//    }

//    /// <summary>
//    /// Create a map extent from a list of sites.
//    /// </summary>
//    /// <param name="sites">
//    /// The sites.
//    /// </param>
//    /// <param name="height">
//    /// The height.
//    /// </param>
//    /// <param name="width">
//    /// The width.
//    /// </param>
//    /// <returns>
//    /// The map exent created.
//    /// </returns>
//    public static MapExtent Create(IList<Site> sites, int height, int width)
//    {
//        // TODO: Refactor all usages of this method to get the correct model.
//        var mapViews =
//            sites.Select(
//                site =>
//                new SiteMapExtentView
//                {
//                    Accuracy = site.Accuracy,
//                    SiteId = site.Id,
//                    XCoord = site.XCoord,
//                    YCoord = site.YCoord
//                });

//        return Create(mapViews.ToList(), height, width);
//    }

//    /// <summary>
//    /// Create a map extent from a list of sites.
//    /// </summary>
//    /// <param name="sites">
//    /// The sites.
//    /// </param>
//    /// <returns>
//    /// The map exent created.
//    /// </returns>
//    public static MapExtent Create(IList<Site> sites)
//    {
//        // TODO: Refactor all usages of this method to get the correct model.
//        var mapViews = from Site site in sites select new SiteMapExtentView { Accuracy = site.Accuracy, SiteId = site.Id, XCoord = site.XCoord, YCoord = site.YCoord };

//        return Create(mapViews.ToList());
//    }

//    /// <summary>
//    /// Create a map extent from a list of Site map view data.
//    /// </summary>
//    /// <param name="siteMapViews">
//    /// The site map views.
//    /// </param>
//    /// <returns>
//    /// The map exent created.
//    /// </returns>
//    public static MapExtent Create(IList<SiteMapExtentView> siteMapViews)
//    {
//        var extent = new MapExtent();
//        if (siteMapViews.Count <= 0)
//        {
//            return extent;
//        }

//        var mapExtentView = siteMapViews[0];
//        if (mapExtentView == null)
//        {
//            return extent;
//        }

//        extent.MinX = mapExtentView.XCoord - mapExtentView.Accuracy;
//        extent.MaxX = mapExtentView.XCoord + mapExtentView.Accuracy;
//        extent.MinY = mapExtentView.YCoord - mapExtentView.Accuracy;
//        extent.MaxY = mapExtentView.YCoord + mapExtentView.Accuracy;
//        for (var i = 1; i < siteMapViews.Count; i++)
//        {
//            mapExtentView = siteMapViews[i];
//            if (mapExtentView == null)
//            {
//                continue;
//            }

//            if (extent.MinX > (mapExtentView.XCoord - mapExtentView.Accuracy))
//            {
//                extent.MinX = mapExtentView.XCoord - mapExtentView.Accuracy;
//            }

//            if (extent.MinY > (mapExtentView.YCoord - mapExtentView.Accuracy))
//            {
//                extent.MinY = mapExtentView.YCoord - mapExtentView.Accuracy;
//            }

//            if (extent.MaxX < (mapExtentView.XCoord + mapExtentView.Accuracy))
//            {
//                extent.MaxX = mapExtentView.XCoord + mapExtentView.Accuracy;
//            }

//            if (extent.MaxY < (mapExtentView.YCoord + mapExtentView.Accuracy))
//            {
//                extent.MaxY = mapExtentView.YCoord + mapExtentView.Accuracy;
//            }
//        }

//        return extent;
//    }

//    /// <summary>
//    /// Create a map extent from a list of Site map view data.
//    /// </summary>
//    /// <param name="siteMapViews">
//    /// The site map views.
//    /// </param>
//    /// <param name="height">
//    /// The height.
//    /// </param>
//    /// <param name="width">
//    /// The width.
//    /// </param>
//    /// <returns>
//    /// The map exent created.
//    /// </returns>
//    public static MapExtent Create(IList<SiteMapExtentView> siteMapViews, int height, int width)
//    {
//        var extent = new MapExtent();
//        if (siteMapViews.Count <= 0)
//        {
//            return extent;
//        }

//        var mapExtentView = siteMapViews[0];
//        if (mapExtentView == null)
//        {
//            return extent;
//        }

//        extent.MinX = mapExtentView.XCoord - mapExtentView.Accuracy;
//        extent.MaxX = mapExtentView.XCoord + mapExtentView.Accuracy;
//        extent.MinY = mapExtentView.YCoord - mapExtentView.Accuracy;
//        extent.MaxY = mapExtentView.YCoord + mapExtentView.Accuracy;
//        for (var i = 1; i < siteMapViews.Count; i++)
//        {
//            mapExtentView = siteMapViews[i];
//            if (mapExtentView == null)
//            {
//                continue;
//            }

//            if (extent.MinX > (mapExtentView.XCoord - mapExtentView.Accuracy))
//            {
//                extent.MinX = mapExtentView.XCoord - mapExtentView.Accuracy;
//            }

//            if (extent.MinY > (mapExtentView.YCoord - mapExtentView.Accuracy))
//            {
//                extent.MinY = mapExtentView.YCoord - mapExtentView.Accuracy;
//            }

//            if (extent.MaxX < (mapExtentView.XCoord + mapExtentView.Accuracy))
//            {
//                extent.MaxX = mapExtentView.XCoord + mapExtentView.Accuracy;
//            }

//            if (extent.MaxY < (mapExtentView.YCoord + mapExtentView.Accuracy))
//            {
//                extent.MaxY = mapExtentView.YCoord + mapExtentView.Accuracy;
//            }
//        }

//        // handle one site with no accurancy bug
//        if (extent.MinX == extent.MaxX)
//        {
//            extent.MinX -= 200;
//            extent.MaxX += 200;
//        }

//        if (extent.MinY == extent.MaxY)
//        {
//            extent.MinY -= 200;
//            extent.MaxY += 200;
//        }

//        extent.CorrectProportionsOfExtentForSpecificHeightAndWidth(height, width, 1.2);

//        return extent;
//    }

//    /// <summary>
//    /// Generates MapExtent from envelope
//    /// </summary>
//    /// <param name="envelope">
//    /// </param>
//    /// <returns>
//    /// a mapextent thingy
//    /// </returns>
//    public static MapExtent FromEnvelope(Geometry envelope)
//    {
//        var extent = new MapExtent { MinX = (int)envelope.Coordinates[0].X, MinY = (int)envelope.Coordinates[0].Y, MaxX = (int)envelope.Coordinates[2].X, MaxY = (int)envelope.Coordinates[2].Y };
//        return extent;
//    }

//    /// <summary>
//    /// The from wkt geometry.
//    /// </summary>
//    /// <param name="wktGeometry">
//    /// The wkt geometry.
//    /// </param>
//    /// <returns>
//    /// The <see cref="MapExtent"/>.
//    /// </returns>
//    public static MapExtent FromWktGeometry(string wktGeometry)
//    {
//        var geometry = new WKTReader().Read(wktGeometry).Envelope;
//        return FromEnvelope(geometry);
//    }

//    /// <summary>
//    /// The from envelope.
//    /// </summary>
//    /// <param name="envelope">
//    /// The envelope.
//    /// </param>
//    /// <returns>
//    /// The <see cref="MapExtent"/>.
//    /// </returns>
//    public static MapExtent FromEnvelope(Envelope envelope)
//    {
//        var extent = new MapExtent { MinX = (int)envelope.MinX, MinY = (int)envelope.MinY, MaxX = (int)envelope.MaxX, MaxY = (int)envelope.MaxY };
//        return extent;
//    }

//    /// <summary>
//    /// Corrects and calculates a real extent to match a height and a width
//    /// </summary>
//    /// <param name="imageHeight">
//    /// The imageHeight.
//    /// </param>
//    /// <param name="imageWidth">
//    /// The imageWidth.
//    /// </param>
//    /// <param name="scaleFactor">
//    /// The scale factor.
//    /// </param>
//    /// <exception cref="ArgumentException">One of the parameters was 0</exception>
//    public void CorrectProportionsOfExtentForSpecificHeightAndWidth(int imageHeight, int imageWidth, double scaleFactor)
//    {
//        // <pex>
//        if (imageHeight == 0 || imageWidth == 0 || Math.Abs(scaleFactor) < 0.00001)
//        {
//            throw new ArgumentException("One of the parameters was 0");
//        }

//        this.CheckValidExtent();

//        // </pex>
//        // sorter koordinat for å unngå eventuelle feil
//        this.EnsureCorrectCoordinateOrder();

//        var bbox = this.ToBboxString();
//        try
//        {
//            var iaspect = Convert.ToDouble(imageHeight) / Convert.ToDouble(imageWidth);

//            double width = this.MaxX - this.MinX;
//            double height = this.MaxY - this.MinY;
//            var easpect = height / width;

//            var midX = this.MinX + (width / 2);
//            var midY = this.MinY + (height / 2);

//            if (easpect < iaspect)
//            {
//                // Increase height so that height / width = iaspect
//                height = width * iaspect; // Same as height * iaspect / easpect
//            }
//            else if (easpect > iaspect)
//            {
//                // Increase width so that height / width = iaspect
//                width = height / iaspect; // Same as width * easpect / iaspect
//            }

//            this.MinX = Convert.ToInt32(midX - (scaleFactor * width / 2));
//            this.MinY = Convert.ToInt32(midY - (scaleFactor * height / 2));
//            this.MaxX = Convert.ToInt32(midX + (scaleFactor * width / 2));
//            this.MaxY = Convert.ToInt32(midY + (scaleFactor * height / 2));
//        }
//        catch (Exception x)
//        {
//            if (Math.Abs(scaleFactor - 1.001) < 0.0005)
//            {
//                // ReSharper disable once ExceptionNotDocumented
//                // ReSharper disable once ThrowingSystemException
//                throw new Exception(string.Format("{0}:{1}", bbox, x.Message), x);
//            }

//            // 752243,7993085,3376553,11182557 nordkallotten
//            this.MinX = 752243;
//            this.MinY = 7993085;
//            this.MaxX = 3376553;
//            this.MaxY = 11182557;
//            this.CorrectProportionsOfExtentForSpecificHeightAndWidth(imageHeight, imageWidth, 1.001);
//        }
//    }

//    /// <summary>
//    /// Calculates scale
//    /// </summary>
//    /// <param name="imageWidth">
//    /// The image width.
//    /// </param>
//    /// <remarks>
//    /// Assumes correct extent - if unsure use ensure...
//    /// </remarks>
//    /// <returns>
//    /// A map scale
//    /// </returns>
//    /// <exception cref="ArgumentException">One of the parameters was 0 or less</exception>
//    public double GetScale(int imageWidth)
//    {
//        this.CheckValidExtent();
//        if (imageWidth <= 0)
//        {
//            throw new ArgumentException("One of the parameters was 0 or less");
//        }

//        const double DPixelsPerInch = 97.6925;
//        const double DPixelsPerMeter = 39.37 * DPixelsPerInch;
//        double mapExtent = this.MaxX - this.MinX;

//        return mapExtent / (imageWidth / DPixelsPerMeter);
//    }

//    /// <summary>
//    /// Sets an extent to a spesific scale
//    /// </summary>
//    /// <param name="imageScale">
//    /// The Scale parameter
//    /// </param>
//    /// <param name="imageWidth">
//    /// the image width
//    /// </param>
//    /// <param name="imageHeight">
//    /// The image height
//    /// </param>
//    public void SetScale(int imageScale, int imageWidth, int imageHeight)
//    {
//        const double ImageDpi = 97.6925;

//        var midX = Convert.ToDouble(this.MinX + this.MaxX) / 2;
//        var midY = Convert.ToDouble(this.MinY + this.MaxY) / 2;
//        const double DPixelsPerMeter = 39.37 * ImageDpi;

//        var theWidth = (imageWidth / DPixelsPerMeter) * imageScale;
//        var theHeight = Convert.ToDouble(imageHeight / imageWidth) * theWidth;
//        this.MinX = Convert.ToInt32(midX - (theWidth / 2));
//        this.MinY = Convert.ToInt32(midY - (theHeight / 2));
//        this.MaxX = Convert.ToInt32(midX + (theWidth / 2));
//        this.MaxY = Convert.ToInt32(midY + (theHeight / 2));
//        this.CorrectProportionsOfExtentForSpecificHeightAndWidth(imageHeight, imageWidth, 1);
//    }

//    /// <summary>
//    /// Creates a bbox string from extent
//    /// </summary>
//    /// <returns>
//    /// a bbox string
//    /// </returns>
//    public string ToBboxString()
//    {
//        this.CheckValidExtent();

//        return this.MinX + "," + this.MinY + "," + this.MaxX + "," + this.MaxY;
//    }

//    /// <summary>
//    /// COnvert to wkt polygon string
//    /// </summary>
//    /// <returns>
//    /// A string thing
//    /// </returns>
//    public string ToWkt()
//    {
//        return string.Format("POLYGON(({0} {1},{2} {1},{2} {3},{0} {3},{0} {1}))", this.MinX, this.MinY, this.MaxX, this.MaxY);
//    }

//    /// <summary>
//    /// Create extent intersection
//    /// </summary>
//    /// <param name="extent">
//    /// ti intersect with
//    /// </param>
//    /// <returns>
//    /// a mapextent thingy
//    /// </returns>
//    public MapExtent Intersect(MapExtent extent)
//    {
//        var newExtent = new MapExtent { MinX = new[] { this.MinX, extent.MinX }.Max(), MinY = new[] { this.MinY, extent.MinY }.Max(), MaxX = new[] { this.MaxX, extent.MaxX }.Min(), MaxY = new[] { this.MaxY, extent.MaxY }.Min() };
//        return newExtent.IsValid() ? newExtent : null;
//    }

//    /// <summary>
//    /// The is an empty extent. No min max set.....
//    /// </summary>
//    /// <returns>
//    /// The <see cref="bool"/>.
//    /// </returns>
//    public bool IsEmptyExtent()
//    {
//        return this.MinX == 0 && this.MaxX == 0 && this.MinY == 0 && this.MaxY == 0;
//    }

//    /// <summary>
//    /// Grow an extent by a set number of units in each direction
//    /// </summary>
//    /// <param name="numberOfUnitsToExtend">
//    /// The number of units to extend.
//    /// </param>
//    public void Grow(int numberOfUnitsToExtend)
//    {
//        this.MinX -= numberOfUnitsToExtend;
//        this.MaxX += numberOfUnitsToExtend;
//        this.MinY -= numberOfUnitsToExtend;
//        this.MaxY += numberOfUnitsToExtend;
//    }

//    /// <summary>
//    /// compare to extents
//    /// </summary>
//    /// <param name="mapExtent">
//    /// </param>
//    /// <returns>
//    /// true if extent is outside the other
//    /// </returns>
//    public bool IsGreaterThan(MapExtent mapExtent)
//    {
//        return this.MinX < mapExtent.MinX || this.MinY < mapExtent.MinY || this.MaxX > mapExtent.MaxX || this.MaxY > mapExtent.MaxY;
//    }

//    #endregion

//    #region Methods

//    /// <summary>
//    /// Correct the decimal separator for a string containing a decimal number
//    /// </summary>
//    /// <param name="value">
//    /// String containing a decimal number
//    /// </param>
//    /// <returns>
//    /// String containing a corrected decimal number
//    /// </returns>
//    private static string CorrectDecimalSeparator(string value)
//    {
//        return Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ","
//                   ? value.Replace('.', ',')
//                   : value.Replace(',', '.');
//    }

//    /// <summary>
//    /// Checks current extent for validity
//    /// </summary>
//    private void CheckValidExtent()
//    {
//        if (this.MaxX == this.MinX || this.MaxY == this.MinY)
//        {
//            throw new ArgumentException("NonValid extent");
//        }
//    }

//    /// <summary>
//    /// Order min og max kordinater .. hvis det er usikkerhet omkring rekkefølgen på dem
//    /// </summary>
//    /// <remarks>
//    /// Kjapp og dirty
//    /// </remarks>
//    private void EnsureCorrectCoordinateOrder()
//    {
//        if (this.MinX > this.MaxX)
//        {
//            var midX = this.MaxX;
//            this.MaxX = this.MinX;
//            this.MinX = midX;
//        }

//        if (this.MinY <= this.MaxY)
//        {
//            return;
//        }

//        var midY = this.MaxY;
//        this.MaxY = this.MinY;
//        this.MinY = midY;
//    }

//    #endregion

//    private bool IsValid()
//    {
//        return this.MaxX > this.MinX && this.MaxY > this.MinY;
//    }
//}

///// <summary>
///// This view represents data needed to display sites on a map.
///// </summary>
//public class SiteMapExtentView
//{
//    private int accuracy;

//    /// <summary>
//    /// Gets or sets SiteId.
//    /// </summary>
//    public int SiteId { get; set; }

//    /// <summary>
//    /// Gets or sets XCoord.
//    /// </summary>
//    public int XCoord { get; set; }

//    /// <summary>
//    /// Gets or sets YCoord.
//    /// </summary>
//    public int YCoord { get; set; }

//    /// <summary>
//    /// Gets or sets Accuracy.
//    /// To avoid zooming to much in on the map, the accuracy is never set to less than 250.
//    /// </summary>
//    public int Accuracy
//    {
//        get
//        {
//            return this.accuracy;
//        }

//        set
//        {
//            this.accuracy = value < 250 ? 250 : value;
//        }
//    }
//}

///// <summary>
///// Domain object for Site
///// </summary>
//[Serializable]
//public class Site
//{
//    public int Id { get; set; }

//    /// <summary>
//    /// The coordinate system used for TrueXCoord, TrueYCoord, XCoord and YCoord
//    /// </summary>
//    public const CoordinateSystem.CoordinateSystemName DatabaseCoordinateSystem = CoordinateSystem.CoordinateSystemName.GoogleMercator;

//    /// <summary>
//    /// Default accuracy
//    /// </summary>
//    public const int DefaultAccuracy = 10;

//    /// <summary>
//    /// Default diffusion
//    /// </summary>
//    public const int DefaultDiffusion = 0;

//    /// <summary>
//    /// siteType
//    /// </summary>
//    private int siteType;

//    /// <summary>
//    /// Initializes a new instance of the <see cref="Site"/> class.
//    /// </summary>
//    public Site()
//    {
//        IsWithinAllowedArea = true;
//    }

//    #region Properties for Core Site object

//    /// <summary>
//    /// Gets or sets Parent.
//    /// </summary>    
//    public virtual Site Parent { get; set; }

//    /// <summary>
//    /// Gets or sets the parent id.
//    /// </summary>
//    public virtual int? ParentId { get; set; }

//    /// <summary>
//    /// Gets or sets IncludedBySiteId.
//    /// </summary>
//    public virtual int? IncludedBySiteId { get; set; }

//    /// <summary>
//    /// Gets or sets Name.
//    /// </summary>
//    [Required]
//    [StringLength(75)]
//    public virtual string Name { get; set; }

//    /// <summary>
//    /// Gets or sets InputString. 
//    /// Wich persists the (Coordinate, observers coordinate, cordinatesystem)/mgrs and accuracy which the user originaly entered.  
//    /// Gets the comma separated InputString in the form "coordinateSystemName;xEastLongitudeMGRS;yNorthLatitude;accuracy;obeservatorXEastLongitudeMGRS;observatorYNorthLatitude".
//    /// (Text is as entered). 
//    /// Parser in SiteCoordinateInfo(string inputString).
//    /// </summary>
//    public virtual string PersistedCoordinate { get; set; }

//    /// <summary>
//    /// Gets or sets Accuracy.
//    /// </summary>
//    [Required]
//    public virtual int Accuracy { get; set; }

//    ///// <summary>
//    ///// Gets or sets AccuracyDisplay.
//    ///// </summary>   
//    //[Required]
//    //public virtual Accuracy AccuracyDisplay
//    //{
//    //    get
//    //    {
//    //        var accuracy = new Accuracy
//    //        {
//    //            Distance = Accuracy,
//    //            Id = Accuracy,
//    //            Value = Accuracy.ToString(CultureInfo.InvariantCulture) + " m"
//    //        };
//    //        return accuracy;
//    //    }

//    //    set
//    //    {
//    //        Accuracy = value == null ? DefaultAccuracy : value.Distance;
//    //    }
//    //}

//    /// <summary>
//    /// Diffusion
//    /// </summary>    
//    public virtual int Diffusion { get; set; }

//    /// <summary>
//    /// Diffusion
//    /// </summary>    
//    //public virtual Diffusion DiffusionDisplay { get; set; }

//    /// <summary>
//    /// Exact CoordNorth
//    /// This is the exact Y coordinate in the internal database  coordinate system, see <see cref="DatabaseCoordinateSystem"/>
//    /// </summary>
//    [Required]
//    public virtual int TrueYCoord { get; set; }

//    /// <summary>
//    /// Exact CoordEast
//    /// This is the X coordinate in the internal database  coordinate system, see <see cref="DatabaseCoordinateSystem"/>
//    /// </summary>
//    [Required]
//    public virtual int TrueXCoord { get; set; }

//    /// <summary>
//    /// Gets or sets CoordEast.
//    /// This is the X coordinate in the internal database  coordinate system, see <see cref="DatabaseCoordinateSystem"/>
//    /// </summary>
//    [Required]
//    public virtual int XCoord { get; set; }

//    /// <summary>
//    /// Gets or sets CoordNorth
//    /// This is the Y coordinate in the internal database  coordinate system, see <see cref="DatabaseCoordinateSystem"/>
//    /// </summary>
//    [Required]
//    public virtual int YCoord { get; set; }

//    /// <summary>
//    /// Gets or sets SpeciesGroup
//    /// </summary>    
//    public virtual SpeciesGroup SpeciesGroup { get; set; }

//    /// <summary>
//    /// Gets or sets a value indicating whether this Site is a private site. 
//    /// </summary>
//    public virtual bool IsPrivate { get; set; }

//    /// <summary>
//    /// Gets or sets site category id.
//    /// </summary>    
//    public virtual int SiteCategoryId { get; set; }

//    /// <summary>
//    /// Gets or sets site class id.
//    /// </summary>    
//    //public virtual SiteClass SiteClass { get; set; }
//    /// <summary>
//    /// Gets or sets Comment.
//    /// </summary>    
//    [StringLength(255)]
//    public virtual string Comment { get; set; }

//    /// <summary>
//    /// Gets or sets Areas.
//    /// </summary>    
//    public virtual ICollection<Area> Areas { get; set; }

//    /// <summary>
//    /// Gets or sets Project.
//    /// </summary>
//    public virtual Project Project { get; set; }

//    /// <summary>
//    /// Gets or sets the id of a site when associated to a project
//    /// </summary>
//    public virtual string ExternalId { get; set; }

//    /// <summary>
//    /// Gets or sets the AreaId. Has only value if it is a geographic site representing an area.
//    /// </summary>
//    public virtual int? AreaId { get; set; }

//    /// <summary>
//    /// Gets or sets valid daterange for a site
//    /// </summary>
//    /// <remarks>
//    /// A site might no longer be valid - a pond removed 
//    /// </remarks>
//    public virtual DateTime? StartDate { get; set; }

//    /// <summary>
//    /// Gets or sets EndDate.
//    /// </summary>
//    /// <remarks>
//    /// A site might no longer be valid - a pond removed 
//    /// </remarks>
//    public virtual DateTime? EndDate { get; set; }

//    /// <summary>
//    /// Gets or sets ControlingOrganisation.
//    /// </summary>
//    public virtual Organization ControlingOrganisation { get; set; }

//    /// <summary>
//    /// Gets or sets ControlingUser.
//    /// </summary>
//    //public virtual User ControlingUser { get; set; }

//    /// <summary>
//    /// Gets or sets a value indicating whether this Site is within allowed area, be it Norway or Sweden. 
//    /// </summary>
//    public virtual bool IsWithinAllowedArea { get; set; }

//    #endregion

//    /// <summary>
//    /// Gets IncludingSite.
//    /// If the site has been included by another site, the including site will be returned.
//    /// </summary>   
//    public virtual Site IncludingSite
//    {
//        get
//        {
//            // If the includebysiteid isn't the the same as the site id then the parent site should be used // johan n 2012-05-23
//            if (IncludedBySiteId != Id)
//            {
//                return Parent;
//            }

//            return this;
//        }
//    }

//    /// <summary>
//    /// Gets and sets sitetype
//    /// </summary>
//    /// <returns>
//    /// returns a number like 0, 1, 2
//    /// </returns>
//    //public virtual int SiteType
//    //{
//    //    get
//    //    {
//    //        if (IsPrivate)
//    //        {
//    //            var sType = 0;
//    //            if (Diffusion != 0)
//    //            {
//    //                if (siteType == Constants.SiteType.UnDiffused)
//    //                {
//    //                    sType = siteType;
//    //                }
//    //                else
//    //                {
//    //                    sType = Constants.SiteType.Diffused;
//    //                }
//    //            }
//    //            else
//    //            {
//    //                sType = Constants.SiteType.Private;
//    //            }
//    //            return sType;
//    //        }
//    //        if (Diffusion != 0)
//    //        {
//    //            return Constants.SiteType.Diffused;
//    //        }

//    //        return Parent == null ? Constants.SiteType.Parent : Constants.SiteType.Child;
//    //    }
//    //    set
//    //    {
//    //        siteType = value;
//    //    }
//    //}

//    /// <summary>
//    /// Gets site color for gui presentation
//    /// </summary>
//    /// <returns>
//    /// color code
//    /// </returns>
//    public virtual string ColorCode
//    {
//        get
//        {
//            if (IsPrivate && Parent != null)
//            {
//                return SiteColorString.PrivateWithParent;
//            }

//            if (IsPrivate)
//            {
//                return SiteColorString.Private;
//            }

//            if (Parent != null)
//            {
//                return SiteColorString.Child;
//            }

//            if (Parent == null && SiteCategoryId == (int)SiteCategory.LocalitySite)
//            {
//                return SiteColorString.MainSiteForPositions;
//            }

//            return SiteColorString.Parent;
//        }
//    }

//    /// <summary>
//    /// Gets the true coordinates of a site
//    /// </summary>
//    /// <returns>
//    /// optionally transformed coordinates
//    /// </returns>
//    public virtual int[] TrueCoordinatesArray => new[] { TrueXCoord, TrueYCoord };

//    /// <summary>
//    /// Gets The coordinates of a site
//    /// </summary>
//    /// <returns>
//    /// optionally transformed coordinates
//    /// </returns>
//    public virtual int[] CoordinatesArray => new[] { XCoord, YCoord };

//    /// <summary>
//    /// Gets or sets the site presentation name. SiteName + ParentSiteName + RegionName.
//    /// <remarks>presentation name is calculated by ISiteService.SaveSite.
//    /// This maps to PresentationNameRegion.</remarks>
//    /// </summary>
//    public virtual string PresentationName { get; set; }

//    /// <summary>
//    /// Gets or sets the presentation name parish region.
//    /// </summary>
//    public virtual string PresentationNameParishRegion { get; set; }


//    /// <summary>
//    /// Makes string description of project and associated externalId
//    /// </summary>
//    /// <returns>
//    /// A string rep
//    /// </returns>
//    //public virtual string GetProjectAndExternalId()
//    //{
//    //    string result = string.Empty;
//    //    if (Project == null)
//    //    {
//    //        return result;
//    //    }

//    //    if (ExternalId.IsNullOrEmpty())
//    //    {
//    //        return Project.ProjectName;
//    //    }

//    //    return Project.ProjectName + " (" + ExternalId + ")";
//    //}

//    /// <summary>
//    /// Returns the name of an area
//    /// </summary>
//    /// <param name="areaDatasetId">The area to search for</param>
//    /// <returns>The name of the found area</returns>
//    //public virtual string GetAreaName(int areaDatasetId)
//    //{
//    //    return Areas == null ? string.Empty : (from a in Areas where a.AreaDataset.Id == areaDatasetId select a.Name).FirstOrDefault();
//    //}

//    /// <summary>
//    /// Returns the short name of an area
//    /// </summary>
//    /// <param name="areaDatasetId">The area to search for</param>
//    /// <returns>The name of the found area</returns>
//    //public virtual string GetAreaShortName(int areaDatasetId)
//    //{
//    //    return Areas == null ? string.Empty : (from a in Areas where a.AreaDataset.Id == areaDatasetId select a.ShortName).FirstOrDefault();
//    //}

//    /// <summary>
//    /// Returns the name of an area
//    /// </summary>
//    /// <param name="areaDatasetId">The area to search for</param>
//    /// <returns>The name of the found area</returns>
//    //public virtual string GetAreaName(IEnumerable<int> areaDatasetId)
//    //{
//    //    if (Areas == null)
//    //    {
//    //        return string.Empty;
//    //    }

//    //    foreach (var area in
//    //        areaDatasetId.Select(i1 => (from a in Areas where a.AreaDataset.Id == i1 select a).FirstOrDefault()).Where(areaName => areaName != null))
//    //    {
//    //        return area.Name;
//    //    }

//    //    return string.Empty;
//    //}

//    /// <summary>
//    /// Check whether site is bird site
//    /// </summary>
//    /// <returns>
//    /// true or false
//    /// </returns>
//    //public virtual bool IsPublicBirdSite()
//    //{
//    //    return SpeciesGroup != null && SpeciesGroup.Id == SpeciesGroupId.Birds && !IsPrivate;
//    //}

//    /// <summary>
//    /// Check wheter the site is associated to one of the projects
//    /// </summary>
//    /// <param name="projectIds">
//    /// Comma separated string of projects
//    /// </param>
//    /// <returns>
//    /// true or false
//    /// </returns>
//    //public virtual bool IsAssociatedToOneProject(string projectIds)
//    //{
//    //    if (projectIds.IsNullOrEmpty())
//    //    {
//    //        return false;
//    //    }

//    //    return Project != null && projectIds.Split(',').ToList().Contains(Project.Id.ToString());
//    //}

//    /// <summary>
//    /// The get project name.
//    /// </summary>
//    /// <returns>
//    /// The <see cref="string"/>.
//    /// </returns>
//    //public virtual string GetProjectName()
//    //{
//    //    return Project != null ? Project.ProjectName : string.Empty;
//    //}

//    /// <summary>
//    /// The get project id.
//    /// </summary>
//    /// <returns>
//    /// The <see cref="int"/>.
//    /// </returns>
//    public virtual int? GetProjectId()
//    {
//        return Project != null ? (int?)Project.Id : null;
//    }

//    /// <summary>
//    /// Computes the distance from a true point
//    /// </summary>
//    /// <param name="coordinate">
//    /// The point
//    /// </param>
//    /// <returns>
//    /// The distance
//    /// </returns>
//    public virtual double DistanceFromTruePoint(Coordinate coordinate)
//    {
//        // 10.0 * and 0.05 * to round to nerest 10 and 1 google mercator is ~0.5 m
//        return 10.0 * Math.Round(0.05 * Math.Sqrt(Math.Pow(coordinate.X - TrueXCoord, 2) + Math.Pow(coordinate.Y - TrueYCoord, 2)));
//    }

//    /// <summary>
//    /// Computes the distance from a point
//    /// </summary>
//    /// <param name="coordinate">
//    /// The point
//    /// </param>
//    /// <returns>
//    /// The distance
//    /// </returns>
//    public virtual double DistanceFromPoint(Coordinate coordinate)
//    {
//        // 10.0 * and 0.05 * to round to nerest 10 and 1 google mercator is ~0.5 m
//        return 10.0 * Math.Round(0.05 * Math.Sqrt(Math.Pow(coordinate.X - XCoord, 2) + Math.Pow(coordinate.Y - YCoord, 2)));
//    }

//    /// <summary>
//    /// Method to make a copy of site
//    /// </summary>
//    /// <param name="controllingUser">
//    /// The controlling User.
//    /// </param>
//    /// <param name="controllingOrganization">
//    /// The controlling Organization.
//    /// </param>
//    /// <returns>
//    /// A copy of the site
//    /// </returns>
//    //public virtual Site MakeCopy(User controllingUser, Organization controllingOrganization)
//    //{
//    //    if (controllingOrganization != null && controllingUser != null)
//    //    {
//    //        throw new ArgumentException("Site can't have both a user and organization as owner.");
//    //    }

//    //    var site = new Site
//    //    {
//    //        Accuracy = Accuracy,
//    //        Diffusion = Diffusion,
//    //        TrueXCoord = TrueXCoord,
//    //        TrueYCoord = TrueYCoord,
//    //        XCoord = XCoord,
//    //        YCoord = YCoord,
//    //        Name = Name,
//    //        ControlingOrganisation = controllingOrganization,
//    //        ControlingUser = controllingUser,
//    //        SiteCategoryId = SiteCategoryId,
//    //    };

//    //    var areas = new List<Area>();
//    //    areas.AddRange(Areas);
//    //    site.Areas = areas;
//    //    return site;
//    //}
//}

///// <summary>
///// The site geometry.
///// </summary>
//public class SiteGeometry
//{
//    public int Id { get; set; }

//    /// <summary>
//    /// Initializes a new instance of the <see cref="SiteGeometry"/> class.
//    /// </summary>
//    /// <param name="wktGeometry">
//    /// The wkt geometry.
//    /// </param>
//    public SiteGeometry(string wktGeometry)
//    {
//        var wkt = new WKTReader();

//        // ReSharper disable DoNotCallOverridableMethodsInConstructor
//        Geometry = wkt.Read(wktGeometry).AsText();
//        // ReSharper restore DoNotCallOverridableMethodsInConstructor
//        if (!IsValid || !wktGeometry.ToLowerInvariant().Contains("polygon"))
//        {
//            //throw new RulesException("Site", "ValidationError_GeometryIsNotValid");
//        }
//    }

//    /// <summary>
//    /// Initializes a new instance of the <see cref="SiteGeometry"/> class.
//    /// </summary>
//    public SiteGeometry()
//    {
//    }

//    /// <summary>
//    /// Gets or sets the site the geometry belongs to.
//    /// </summary>
//    public virtual Site Site { get; set; }

//    /// <summary>
//    /// Gets or sets Geometry
//    /// </summary>
//    public virtual string Geometry { get; set; }

//    /// <summary>
//    /// Gets or sets UserId.
//    /// </summary>
//    //public virtual User User { get; set; }

//    /// <summary>
//    /// Gets or sets Areas.
//    /// </summary>
//    public virtual ICollection<Area> Areas { get; set; }

//    /// <summary>
//    /// Gets coordinates for geometry
//    /// </summary>
//    public virtual Coordinate[] GetGeometryPoints
//    {
//        get
//        {
//            var wkt = new WKTReader();
//            var geom = wkt.Read(Geometry);
//            return geom.Coordinates;
//        }
//    }

//    /// <summary>
//    /// Gets The coordinates of a site polygon
//    /// </summary>
//    /// <returns>
//    /// optionally transformed coordinates
//    /// </returns>
//    public virtual IEnumerable<double[]>[] GetCoordinateArray
//    {
//        get
//        {
//            return new[] { from c in GetGeometryPoints select new[] { c.X, c.Y } };
//        }
//    }

//    /// <summary>
//    /// Gets the get polygon size.
//    /// </summary>
//    public virtual int PolygonRadius
//    {
//        get
//        {
//            return CalculateAccuracyFromGeometry(Geometry);
//        }
//    }

//    /// <summary>
//    /// Gets or sets the type of the site.
//    /// </summary>
//    public virtual int SiteType { get; set; }

//    /// <summary>
//    /// Gets or sets the proportion size.
//    /// </summary>
//    public virtual int ProportionSize { get; set; }

//    /// <summary>
//    /// Gets or sets the proportion rotation.
//    /// </summary>
//    public virtual string ProportionRotation { get; set; }

//    /// <summary>
//    /// Gets or sets the proportion height.
//    /// </summary>
//    public virtual string ProportionHeight { get; set; }

//    /// <summary>
//    /// Gets or sets the proportion colour.
//    /// </summary>
//    public virtual string ProportionColour { get; set; }

//    /// <summary>
//    /// Gets or sets the number of sightings.
//    /// </summary>
//    public virtual int NumberOfSightings { get; set; }

//    /// <summary>
//    /// Gets a value indicating whether geometry is valid or not
//    /// </summary>
//    public virtual bool IsValid
//    {
//        get
//        {
//            var wkt = new WKTReader();
//            var geom = wkt.Read(Geometry);
//            return geom.IsValid;
//        }
//    }

//    /// <summary>
//    /// The calculate accuracy from geometry.
//    /// </summary>
//    /// <param name="geometry">
//    /// The geometry.
//    /// </param>
//    /// <returns>
//    /// The <see cref="int"/>.
//    /// </returns>
//    public static int CalculateAccuracyFromGeometry(string geometry)
//    {
//        var wkt = new WKTReader();

//        var geom = wkt.Read(geometry);

//        var coordinates = geom.Coordinates;
//        var centroid = geom.Centroid;
//        var diameter = coordinates.Select(coordinate => Math.Sqrt(Math.Pow(coordinate.X - centroid.X, 2) + Math.Pow(coordinate.Y - centroid.Y, 2))).Concat(new[] { 0.0 }).Max();

//        // Must divide by 4 and not 2 for this to be correct. (Distance between two points is 0.5 meters?)
//        return Convert.ToInt32(diameter / 2);
//    }

//    /// <summary>
//    /// The get centroid.
//    /// </summary>
//    /// <param name="geometry">
//    /// The geometry.
//    /// </param>
//    /// <returns>
//    /// The <see cref="Tuple"/>.
//    /// </returns>
//    public static Tuple<int, int> GetCentroid(string geometry)
//    {
//        var wkt = new WKTReader();

//        var geom = wkt.Read(geometry);

//        var centroid = geom.Centroid;
//        return new Tuple<int, int>(Convert.ToInt32(centroid.X), Convert.ToInt32(centroid.Y));
//    }
//}

//public class CoordinateSystem
//{
//    #region Static Fields

//    /// <summary>
//    ///   The default coordinate system for users of this system.
//    /// </summary>
//    public static readonly CoordinateSystemName DefaultCoordinateSystem = CoordinateSystemName.Rt9025GonVSweref;

//    #endregion

//    #region Enums

//    /// <summary>
//    ///   Enumeration of coordinate systems
//    /// </summary>
//    public enum CoordinateSystemName
//    {
//        /// <summary>
//        ///   RT90 2,5 gon V
//        /// </summary>
//        Rt9025GonVSweref = 1,

//        /// <summary>
//        ///   Ed50UtmZone29N sys
//        /// </summary>
//        Ed50UtmZone29N = 21,

//        /// <summary>
//        ///   Ed50UtmZone30N sys
//        /// </summary>
//        Ed50UtmZone30N = 2,

//        /// <summary>
//        ///   Ed50UtmZone31N sys
//        /// </summary>
//        Ed50UtmZone31N = 3,

//        /// <summary>
//        ///   The e d 50_ ut m_zone_32 n. sys
//        /// </summary>
//        Ed50UtmZone32N = 4,

//        /// <summary>
//        ///   The e d 50_ ut m_zone_33 n. sys
//        /// </summary>
//        Ed50UtmZone33N = 5,

//        /// <summary>
//        ///   The e d 50_ ut m_zone_34 n. sys
//        /// </summary>
//        Ed50UtmZone34N = 6,

//        /// <summary>
//        ///   The e d 50_ ut m_zone_35 n. sys
//        /// </summary>
//        Ed50UtmZone35N = 7,

//        /// <summary>
//        ///   The e d 50_ ut m_zone_36 n. sys
//        /// </summary>
//        Ed50UtmZone36N = 8,

//        /// <summary>
//        ///   The e d 50_ ut m_zone_37 n. sys
//        /// </summary>
//        Ed50UtmZone37N = 9,

//        /// <summary>
//        ///   The l l_ wg s 84. sys
//        /// </summary>
//        LlWgs84 = 10,

//        /// <summary>
//        ///   The wg s_84_ ut m_zone_29 n. sys
//        /// </summary>
//        Wgs84UtmZone29N = 22,

//        /// <summary>
//        ///   The wg s_84_ ut m_zone_30 n. sys
//        /// </summary>
//        Wgs84UtmZone30N = 11,

//        /// <summary>
//        ///   The wg s_84_ ut m_zone_31 n. sys
//        /// </summary>
//        Wgs84UtmZone31N = 12,

//        /// <summary>
//        ///   The wg s_84_ ut m_zone_32 n. sys
//        /// </summary>
//        Wgs84UtmZone32N = 13,

//        /// <summary>
//        ///   The wg s_84_ ut m_zone_33 n. sys
//        /// </summary>
//        Wgs84UtmZone33N = 14,

//        /// <summary>
//        ///   The wg s_84_ ut m_zone_34 n. sys
//        /// </summary>
//        Wgs84UtmZone34N = 15,

//        /// <summary>
//        ///   The wg s_84_ ut m_zone_35 n. sys
//        /// </summary>
//        Wgs84UtmZone35N = 16,

//        /// <summary>
//        ///   The wg s_84_ ut m_zone_36 n. sys
//        /// </summary>
//        Wgs84UtmZone36N = 17,

//        /// <summary>
//        ///   The wg s_84_ ut m_zone_37 n. sys
//        /// </summary>
//        Wgs84UtmZone37N = 18,

//        /// <summary>
//        ///   The coordinate system used in google maps.
//        /// </summary>
//        GoogleMercator = 19,

//        /// <summary>
//        ///   Old sweedish dish
//        /// </summary>
//        Rt9025GonW = 20
//    }

//    #endregion

//    #region Public Properties

//    /// <summary>
//    ///   Gets or sets Coordiate system grouping id.
//    /// </summary>
//    //[Required]    
//    //public virtual CoordinateSystemGrouping CoordinateSystemGrouping { get; set; }

//    /// <summary>
//    ///   Gets or sets EpsgId.
//    /// </summary>
//    /// <remarks>
//    ///   Identical to the EPSG code of the coordinate system
//    /// </remarks>
//    [Required]
//    public virtual int EpsgId { get; set; }

//    /// <summary>
//    ///   Gets or sets Id.
//    /// </summary>
//    /// <remarks>
//    ///   PKid from table
//    /// </remarks>
//    public virtual int Id { get; set; }

//    /// <summary>
//    ///   Gets or sets Name.
//    /// </summary>
//    [Required]
//    [StringLength(40)]
//    public virtual string Name { get; set; }

//    /// <summary>
//    ///   Gets or sets Rank/Sortorder
//    /// </summary>
//    [Required]
//    public virtual byte Rank { get; set; }

//    /// <summary>
//    ///   Gets Coordiate system type. (just a proxy for the CoordinateSystemGrouping.Type)
//    /// </summary>
//    //public virtual CoordinateSystemGrouping.CoordinateSystemType Type
//    //{
//    //    get
//    //    {
//    //        return this.CoordinateSystemGrouping.Type;
//    //    }
//    //}

//    /// <summary>
//    ///   Gets or sets WktFormat.
//    /// </summary>
//    [Required]
//    [StringLength(1500)]
//    public virtual string WktFormat { get; set; }

//    /// <summary>
//    ///   Gets or sets Zone string (UI-name used in conjunction with CoordinateSystemGrouping.GroupName)
//    /// </summary>
//    [StringLength(50)]
//    public virtual string ZoneName { get; set; }

//    #endregion
//}

//public class Area
//{
//    /// <summary>
//    /// Gets or sets the Id
//    /// </summary>
//    public int Id { get; set; }

//    /// <summary>
//    /// Gets or sets the AreaTypeId
//    /// </summary>
//    public int AreaTypeId { get; set; }

//    /// <summary>
//    /// Gets or sets the AreaTypeName
//    /// </summary>
//    public string AreaTypeName { get; set; }

//    /// <summary>
//    /// Gets or sets the ExternalId
//    /// </summary>
//    public string ExternalId { get; set; }

//    /// <summary>
//    /// Gets or sets the name
//    /// </summary>
//    public string Name { get; set; }

//    /// <summary>
//    /// Gets or sets the shortname
//    /// </summary>
//    public string ShortName { get; set; }

//    /// <summary>
//    /// Gets or sets Bbox string on the standard format "left,bottom,right,top" in Google Mercator coordinates.
//    /// </summary>
//    public string Bbox { get; set; }
//}

///// <summary>
///// SiteCategory enum values
///// </summary>
//public enum SiteCategory
//{
//    /// <summary>
//    /// Normal site
//    /// </summary>
//    NormalSite = 1,

//    /// <summary>
//    /// Area site
//    /// </summary>
//    AreaSite = 2,

//    /// <summary>
//    /// Locality (ortsnamnen)
//    /// </summary>
//    LocalitySite = 3,

//    /// <summary>
//    /// Position site
//    /// </summary>
//    PositionSite = 10
//}
