using static System.Console;
using System.IO;

namespace DataProcessor
{
    /// <summary>
    /// Process a file
    /// </summary>
    internal class FileProcessor
    {
        public string InputFilePath;

        public FileProcessor(string filePath)
        {
            this.InputFilePath = filePath;
        }

        public void Process()
        {
            WriteLine($"Begin processing of {InputFilePath}");

            // check if file exists
            if (!File.Exists(InputFilePath))
            {
                WriteLine($"ERROR: file {InputFilePath} not found.");
                return;
            }
        }
    }
}