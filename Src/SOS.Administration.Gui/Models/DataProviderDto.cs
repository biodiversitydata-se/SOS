using MongoDB.Bson.Serialization.Attributes;

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
