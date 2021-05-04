using System.Collections.Generic;
using System.Linq;
using SOS.Lib.Extensions;
using SOS.Lib.Models.Shared;
using SOS.Observations.Api.Extensions;

namespace SOS.Observations.Api.Dtos
{
    /// <summary>
    /// Information about a taxon list.
    /// </summary>
    public class TaxonListDto
    {
        /// <summary>
        /// The Id of the taxon list.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The parent Id of the taxon list.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// The Id in Taxon list service.
        /// </summary>
        public int TaxonListServiceId { get; set; }

        /// <summary>
        ///     The name of the taxon list.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The taxa in this taxon list.
        /// </summary>
        public IEnumerable<TaxonListTaxonInformationDto> Taxa { get; set; }

        /// <summary>
        /// Creates a new TaxonListDto object.
        /// </summary>
        /// <param name="taxonList"></param>
        /// <param name="cultureCode"></param>
        /// <returns></returns>
        public static TaxonListDto Create(TaxonList taxonList, string cultureCode)
        {
            if (taxonList == null)
            {
                return null;
            }
            return new TaxonListDto
            {
                Id = taxonList.Id,
                ParentId = taxonList.ParentId,
                TaxonListServiceId = taxonList.TaxonListServiceId,
                Name = taxonList.Names?.Translate(cultureCode),
                Taxa = taxonList.Taxa.Select(m => m.ToTaxonListTaxonInformationDto())
            };
        }
    }
}