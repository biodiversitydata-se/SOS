using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Administration.Gui.Models
{
    public class InvalidObservationsDatabaseSettings : IInvalidObservationsDatabaseSettings
    {
        public string CollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }

    public interface IInvalidObservationsDatabaseSettings
    {
        string CollectionName { get; set; }
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
    }
}
