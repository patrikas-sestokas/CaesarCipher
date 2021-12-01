using CaesarCipher;
using Xunit;

namespace CaesarCipherTests
{
    public class ReadShift
    {
        /// <summary>
        /// Checks if specifically ConsoleException and not some other type of exception is thrown.
        /// </summary>
        /// <param name="input">Simulated user input specifying shift amount.</param>
        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("Hello")]
        [InlineData("2147483648")] // int.MaxValue + 1
        [InlineData("-2147483649")] // int.MinValue - 1
        [InlineData("0")]
        [InlineData("26")]
        public void ReadShiftWrongInputThrowsConsoleException(string input)
        {
            Assert.Throws<ConsoleException>(() => Program.ReadShift(input));
        }
    }
}
