using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Moq;
using SOS.Lib.Constants;
using SOS.Lib.Enums;
using SOS.Lib.Enums.FieldMappingValues;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Processed.Observation;
using SOS.Lib.Models.Shared;
using SOS.Process.Helpers.Interfaces;
using SOS.Process.Processors.DarwinCoreArchive;
using SOS.Process.UnitTests.TestHelpers;
using SOS.Process.UnitTests.TestHelpers.Factories;
using SOS.TestHelpers.Gis;
using SOS.TestHelpers.Helpers.Builders;
using SOS.TestHelpers.Taxonomy;
using Xunit;

namespace SOS.Process.UnitTests.Processors.DarwinCoreArchive.DwcaObservationFactory
{
    [CollectionDefinition("DwcaObservationFactory collection")]
    public class EventTests : IClassFixture<DwcaObservationFactoryFixture>
    {
        private readonly DwcaObservationFactoryFixture _fixture;

        public EventTests(DwcaObservationFactoryFixture fixture)
        {
            _fixture = fixture;
        }

    }
}