
namespace SOS.Process.Models.Processed.Interfaces
{
    /// <summary>
    /// IEntity interface
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// Id
        /// </summary>
        TKey DatasetID { get; set; }
    }

    /// <summary>
    /// IEntity interface
    /// </summary>
    public interface IEntity : IEntity<string>
    {

    }
}
