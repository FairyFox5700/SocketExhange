using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Fairy.ConsoleSocketClient
{
    public interface ISonnetGenerator
    {
        string GenerateRandomShakespeareSonnet();
    }

    public class SonnetGenerator : ISonnetGenerator
    {
        private const string StringSplit = "Sonnet";
        private static Regex RX= new(StringSplit, RegexOptions.Compiled);
        private static string FileName = "Shekspire.txt";
        public  string GenerateRandomShakespeareSonnet()
        {
            try
            {
                using var sr = new StreamReader(FileName);
                var text = sr.ReadToEnd();
                var sonnets = RX.Split(text);
                var randomIndex = new Random().Next(sonnets.Length);
                return StringSplit+sonnets[randomIndex];
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                return "";
            }
        }
    }
}