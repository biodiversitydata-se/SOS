using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Taxon list definition.
    /// </summary>
    public class TaxonListDefinitionDto
    {
        /// <summary>
        /// The Id of the taxon list.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     The name of the taxon list.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Creates a new TaxonListDefinitionDto object.
        /// </summary>
        /// <param name="taxonList"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        public static TaxonListDefinitionDto Create(TaxonList taxonList, string cultureCode)
        {
            if (taxonList == null)
            {
                return null;
            }
            return new TaxonListDefinitionDto
            {
                Id = taxonList.Id,
                Name = taxonList.Names?.Translate(cultureCode)
            };
        }
    }
}