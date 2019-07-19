using System;
using System.IO;
using static System.Console;

namespace DataProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            WriteLine("Parsing command line arguments\n");
            WriteLine($"Command: {args[0]}\nTarget: {args[1]}\n");

            // command line validation omitted for brevity

            var command = args[0];


            switch (command)
            {
                case "--file":
                    var filePath = args[1];
                    WriteLine($"Single file {filePath} selected");
                    ProcessSingleFile(filePath);
                    break;

                case "--dir":
                    var directoryPath = args[1];
                    var fileType = args[2];
                    WriteLine($"Directory {directoryPath} selected for {fileType} files");
                    ProcessDirectory(directoryPath, fileType);
                    break;

                default:
                    WriteLine("Invalid command line options");
                    break;
            }

            WriteLine("\nPress enter to quit");
            ReadLine();
        }



        private static void ProcessSingleFile(string filePath)
        {
            var fileProcessor = new FileProcessor(filePath);
            fileProcessor.Process();
        }
        private static void ProcessDirectory(string directoryPath, string fileType)
        {
            //var allFiles = Directory.GetFiles(directoryPath);  // get all files

            switch (fileType.ToUpper())
            {
                case "TEXT":
                    string[] textFiles = Directory.GetFiles(directoryPath, "*.txt");
                    foreach (var textFilePath in textFiles)
                    {
                        var fileProcessor = new FileProcessor(textFilePath);
                        fileProcessor.Process();
                    }
                    break;
                default:
                    WriteLine($"ERROR: {fileType} is not supported");
                    return;
            }
        }
    }
}
