using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SOS.Administration.Gui.Models
{
    [BsonIgnoreExtraElements]
    public class DataProviderDto
    {
        public int Id { get; set; }

        /// <summary>
        ///     Name of data set
        /// </summary>
        public string Name { get; set; }

    }
}
