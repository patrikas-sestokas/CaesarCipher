using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using CaesarCipher;
using Xunit;

namespace CaesarCipherTests
{
    public class Main
    {
        [Fact]
        public void HelpArgument()
        {
            Assert.Equal(0, Program.Main(new []{"help"}));
        }
        [Theory]
        [InlineData(0)]
        [InlineData(4)]
        [InlineData(5)]
        public void WrongNumberOfArguments(int argCount)
        {
            Assert.Throws<ConsoleException>(() => Program.Main(new string[argCount]));
        }
        [Theory]
        [InlineData("10", "SomeRandomNonExistentFile", Program.StandardOutputIdentifier)]
        [InlineData("Something that is not a number", Program.StandardInputIdentifier, Program.StandardOutputIdentifier)]
        public void WrongArguments(string shift, string input, string output)
        {
            Assert.Throws<ConsoleException>(() => Program.Main(new [] {shift, input, output}));
        }
        // Application hanging when it's expecting data from stdin and nothing being provided is expected behavior.
        [Theory]
        [InlineData("10", Program.StandardInputIdentifier, Program.StandardOutputIdentifier)]
        [InlineData("-10", Program.StandardInputIdentifier, "SomeFile")]
        public async Task HangingTest(string shift, string input, string output)
        {
            var application = Task.Run(() => Program.Main(new[] { shift, input, output }));
            var timeOut = Task.Delay(TimeSpan.FromSeconds(5));
            Assert.Equal(await Task.WhenAny(application, timeOut), timeOut);
        }
        static IEnumerable<object[]> GenerateData()
        {
            var rng = new Random();
            for (var i = 0; i < 10; ++i)
            {
                var file = Path.GetRandomFileName();
                var stream = File.Create(file);
                var buffer = new byte[rng.Next(1024 * 1024, 50 * 1024 * 1024)];
                rng.NextBytes(buffer);
                stream.Write(buffer);
                stream.Close();
                yield return new object[] { rng.Next(int.MinValue, int.MaxValue), file, Path.GetRandomFileName() };
            }
        }
        /// <summary>
        /// Practically the same test as in Transform.cs, this time with real files.
        /// </summary>
        [Theory]
        [MemberData(nameof(GenerateData))]
        public void RuntimeFileTests(string shift, string input, string output)
        {
            using var md5 = MD5.Create();
            var inputFile = File.Open(input, FileMode.Open, FileAccess.Read);
            var sourceHash = md5.ComputeHash(inputFile);
            inputFile.Close();
            Program.Main(new[] { shift, input, output });
            var outputFile = File.Open(output, FileMode.Open, FileAccess.Read);
            var resultHash = md5.ComputeHash(outputFile);
            outputFile.Close();
            Assert.NotEqual(sourceHash, resultHash);
            
            Program.Main(new[] { (-int.Parse(shift)).ToString(), output, input });
            inputFile = File.Open(input, FileMode.Open, FileAccess.Read);
            var decryptedFileHash = md5.ComputeHash(inputFile);
            inputFile.Close();
            Assert.Equal(sourceHash, decryptedFileHash);
            File.Delete(input);
            File.Delete(output);
        }
    }
}