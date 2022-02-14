using System;
using System.Security.Cryptography;
using System.Text;

namespace SOS.Lib.Helpers
{    
    public enum HashingClassAlgorithms
    {
        MD5 = 0,
        SHA1 = 1,
        SHA256 = 2,
        SHA384 = 3,
        SHA512 = 4
    }

    public sealed class Hashing
    {
        public HashingClassAlgorithms HashingAlgorithms
        { get; private set; }

        public Hashing(HashingClassAlgorithms hashingAlgorithm)
        {
            this.HashingAlgorithms = hashingAlgorithm;
        }

        public HashAlgorithm CreateNewInstance()
        {
            HashAlgorithm result = null;

            switch (this.HashingAlgorithms)
            {
                case HashingClassAlgorithms.MD5:
                    result = MD5.Create();
                    break;
                case HashingClassAlgorithms.SHA1:
                    result = SHA1Managed.Create();
                    break;
                case HashingClassAlgorithms.SHA256:
                    result = SHA256Managed.Create();
                    break;
                case HashingClassAlgorithms.SHA384:
                    result = SHA384.Create();
                    break;
                case HashingClassAlgorithms.SHA512:
                    result = SHA512Managed.Create();
                    break;
            }

            return result;
        }
    }

    public class HashingHelper
    {
        private static void Validate(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                throw new
                        ArgumentNullException
                        ("Input parameter is empty or only contains whitespace");
            }
        }

        private static void Validate(HashAlgorithm algorithm)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm is null");
            }
        }

        private static HashAlgorithm CreateNewInstance(HashingClassAlgorithms algorithm)
        {
            return new Hashing(algorithm).CreateNewInstance();
        }

        private static void ValidateParams(string input, HashAlgorithm algorithm)
        {
            Validate(input);

            Validate(algorithm);
        }

        private static string ComputeHash(string input, HashAlgorithm algorithm, bool upperCase = true)
        {
            string result = string.Empty;

            var hashingService = algorithm;

            using (hashingService)
            {
                byte[] hash = hashingService.ComputeHash(Encoding.UTF8.GetBytes(input));

                result = string.Concat(Array.ConvertAll(hash, h => h.ToString($"{(upperCase ? "X2" : "x2")}")));
            }

            return result;
        }

        private static HashAlgorithm ProcessAlgorithm(string input, HashingClassAlgorithms algorithm)
        {
            HashAlgorithm result = CreateNewInstance(algorithm);

            ValidateParams(input, result);

            return result;
        }

        public static string ComputeHashInUpperCase(string input, HashingClassAlgorithms algorithm)
        {
            HashAlgorithm result = ProcessAlgorithm(input, algorithm);

            return ComputeHash(input, result);
        }

        /// <summary>
        /// Computes the hash returns its value. 
        /// </summary>
        /// <param name="input">plainText</param>
        /// <param name="algorithm">Name of the algorithm that you are interested using.</param>
        /// <returns></returns>
        public static string ComputeHashInLowerCase(string input, HashingClassAlgorithms algorithm)
        {
            HashAlgorithm result = ProcessAlgorithm(input, algorithm);

            return ComputeHash(input, result, false);
        }
    }
}

