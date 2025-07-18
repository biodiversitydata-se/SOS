﻿using SOS.Lib.Enums;
using SOS.Lib.Extensions;
using SOS.Lib.Helpers;
using SOS.Lib.Models.Verbatim.DarwinCore;
using SOS.TestHelpers.Gis;
using System;
using System.Globalization;

namespace SOS.TestHelpers.Helpers.Builders
{
    public class DwcObservationVerbatimBuilder : BuilderBase<DwcObservationVerbatimBuilder, DwcObservationVerbatim>
    {
        public DwcObservationVerbatimBuilder WithDecimalLatitude(double decimalLatitude)
        {
            return With(entity => entity.DecimalLatitude = decimalLatitude.ToString(CultureInfo.InvariantCulture));
        }

        public DwcObservationVerbatimBuilder WithDecimalLatitude(string decimalLatitude)
        {
            return With(entity => entity.DecimalLatitude = decimalLatitude);
        }

        public DwcObservationVerbatimBuilder WithDecimalLongitude(double decimalLongitude)
        {
            return With(entity => entity.DecimalLongitude = decimalLongitude.ToString(CultureInfo.InvariantCulture));
        }

        public DwcObservationVerbatimBuilder WithDecimalLongitude(string decimalLongitude)
        {
            return With(entity => entity.DecimalLongitude = decimalLongitude);
        }

        public DwcObservationVerbatimBuilder WithGeodeticDatum(string geodaticDatum)
        {
            return With(entity => entity.GeodeticDatum = geodaticDatum);
        }

        public DwcObservationVerbatimBuilder WithCoordinateUncertaintyInMeters(int? coordinateUncertaintyInMeters)
        {
            return With(entity => entity.CoordinateUncertaintyInMeters = coordinateUncertaintyInMeters?.ToString());
        }

        public DwcObservationVerbatimBuilder WithCoordinateUncertaintyInMeters(string coordinateUncertaintyInMeters)
        {
            return With(entity => entity.CoordinateUncertaintyInMeters = coordinateUncertaintyInMeters);
        }

        public DwcObservationVerbatimBuilder WithOccurrenceRemarks(string occurrenceRemarks)
        {
            return With(entity => entity.OccurrenceRemarks = occurrenceRemarks);
        }

        public DwcObservationVerbatimBuilder WithSex(string sex)
        {
            return With(entity => entity.Sex = sex);
        }

        public DwcObservationVerbatimBuilder WithLifeStage(string lifeStage)
        {
            return With(entity => entity.LifeStage = lifeStage);
        }

        public DwcObservationVerbatimBuilder WithOrganismQuantity(string organismQuantity)
        {
            return With(entity => entity.OrganismQuantity = organismQuantity);
        }

        public DwcObservationVerbatimBuilder WithReproductiveCondition(string reproductiveCondition)
        {
            return With(entity => entity.ReproductiveCondition = reproductiveCondition);
        }

        public DwcObservationVerbatimBuilder WithBehavior(string behavior)
        {
            return With(entity => entity.Behavior = behavior);
        }

        public DwcObservationVerbatimBuilder WithOccurrenceStatus(string occurrenceStatus)
        {
            return With(entity => entity.OccurrenceStatus = occurrenceStatus);
        }

        public DwcObservationVerbatimBuilder WithScientificName(string scientificName)
        {
            return With(entity => entity.ScientificName = scientificName);
        }

        public DwcObservationVerbatimBuilder WithTaxonId(string taxonId)
        {
            return With(entity => entity.TaxonID = taxonId);
        }

        public DwcObservationVerbatimBuilder WithEventDate(string eventDate)
        {
            return With(entity => entity.EventDate = eventDate);
        }

        public DwcObservationVerbatimBuilder WithEventDate(DateTime eventDate)
        {
            return With(entity => entity.EventDate = DwcFormatter.CreateDateString(eventDate));
        }

        public DwcObservationVerbatimBuilder WithEventTime(string eventTime)
        {
            return With(entity => entity.EventTime = eventTime);
        }

        public DwcObservationVerbatimBuilder WithDateIdentified(string dateIdentified)
        {
            return With(entity => entity.DateIdentified = dateIdentified);
        }

        public DwcObservationVerbatimBuilder WithDateIdentified(DateTime dateIdentified)
        {
            return With(entity => entity.DateIdentified = DwcFormatter.CreateDateString(dateIdentified));
        }

        public DwcObservationVerbatimBuilder WithIdentificationVerificationStatus(
            string identificationVerificationStatus)
        {
            return With(entity => entity.IdentificationVerificationStatus = identificationVerificationStatus);
        }

        public DwcObservationVerbatimBuilder WithDefaultValues()
        {
            WithGeodeticDatum(CoordinateSys.WGS84.EpsgCode());
            WithDecimalLatitude(TestCoordinates.TranasMunicipality.Latitude);
            WithDecimalLongitude(TestCoordinates.TranasMunicipality.Longitude);

            return this;
        }

        protected override DwcObservationVerbatim CreateEntity()
        {
            var observation = new DwcObservationVerbatim();
            return observation;
        }
    }
}