namespace SOS.Lib.Models.Shared
{
    /// <summary>
    ///     Represents metadata item with category
    /// </summary>
    public class MetadataWithCategory : Metadata
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MetadataWithCategory() : base(0)
        {

        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="categoryId"></param>
        public MetadataWithCategory(int id, int categoryId) : base(id)
        {
            Category = new Metadata(categoryId);
        }

        /// <summary>
        ///     Category of item
        /// </summary>
        public Metadata Category { get; set; }
    }
}