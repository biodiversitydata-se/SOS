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
        private byte[] _iv = 
        {
            0x48, 0xC2, 0xA3, 0x6C, 0x61, 0x56, 0x40, 0x72,
            0x76, 0x31, 0x52, 0x75, 0x6E, 0x74, 0x3A, 0x29
        };

        private readonly byte[] _key;


        /// <summary>
        /// Create key from password
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        private byte[] DeriveKeyFromPassword(string password, string salt)
        {
            var iterations = 1000;
            var desiredKeyLength = 16; // 16 bytes equal 128 bits.
            var hashMethod = HashAlgorithmName.SHA384;
            return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
                    Encoding.Unicode.GetBytes(salt),
                    iterations,
                    hashMethod,
                    desiredKeyLength);
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

            _key = DeriveKeyFromPassword(cryptoConfiguration.Password, cryptoConfiguration.Salt);
        }

        /// <inheritdoc/>
        public async Task<string> DecryptAsync(string encrypted)
        {
            if (string.IsNullOrEmpty(encrypted))
            {
                return null!;
            }
            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using MemoryStream input = new(Encoding.Unicode.GetBytes(encrypted));
            using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using MemoryStream output = new();
            await cryptoStream.CopyToAsync(output);
            return Encoding.Unicode.GetString(output.ToArray());
        }

        /// <inheritdoc/>
        public async Task<string> EncryptAsync(string clearText)
        {
            if (string.IsNullOrEmpty(clearText))
            {
                return null!;
            }

            using Aes aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using MemoryStream output = new();
            using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
            await cryptoStream.WriteAsync(Encoding.Unicode.GetBytes(clearText));
            await cryptoStream.FlushFinalBlockAsync();
            return Encoding.Unicode.GetString(output.ToArray());
        }
    }
}
