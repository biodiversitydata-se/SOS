using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Administration.Gui.Models
{
    public class InvalidLocationDto
    {
        public string DataSetName { get; set; }
        public string DataSetId { get; set; }
        public string OccurrenceId { get; set; }
        public float Lat { get; set; }
        public float Lon { get; set; }
    }
}
