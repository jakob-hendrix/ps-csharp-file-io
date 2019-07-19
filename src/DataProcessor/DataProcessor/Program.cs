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

            var directoryToWatch = args[0];
            if (!Directory.Exists(directoryToWatch))
            {
                WriteLine($"ERROR: {directoryToWatch} does not exist");
            }
            else
            {
                WriteLine($"Watching directory {directoryToWatch} for changes...");

                // FileSystemWatcher implements IDisposable, huzzah!
                using (var watcher = new FileSystemWatcher(directoryToWatch))
                {
                    watcher.IncludeSubdirectories = false;
                    watcher.InternalBufferSize = 32768; // 32k
                    watcher.Filter = "*.*"; // default filter

                    // What parts of a file can trigger an event
                    watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite;

                    // Wire up our desired events methods
                    watcher.Created += FileCreated;
                    watcher.Changed += FileChanged;
                    watcher.Deleted += FileDeleted;
                    watcher.Renamed += FileRenamed;
                    watcher.Error += WatcherError;

                    // Enables this watcher
                    watcher.EnableRaisingEvents = true;

                    WriteLine($"Press enter to quit.");
                    ReadLine();
                }
            }
        }

        private static void WatcherError(object sender, ErrorEventArgs e)
        {
            WriteLine($"ERROR: file system watching may no longer be active: {e.GetException()}");
        }

        private static void FileRenamed(object sender, RenamedEventArgs e)
        {
            WriteLine($"* File renamed: {e.Name} - Type: {e.ChangeType}");
        }

        private static void FileDeleted(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File deleted: {e.Name} - Type: {e.ChangeType}");
        }

        private static void FileChanged(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File changed: {e.Name} - Type: {e.ChangeType}");

            var processor = new FileProcessor(e.FullPath);
            processor.Process();
        }

        private static void FileCreated(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File created: {e.Name} - Type: {e.ChangeType}");

            var processor = new FileProcessor(e.FullPath);
            processor.Process();
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
