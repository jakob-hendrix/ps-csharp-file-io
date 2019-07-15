using static System.Console;
using System.IO;
using System;

namespace DataProcessor
{
    /// <summary>
    /// Process a file
    /// </summary>
    internal class FileProcessor
    {
        private static readonly string BackupDirectoryName = "backup";
        private static readonly string InProgressDirectoryName = "processing";
        private static readonly string CompletedDirectoryName = "complete";

        public string InputFilePath { get; }

        public FileProcessor(string filePath)
        {
            this.InputFilePath = filePath;
        }

        public void Process()
        {
            WriteLine($"Begin processing of: {InputFilePath}");

            if (!File.Exists(InputFilePath))
            {
                WriteLine($"ERROR: file {InputFilePath} not found.");
                return;
            }

            string inputFileDirectory = Path.GetDirectoryName(InputFilePath);
            string rootDirectoryPath = new DirectoryInfo(inputFileDirectory).Parent.FullName;

            WriteLine($"Input Directory is: {inputFileDirectory}");
            WriteLine($"Root data path is: {rootDirectoryPath}");

            // Check if a backup directory exists. If not, go ahead an make it
            string backupDirectoryPath = Path.Combine(rootDirectoryPath, BackupDirectoryName);

            //if (!Directory.Exists(backupDirectoryPath))
            //{
            //    WriteLine($"Creating: {backupDirectoryPath}");
            //    Directory.CreateDirectory(backupDirectoryPath);
            //}
            Directory.CreateDirectory(backupDirectoryPath);

            // Copy file to backup dir
            string inputFileName = Path.GetFileName(InputFilePath);
            string backupFilePath = Path.Combine(backupDirectoryPath, inputFileName);
            WriteLine($"Copying: {InputFilePath} -> {backupFilePath}");

            // will overwrite the destination file
            File.Copy(InputFilePath, backupFilePath, true);


            // Move to in-progress
            Directory.CreateDirectory(Path.Combine(rootDirectoryPath, InProgressDirectoryName));
            string inProgressFilePath = Path.Combine(rootDirectoryPath, InProgressDirectoryName, inputFileName);
            WriteLine($"Moving: {InputFilePath} -> {inProgressFilePath}");
            
            if (File.Exists(inProgressFilePath))
            {
                WriteLine($"ERROR: a file with the name {inProgressFilePath} is already being processed");
                return;
            }

            File.Move(InputFilePath, inProgressFilePath);

            // Determine the type of file
            string extention = Path.GetExtension(InputFilePath);

            switch (extention)
            {
                case ".txt":
                    ProcessTextFile(inProgressFilePath);
                    break;
                default:
                    WriteLine($"{extention} is an unsupported file type.");
                    break;
            }
        }

        private void ProcessTextFile(string inputFile)
        {
            WriteLine($"Processing text file: {inputFile}");
        }
    }
}