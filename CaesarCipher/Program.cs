using System;
using System.IO;

namespace CaesarCipher
{
    public enum FailureType
    {
        WrongNumberOfArguments = 1,
        IOError = 2,
        FailedToParse = 3,
        ShiftNotWithinRange = 4
    }
    //To differentiate user input errors from others.
    public class ConsoleException : Exception
    {
        public ConsoleException(string message, FailureType type) : base(message) => HResult = (int)type;
    }
    public static class Program
    {
        public enum StreamType : byte
        {
            Input = 0,
            Output = 1
        }
        public const string StandardInputIdentifier = "-";
        public const string StandardOutputIdentifier = "-";
        static readonly string Help = "{shift} {input} {output} {mode}\n" +
                                      "    {shift} - the amount in range [1, 25] by which to shift bytes of input.\n" +
                                      $"    {{input}} - either file path or {StandardInputIdentifier} specifying standard input.\n" +
                                      $"    {{output}} - either file path or {StandardOutputIdentifier} specifying standard output.";
        public static int Main(string[] args)
        {
            //In case of erroneous user input there is little need to specify which pieces of code caused it.
            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
            {
                if (eventArgs.ExceptionObject is not ConsoleException exception)
                    throw (Exception)eventArgs.ExceptionObject;
                Console.Error.WriteLine(exception.Message);
                Environment.Exit(exception.HResult);
            };
            if (args.Length == 1 && args[0] == "help")
            {
                Console.WriteLine(Help);
                return 0;
            }

            if (args.Length != 3)
                throw new ConsoleException($"Usage instructions:\n{Help}\n" +
                                           $"Got \"{string.Join(' ', args)}\" of length {args.Length} instead.",
                    FailureType.WrongNumberOfArguments);
            var shift = ReadShift(args[0]);
            Stream input = OpenStream(args[1], StreamType.Input),
                output = OpenStream(args[2], StreamType.Output);
            Transform(input, output, shift);
            input.Close();
            output.Close();
            //Standard error because standard output could be piped to another process.
            /*Console.Error.WriteLine($"\nSuccessfully shifted all bytes of {(args[1] == StandardInputIdentifier ? "stdin" : args[1])}" +
                                    $" by {args[0]} and wrote the result to {(args[2] == StandardOutputIdentifier ? "stdout" : args[2])}.");*/
            return 0;
        }
        /// <summary>
        ///     Attempts to open user provided argument as either file or stream.
        /// </summary>
        /// <param name="parameter">User input.</param>
        /// <param name="type">Internally provided type.</param>
        /// <returns>Stream of file or stdin/stdout.</returns>
        /// <exception cref="ArgumentException">In case of wrong internally provided type.</exception>
        /// <exception cref="ConsoleException">In case of failure when opening or creating stream.</exception>
        public static Stream OpenStream(string parameter, StreamType type)
        {
            try
            {
                return type switch
                {
                    StreamType.Input => parameter == StandardInputIdentifier
                        ? Console.OpenStandardInput()
                        : new FileStream(parameter, FileMode.Open, FileAccess.Read, FileShare.Read),
                    StreamType.Output => parameter == StandardOutputIdentifier
                        ? Console.OpenStandardOutput()
                        : new FileStream(parameter, FileMode.Create, FileAccess.Write, FileShare.Read),
                    //As stream type is provided internally, it should never be wrong. If it is - failure is expected.
                    _ => throw new ArgumentException($"Unknown stream type \"{type}\".")
                };
            }
            //Exception thrown by wrong internally provided argument should be handled separately than one arising from user input.
            catch (ArgumentException)
            {
                throw;
            }
            //Way too many different types of exceptions could be thrown in this context leading to unfortunate necessity to generalize.
            catch (Exception exception)
            {
                throw new ConsoleException("Problems opening provided " +
                                           $"{type switch { StreamType.Input => "input", StreamType.Output => "output", _ => "" }} " +
                                           $"file \"{parameter}\":\n" + exception.Message, FailureType.IOError);
            }
        }
        /// <summary>
        ///     Shifts all bytes of input by shift and writes them to output.
        ///     If any of the streams fail while being used - catastrophic failure is expected.
        /// </summary>
        /// <param name="input">Input stream.</param>
        /// <param name="output">Output stream.</param>
        /// <param name="shift">The amount by which to shift bytes.</param>
        public static void Transform(Stream input, Stream output, int shift)
        {
            var buffer = new byte[64 * 1024];
            int read;
            while ((read = input.Read(buffer)) != 0)
            {
                for (var i = 0; i < read; i++)
                    buffer[i] = buffer[i] switch
                    {
                        //Unchecked context allows overflows and underflows to not throw exceptions.
                        //In this case neither overflow nor underflow are mathematically possible assuming shift lies in [1, 25] range.
                        //1. If said byte is in specified range - apply shift.
                        //   In a lot of cases result is outside uppercase or lowercase letter ranges.
                        //3. Unfortunately there is no single operation that can return modulus of x in [y, z] range.
                        //   So downshifting entire range to 0 based range by subtracting 65 or 97 is necessary.
                        //5. Applying modulus operation of 26 results in [0, 25] range, which is not expected output.
                        //6. Meaning that the downshift in step 3 has to be reversed.
                        //[65, 90] - Uppercase letters.
                        > 64 and < 91 => unchecked((byte)((buffer[i] + shift - 65) % 26 + 65)),
                        //[97, 122] - Lowercase letters.
                        > 96 and < 123 => unchecked((byte)((buffer[i] + shift - 97) % 26 + 97)),
                        _ => buffer[i]
                    };
                output.Write(buffer, 0, read);
            }
        }
        /// <summary>
        ///     Reads user provided shift and attempts to parse it.
        /// </summary>
        /// <param name="parameter">User input</param>
        /// <returns>Parsed integer</returns>
        /// <exception cref="ConsoleException">In case of failed parsing attempt</exception>
        public static byte ReadShift(string parameter)
        {
            var success = byte.TryParse(parameter, out var shift);
            if (!success)
                throw new ConsoleException($"Failed to parse \"{parameter}\" as a byte.", FailureType.FailedToParse);
            if (shift is < 1 or > 25)
                throw new ConsoleException("Shift must be in [1, 25] range!", FailureType.ShiftNotWithinRange);
            return shift;
        }
    }
}