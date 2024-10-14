namespace SOS.Lib.Models.Shared
{
    /// <summary>
    ///     Represents metadata item with category
    /// </summary>
    public class MetadataWithCategory<T> : Metadata<T>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MetadataWithCategory() : base(default)
        {

        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="id"></param>
        /// <param name="categoryId"></param>
        public MetadataWithCategory(T id, int categoryId) : base(id)
        {
            Category = new Metadata<int>(categoryId);
        }

        /// <summary>
        ///     Category of item
        /// </summary>
        public Metadata<int> Category { get; set; }
    }
}