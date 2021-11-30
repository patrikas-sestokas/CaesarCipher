using System;
using System.Collections.Generic;
using System.IO;

namespace CaesarCipher
{
    static class Program
    {
        const string DecryptIdentifier = "decrypt";
        const string EncryptIdentifier = "encrypt";
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.Error.WriteLine("Not enough arguments. The operation mode must be specified.");
                Environment.Exit(1);
            }
            var operationMode = args[0];
            var parameters = args[1..];
            if (parameters.Length != 3)
            {
                Console.Error.WriteLine("There are 3 required parameters:\n" +
                                        "{operation identifier}\n" +
                                        "{input}\n" +
                                        "{output}\n" +
                                        "{shift}\n" +
                                        $". Got {string.Join('\n', parameters)} of length {parameters.Length} instead.");
                Environment.Exit(2);
            }
            Stream input = CreateStream(parameters[0], StreamType.Input),
                output = CreateStream(parameters[1], StreamType.Output);
            var key = ReadShift(parameters[2]);
            switch (operationMode)
            {
                case EncryptIdentifier:
                    Transform(input, output, key, OperationType.Encryption);
                    break;
                case DecryptIdentifier:
                    Transform(input, output, key, OperationType.Decryption);
                    break;
                default:
                    Console.Error.WriteLine($"Unknown operation mode \"{operationMode}\".\n" +
                                            "Known operation modes:\n" +
                                            $"{EncryptIdentifier} - Encrypts input stream or file.\n" +
                                            $"{DecryptIdentifier} - Decrypts input stream or file.");
                    Environment.Exit(3);
                    break;
            }
            input.Close();
            output.Close();
            return 0;
        }
        enum StreamType : byte
        {
            Input = 0,
            Output = 1
        }
        static Stream CreateStream(string parameter, StreamType type)
        {
            try
            {
                return type switch
                {
                    StreamType.Input => parameter == "-" ? Console.OpenStandardInput() : File.OpenRead(parameter),
                    StreamType.Output => parameter == "-" ? Console.OpenStandardOutput() : File.Create(parameter),
                    _ => throw new ArgumentException($"Unknown stream type \"{type}\".")
                };
            }
            catch(Exception exception)
            {
                Console.Error.WriteLine($"Problems opening provided " +
                                        $"{type switch { StreamType.Input => "input", StreamType.Output => "output", _ => ""}} " +
                                        $"file \"{parameter}\":\n" + exception);
                Environment.Exit(4);
                return null;
            }
        }
        enum OperationType : byte
        {
            Encryption = 0,
            Decryption = 1
        }
        static void Transform(Stream input, Stream output, byte shift, OperationType type)
        {
            var buffer = new byte[64 * 1024];
            int read;
            Func<byte, byte, byte> operation = type switch
            {
                OperationType.Encryption => (x, offset) => unchecked(x += offset),
                OperationType.Decryption => (x, offset) => unchecked(x -= offset),
                _ => throw new ArgumentException($"Unknown operation type \"{type}\".")
            };
            while ((read = input.Read(buffer)) != 0)
            {
                for (var i = 0; i < read; i++)
                    buffer[i] = operation(buffer[i], shift);
                output.Write(buffer, 0, read);
            }
        }
        static byte ReadShift(string parameter)
        {
            var success = byte.TryParse(parameter, out var shift);
            if (success) return shift;
            Console.Error.WriteLine($"Failed to parse \"{parameter}\" as byte.");
            Environment.Exit(5);
            return default;
        }
    }
}