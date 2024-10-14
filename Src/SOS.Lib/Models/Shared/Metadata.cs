using System.Collections.Generic;

namespace SOS.Lib.Models.Shared
{
    /// <summary>
    ///     Represents different metadata items
    /// </summary>
    public class Metadata<T>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="id"></param>
        public Metadata(T id)
        {
            Id = id;
            Translations = new List<MetadataTranslation>();
        }

        /// <summary>
        ///     Id of item
        /// </summary>
        public T Id { get; set; }

        /// <summary>
        ///     Name of item
        /// </summary>
        public ICollection<MetadataTranslation> Translations { get; set; }
    }
}