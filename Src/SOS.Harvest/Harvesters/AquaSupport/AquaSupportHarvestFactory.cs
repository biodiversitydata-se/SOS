﻿using SOS.Harvest.Harvesters;
using SOS.Harvest.Harvesters.Interfaces;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Interfaces;
using System.Xml.Linq;

namespace SOS.Harvesters.AquaSupport
{
    public class AquaSupportHarvestFactory<T> : HarvestBaseFactory, IHarvestFactory<XDocument, T> where T : IEntity<int>
    {
        /// <inheritdoc />
        public async Task<IEnumerable<T>?> CastEntitiesToVerbatimsAsync(XDocument xmlDocument)
        {
            return await Task.Run(() =>
            {
                if (xmlDocument?.Root == null)
                {
                    return null;
                }

                var ns = (XNamespace)"http://schemas.datacontract.org/2004/07/ArtDatabanken.WebService.Data";
                var verbatims = new List<T>();

                foreach (var observatioElement in xmlDocument!.Root!
                    .Element(ns + "CreatedSpeciesObservations")!
                    .Elements(ns + "WebSpeciesObservation"))
                {
                    var verbatim = (IEntity<int>)Activator.CreateInstance(typeof(T))!;

                    if (verbatim == null)
                    {
                        throw new NullReferenceException(nameof(verbatim));
                    }

                    verbatim.Id = NextId;
                    foreach (var fieldElement in observatioElement
                        .Elements(ns + "Fields")
                        .Elements(ns + "WebSpeciesObservationField"))
                    {

                        var property = fieldElement!.Element(ns + "Property")!.Value;
                        var value = fieldElement!.Element(ns + "Value")!.Value;

                        verbatim.SetProperty(property, value);
                    }

                    verbatims.Add((T)verbatim);
                }

                foreach (var observatioElement in xmlDocument!.Root!
                    .Element(ns + "UpdatedSpeciesObservations")!
                    .Elements(ns + "WebSpeciesObservation"))
                {
                    var verbatim = Activator.CreateInstance(typeof(T));
                    foreach (var fieldElement in observatioElement
                        .Elements(ns + "Fields")
                        .Elements(ns + "WebSpeciesObservationField"))
                    {

                        var property = fieldElement!.Element(ns + "Property")!.Value;
                        var value = fieldElement!.Element(ns + "Value")!.Value;

                        verbatim.SetProperty(property, value);
                    }

                    verbatims.Add((T)verbatim!);
                }

                return verbatims;
            });
        }
    }
}
