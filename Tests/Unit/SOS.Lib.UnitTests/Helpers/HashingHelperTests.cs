using FluentAssertions;
using SOS.Lib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace SOS.Lib.UnitTests.Helpers
{
    public class HashingHelperTests
    {        
        private string occurrenceCsvRow = "urn:lsid:artportalen.se:Sighting:14727733	HumanObservation		Artportalen			urn:lsid:swedishlifewatch.se:dataprovider:Artportalen	Artportalen					sv		2011-04-29T16:44:00	SLU Artdatabanken		Tom Volgers		1	336	2004-12-";

        /*
         * MD5 yields hexadecimal digits (0-15 / 0-F), so they are four bits each. 128 / 4 = 32 characters.
         */
        [Fact]
        public void Hashing_Use_MD5_Test()
        {
            var result1 = HashingHelper.ComputeHashInLowerCase(occurrenceCsvRow, HashingClassAlgorithms.MD5);
            var result2 = HashingHelper.ComputeHashInUpperCase(occurrenceCsvRow, HashingClassAlgorithms.MD5);
            
            UTF8Encoding.UTF8.GetByteCount(result1).Should().Be(32);
            UTF8Encoding.UTF8.GetByteCount(result2).Should().Be(32);
            result1.ToCharArray().All(c => char.IsLower(c) || char.IsDigit(c)).Should().BeTrue();
            result2.ToCharArray().All(c => char.IsUpper(c) || char.IsDigit(c)).Should().BeTrue();
        }

        [Fact(Skip = "Runs too slow on build server")]
        public void Test_MD5_Performance()
        {            
            int nrIterations = 10000;
            var algorithm = MD5.Create();         
            var sp = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < nrIterations; i++)
            {
                var str = ComputeHash(occurrenceCsvRow, algorithm, false);
            }
            sp.Stop();
            double ticksPerHash = sp.ElapsedTicks / (double)nrIterations;
            
            ticksPerHash.Should().BeLessThan(500);
        }

        [Fact(Skip = "Runs too slow on build server")]
        public void Test_SHA512_Performance()
        {
            int nrIterations = 10000;
            var algorithm = SHA512.Create();
            var sp = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < nrIterations; i++)
            {
                var str = ComputeHash(occurrenceCsvRow, algorithm, false);
            }
            sp.Stop();
            double ticksPerHash = sp.ElapsedTicks / (double)nrIterations;

            ticksPerHash.Should().BeLessThan(1000);
        }

        private static string ComputeHash(string input, HashAlgorithm algorithm, bool upperCase = true)
        {
            string result = string.Empty;
            var hashingService = algorithm;            
            byte[] hash = hashingService.ComputeHash(Encoding.UTF8.GetBytes(input));                
            result = string.Concat(Array.ConvertAll(hash, h => h.ToString($"{(upperCase ? "X2" : "x2")}")));
            return result;
        }
    }
}
