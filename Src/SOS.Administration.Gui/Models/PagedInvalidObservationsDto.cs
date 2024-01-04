using System.Collections.Generic;

namespace SOS.Administration.Gui.Models
{
    public class PagedInvalidObservationsDto
    {
        public IEnumerable<InvalidObservationDto> Observations { get; set; }
        // The number of elements in the page
        public int Size { get; set; }
        // The total number of elements
        public long TotalElements { get; set; }
        // The total number of pages
        public long TotalPages { get; set; }
        // The current page number
        public long PageNumber { get; set; }
    }
}
