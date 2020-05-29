namespace DwC_A
{
    /// <summary>
    ///     Interface to manage archive zip files
    /// </summary>
    public interface IArchiveFolder
    {
        /// <summary>
        ///     Indicates whether the extracted folder is a temp folder that should be cleaned
        ///     up on disposal.
        /// </summary>
        bool ShouldCleanup { get; }

        /// <summary>
        ///     Deletes the folder that files were extracted to.
        /// </summary>
        void DeleteFolder();

        /// <summary>
        ///     Extracts files from archive zip file
        /// </summary>
        /// <returns>Path that files were extracted to</returns>
        string Extract();
    }
}