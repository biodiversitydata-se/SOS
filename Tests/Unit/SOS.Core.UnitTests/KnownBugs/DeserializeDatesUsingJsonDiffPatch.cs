using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JsonDiffPatchDotNet;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SOS.Core.Models.Observations;
using SOS.Core.Repositories;
using Xunit;

namespace SOS.Core.Tests.KnownBugs
{
    public class DeserializeDatesUsingJsonDiffPatch
    {

        public class MyTestClass
        {
            public string MyDateAsString { get; set; }
        }

        /// <summary>
        /// This test shows that there can be a problem when deserializing a string field containing a date.
        /// See discussion about the problem here: https://github.com/JamesNK/Newtonsoft.Json/issues/862
        /// </summary>
        /// <returns></returns>
        [Fact]
        [Trait("Category", "Unit")]
        public void TestDeserializeDateStringDiff()
        {
            //-----------------------------------------------------------------------------------------------------------
            // Arrange
            //-----------------------------------------------------------------------------------------------------------
            var  jdp = new JsonDiffPatch();
            var myObj = new MyTestClass
            {
                MyDateAsString = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture)
            };

            //-----------------------------------------------------------------------------------------------------------
            // Act
            //-----------------------------------------------------------------------------------------------------------
            JToken diff = jdp.Diff(JToken.FromObject(myObj), JToken.Parse("{}")); // Simulate deletion of the MyDateAsString property.
            string strDiff = diff.ToString(Formatting.None);
            JToken parsedDiff = JToken.Parse(strDiff); // This parser treats the date string as a date, needs to treat it as string...
            JToken restoredDocument = jdp.Unpatch(
                JToken.Parse("{}"),
                diff);
            JToken restoredDocumentUsingParsedDiff = jdp.Unpatch(
                JToken.Parse("{}"),
                parsedDiff);
            var restoredObject = (MyTestClass)restoredDocument.ToObject(typeof(MyTestClass));
            var restoredObjectUsingParsedDiff = (MyTestClass)restoredDocumentUsingParsedDiff.ToObject(typeof(MyTestClass));


            //-----------------------------------------------------------------------------------------------------------
            // Assert
            //-----------------------------------------------------------------------------------------------------------
            restoredObject.MyDateAsString.Should().Be(myObj.MyDateAsString);
            restoredObjectUsingParsedDiff.MyDateAsString.Should().Be(myObj.MyDateAsString);
        }
    }
}
