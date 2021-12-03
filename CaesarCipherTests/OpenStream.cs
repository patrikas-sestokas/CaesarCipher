using System;
using System.Collections.Generic;
using System.IO;
using CaesarCipher;
using Xunit;

namespace CaesarCipherTests
{
    public class OpenStream
    {
        /// <summary>
        ///     Tests if correct type of exception is thrown from wrong internal argument.
        /// </summary>
        /// <param name="type"></param>
        [Theory]
        [InlineData((Program.StreamType)2)]
        [InlineData((Program.StreamType)255)]
        public void OpenStreamWithWrongStreamType(Program.StreamType type)
        {
            Assert.Throws<ArgumentException>(() => Program.OpenStream("-", type));
        }
        public static IEnumerable<object[]> RandomNonExistentFiles()
        {
            for (var i = 0; i < 5; ++i)
                yield return new object[] { Guid.NewGuid().ToString() };
        }
        /// <summary>
        ///     Checks if non existent source file throws the expected ConsoleException exception type.
        /// </summary>
        /// <param name="parameter">Source file path</param>
        [Theory]
        [MemberData(nameof(RandomNonExistentFiles))]
        public void OpenStreamWithNonExistentFiles(string parameter)
        {
            Assert.Throws<ConsoleException>(() => Program.OpenStream(parameter, Program.StreamType.Input));
        }
        public static IEnumerable<object[]> RandomFiles(bool accessible)
        {
            var rng = new Random();
            for (var i = 0; i < 10; ++i)
            {
                var file = Path.GetRandomFileName();
                var type = (Program.StreamType)(i / 5);
                var stream = File.Create(file);
                var buffer = new byte[rng.Next(1, 512 * 1024)];
                rng.NextBytes(buffer);
                stream.Write(buffer);
                stream.Close();
                yield return new object[]
                {
                    new FileStream(stream.Name, FileMode.Open, FileAccess.Read,
                        type is Program.StreamType.Input ? accessible ? FileShare.Read : FileShare.Write :
                        accessible ? FileShare.Write : FileShare.Read),
                    type
                };
            }
        }
        /// <summary>
        ///     Checks if non-accessible files cause the expected ConsoleException type to be thrown.
        /// </summary>
        /// <param name="stream">Generated file stream.</param>
        /// <param name="type">Type of stream.</param>
        [Theory]
        [MemberData(nameof(RandomFiles), false)]
        public void OpenStreamWithNonAccessibleFiles(FileStream stream, Program.StreamType type)
        {
            Assert.Throws<ConsoleException>(() => Program.OpenStream(stream.Name, type));
            stream.Dispose();
            File.Delete(stream.Name);
        }
        [Theory]
        [MemberData(nameof(RandomFiles), true)]
        public void OpenStreamWithAccessibleFiles(FileStream stream, Program.StreamType type)
        {
            var testStream = Program.OpenStream(stream.Name, type);
            testStream.Close();
            stream.Close();
            File.Delete(stream.Name);
        }
    }
}