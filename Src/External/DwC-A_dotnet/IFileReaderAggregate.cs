namespace DwC_A
{
    /// <summary>
    ///     This interface is an aggregate of any FileReader interfaces
    ///     that should be returned by the CreateFileReader method of
    ///     the AbstractFactory and allows the ArchiveReader to maintain
    ///     a consistent collection of FileReader objects without duplication
    ///     for multiple interfaces.
    /// </summary>
    public interface IFileReaderAggregate : IFileReader, IAsyncFileReader
    {
    }
}