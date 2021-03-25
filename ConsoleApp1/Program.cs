using System;
using System.IO;

namespace OracleParameterDecoder
{
    class Program
    {
        static void Main(string[] args)
        {
            var queryFilePath = new FileInfo(@"C:\Temp\query.txt");

            var decoded = Decoder.DecodeMessage(File.ReadAllText(queryFilePath.FullName));

            var decodedFilePath = Path.Combine(queryFilePath.Directory.FullName, Path.GetFileNameWithoutExtension(queryFilePath.Name) + "_decoded" + Path.GetExtension(queryFilePath.Name));
            File.WriteAllText(decodedFilePath, decoded);

            Console.WriteLine("Decoded at => " + decodedFilePath);

            Console.ReadKey();
        }
    }
}
