
namespace SOS.Process.Models.Verbatim.Interfaces
{
    /// <summary>
    /// IEntity interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEntity<TKey>
    {
        /// <summary>
        /// Id
        /// </summary>
        TKey Id { get; set; }
    }

    /// <summary>
    /// IEntity interface
    /// </summary>
    public interface IEntity : IEntity<string>
    {

    }
}
