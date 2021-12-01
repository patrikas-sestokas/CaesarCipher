# Caesar Cipher command line tool

[Caesar Cipher](https://en.wikipedia.org/wiki/Caesar_cipher) is an algorithm that shifts symbols of input data by user defined offset.


This command line tool provides capability to shift all bytes of provided input (file and standard input are both supported) and write it to user defined output (can be either standard output or a file).

## Prerequisites for compiling code yourself

- [.NET 5](https://dotnet.microsoft.com/download/dotnet/5.0)
- Both of the listed IDEs have been confirmed to work:
  - Microsoft Visual Studio 2019
  - JetBrains Rider

## Usage

Command line interface only accepts either 1 or 3 commands - `help` or `{shift} {input} {output}` where:
- `help` provides usage instructions.
- `{shift}` is a number by which to offset bytes of provided input.
- `{input}` is either a file path or standard input marked by "-".
- `{output}` is either a file path or standard output marked by "-".

⚠️ **If `{output}` file already exists - it will rewritten!**

To decrypt previously encrypted data simply pass `{shift}` with inverted sign. 

For example if data was encrypted with `{shift}` of `5` - it can be decrypted with `-5`.

## Usage examples
- #### Using no files and `{shift}` of "5":
  ```echo Hello World! | CaesarCipher 5 - -``` which results in `Mjqqt%\twqi&%↕☼`.
  
  Passing said output into the tool again with inverted `{shift}` like so:

  ```echo Hello World! | CaesarCipher 5 - - | CaesarCipher -5 - -```

  Results in `Hello World!`.

- #### Using files and `{shift}` of "-7":

  For example file `data.txt` in the same directory as the executable contains:

  ```The quick brown fox jumps over the lazy dog.```

  Running it through the tool: ```CaesarCipher -7 data.txt encrypted.txt```.

  Will create a file `encrypted.txt` in the same directory as executable with obviously garbled content:

  ```Ma^↓jnb\d↓[khpg↓_hq↓cnfil↓ho^k↓ma^↓eZsr↓]h`'```

  Running encrypted file through the tool with inverted `{shift}` like so: ```CaesarCipher 7 encrypted.txt decrypted.txt```.

  Will result in file `decrypted.txt` with content:

  ```The quick brown fox jumps over the lazy dog.```

- #### Using a mix of both files and standard input/output:

  ```CaesarCipher -7 data.txt -``` results in ```Ma^↓jnb\d↓[khpg↓_hq↓cnfil↓ho^k↓ma^↓eZsr↓]h`'```

  Piping said output through ```CaesarCipher -7 data.txt - | CaesarCipher 7 - -``` will result in:

  ```The quick brown fox jumps over the lazy dog.```
  
  Using ```CaesarCipher -7 data.txt - | CaesarCipher 7 - copy.txt``` will result in file `copy.txt` with content:
  
  ```The quick brown fox jumps over the lazy dog.```
  
## Known behavior
- `{output}` files are overwritten.
- Launching tool with input method as standard input like so ```CaesarCipher 5 - -``` will result in application waiting for said input. Forever, if necessary. 