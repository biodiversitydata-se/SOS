namespace SOS.Lib.Configuration.Export
{
     /// <summary>
     /// Settings for blob storage
     /// </summary>
    public class BlobStorageConfiguration
    {
        /// <summary>
        /// Name of storage account
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// Storage connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Storage key
        /// </summary>
        public string Key { get; set; }
    }
}
