using System;
using System.Collections.Generic;
using System.Xml.Linq;
using SOS.Lib.Extensions;

namespace SOS.Import.Extensions
{
    public static class AquaSupportObservationExtensions
    {
        /// <summary>
        ///     Cast multiple sightings entities to models .
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static IEnumerable<T> ToVerbatims<T>(this XDocument xmlDocument, XNamespace ns)
        {
            var verbatims = new List<T>();

            foreach (var observatioElement in xmlDocument.Root
                .Element(ns + "CreatedSpeciesObservations")
                .Elements(ns + "WebSpeciesObservation"))
            {
               var verbatim = Activator.CreateInstance(typeof(T));
                foreach (var fieldElement in observatioElement
                    .Elements(ns + "Fields")
                    .Elements(ns + "WebSpeciesObservationField"))
                {

                    var property = fieldElement.Element(ns + "Property").Value;
                    var value = fieldElement.Element(ns + "Value").Value;

                    verbatim.SetProperty(property, value);
                }

                verbatims.Add((T)verbatim);
            }

            foreach (var observatioElement in xmlDocument.Root
                .Element(ns + "UpdatedSpeciesObservations")
                .Elements(ns + "WebSpeciesObservation"))
            {
                var verbatim = Activator.CreateInstance(typeof(T));
                foreach (var fieldElement in observatioElement
                    .Elements(ns + "Fields")
                    .Elements(ns + "WebSpeciesObservationField"))
                {

                    var property = fieldElement.Element(ns + "Property").Value;
                    var value = fieldElement.Element(ns + "Value").Value;

                    verbatim.SetProperty(property, value);
                }

                verbatims.Add((T)verbatim);
            }

            return verbatims;
        }
    }
}