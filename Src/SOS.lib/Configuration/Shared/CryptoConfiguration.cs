namespace SOS.Lib.Configuration.Shared
{
    public class CryptoConfiguration
    {
        /// <summary>
        /// Encryption/decryption password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Encryption/decryption salt
        /// </summary>
        public string Salt { get; set; }
    }
}
