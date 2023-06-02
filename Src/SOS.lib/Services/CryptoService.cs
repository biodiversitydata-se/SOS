using SOS.Lib.Configuration.Shared;
using SOS.Lib.Services.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SOS.Lib.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly byte[] _iv;
        private readonly byte[] _key;


        /// <summary>
        ///  Create key from password
        /// </summary>
        /// <param name="password"></param>
        /// <param name="salt"></param>
        /// <param name="desiredKeyLength"></param>
        /// <returns></returns>
        private byte[] DeriveFromPassword(string password, string salt, int desiredKeyLength)
        {
            var iterations = 1000;
            var hashMethod = HashAlgorithmName.SHA384;

            return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
                    Encoding.Unicode.GetBytes(salt),
                    iterations,
                    hashMethod,
                    desiredKeyLength);
        }

        /// <summary>
        /// Initialize aes
        /// </summary>
        /// <param name="aes"></param>
        private void InitializeAes(Aes aes)
        {
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;
            aes.Key = _key;
            aes.IV = _iv;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public CryptoService(CryptoConfiguration cryptoConfiguration)
        {
            if (cryptoConfiguration == null)
            {
                throw new ArgumentNullException(nameof(cryptoConfiguration));
            }

            _iv = DeriveFromPassword(cryptoConfiguration.Password, cryptoConfiguration.Salt, 16);
            _key = DeriveFromPassword(cryptoConfiguration.Password, cryptoConfiguration.Salt, 32);
        }

        /// <inheritdoc/>
        public async Task<string> DecryptAsync(string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted?.Trim()))
            {
                return null!;
            }
            using Aes aes = Aes.Create();
            InitializeAes(aes);
            // using MemoryStream input = new(Encoding.Unicode.GetBytes(encrypted));
            using MemoryStream input = new(Convert.FromBase64String(encrypted));
            using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using MemoryStream output = new();
            await cryptoStream.CopyToAsync(output);
            return Encoding.Unicode.GetString(output.ToArray());
        }

       

        /// <inheritdoc/>
        public async Task<string> EncryptAsync(string clearText)
        {
            if (string.IsNullOrEmpty(clearText?.Trim()))
            {
                return null!;
            }

            using Aes aes = Aes.Create();
            InitializeAes(aes);
            using MemoryStream output = new();
            using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
            var textBytes = Encoding.Unicode.GetBytes(clearText);
            await cryptoStream.WriteAsync(textBytes);
            await cryptoStream.FlushFinalBlockAsync();

            return Convert.ToBase64String(output.ToArray());
            //return Encoding.Unicode.GetString(output.ToArray());
        }
    }
}
