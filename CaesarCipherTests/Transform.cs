using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CaesarCipher;
using Xunit;

namespace CaesarCipherTests
{
    public class Transform
    {
        static IEnumerable<object[]> RandomStreams()
        {
            var rng = new Random();
            for (var i = 0; i < 10; ++i)
            {
                var input = new MemoryStream();
                var buffer = new byte[rng.Next(100 * 1024 * 1024, 300 * 1024 * 1024)];
                rng.NextBytes(buffer);
                input.Write(buffer);
                input.Seek(0, SeekOrigin.Begin);
                yield return new object[] { input, rng.Next(int.MinValue, int.MaxValue) };
            }
        }
        /// <summary>
        /// Compares hashes of encrypted and source data.
        /// <para>1.Encrypts data and compares hashes of encrypted data and source data - they should NOT be equal.</para>
        /// <para>2.Decrypts encrypted data and compares hashes of decrypted data and source data - they should be equal.</para>
        /// </summary>
        /// <param name="input">Data input stream</param>
        /// <param name="shift"></param>
        [Theory]
        [MemberData(nameof(RandomStreams))]
        public void TransformHash(Stream input, int shift)
        {
            var output = new MemoryStream();
            using var md5 = MD5.Create();
            var sourceHash = md5.ComputeHash(input);
            var task = Task.Run(() => Program.Transform(input, output, shift));
            var encryptedDataHash = md5.ComputeHash(output);
            task.Wait();
            //Encrypted data should not have the same hash as source data.
            Assert.NotEqual(sourceHash, encryptedDataHash);
            
            output.Seek(0, SeekOrigin.Begin);
            input.Seek(0, SeekOrigin.Begin);
            task = Task.Run(() => Program.Transform(output, input, -shift));
            var decryptedDataHash = md5.ComputeHash(input);
            task.Wait();
            //Decrypting encrypted data should result in hash identical to the hash of source data.
            Assert.Equal(sourceHash, decryptedDataHash);
        }
    }
}