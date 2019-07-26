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
            WriteLine($"\nBegin processing of: {InputFilePath}");

            if (!File.Exists(InputFilePath))
            {
                WriteLine($"ERROR: file {InputFilePath} not found.");
                return;
            }

            string inputFileDirectory = Path.GetDirectoryName(InputFilePath);
            string rootDirectoryPath = new DirectoryInfo(inputFileDirectory).Parent.FullName;
            
            // Check if a backup directory exists. If not, go ahead an make it
            string backupDirectoryPath = Path.Combine(rootDirectoryPath, BackupDirectoryName);

            Directory.CreateDirectory(backupDirectoryPath);

            // Copy file to backup dir
            string inputFileName = Path.GetFileName(InputFilePath);
            string backupFilePath = Path.Combine(backupDirectoryPath, inputFileName);
            File.Copy(InputFilePath, backupFilePath, true);  // will overwrite the destination file
            
            // Move to in-progress
            Directory.CreateDirectory(Path.Combine(rootDirectoryPath, InProgressDirectoryName));
            string inProgressFilePath = Path.Combine(rootDirectoryPath, InProgressDirectoryName, inputFileName);

            //WriteLine($"Moving to in-progress: {InputFilePath} -> {inProgressFilePath}");
            if (File.Exists(inProgressFilePath))
            {
                WriteLine($"ERROR: a file with the name {inProgressFilePath} is already being processed");
                return;
            }
            File.Move(InputFilePath, inProgressFilePath);

            // Determine the type of file
            string extention = Path.GetExtension(InputFilePath);

            string completedDirectoryPath = Path.Combine(rootDirectoryPath, CompletedDirectoryName);
            Directory.CreateDirectory(completedDirectoryPath);
            var completedFileName = $"{Path.GetFileNameWithoutExtension(InputFilePath)}-{Guid.NewGuid()}{extention}";
            var completedFilePath = Path.Combine(completedDirectoryPath, completedFileName);

            // Process the file
            switch (extention)
            {
                case ".txt":
                    var textProcess = new TextFileProcessor(inProgressFilePath, completedFilePath);
                    textProcess.Process();
                    break;
                default:
                    WriteLine($"{extention} is an unsupported file type.");
                    break;
            }

            WriteLine($"Processing complete for: {InputFilePath}\n\n");
            File.Delete(inProgressFilePath);
        }
    }
}