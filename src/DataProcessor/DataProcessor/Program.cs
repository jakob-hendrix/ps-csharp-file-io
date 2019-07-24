using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static System.Console;

namespace DataProcessor
{
    class Program
    {
        private static ConcurrentDictionary<string, string> FilesToProcess = new ConcurrentDictionary<string, string>();

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
                /* Our file watcher will add items to our concurrent dictionary of work items
                 * while our timer will perodically process all items in that dictionary
                 */
                using (var watcher = new FileSystemWatcher(directoryToWatch))
                using (var timer = new Timer(ProcessFiles, null, 0, 1000))
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


        /// <summary>
        /// This method is handles processing files inside our ConcurrentDictonary of work items
        /// </summary>
        /// <param name="state"></param>
        private static void ProcessFiles(object state)
        {
            foreach (var filename in FilesToProcess.Keys)  // no guaranteed ordering
            {
                // `out _` satisfies the arguments but we don't care about it
                if (FilesToProcess.TryRemove(filename, out _))
                {
                    var fileProcessor = new FileProcessor(filename);
                    fileProcessor.Process();
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

            //var processor = new FileProcessor(e.FullPath);
            //processor.Process();

            FilesToProcess.TryAdd(e.FullPath, e.FullPath);
        }

        private static void FileCreated(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File created: {e.Name} - Type: {e.ChangeType}");

            //var processor = new FileProcessor(e.FullPath);
            //processor.Process();

            /* If the file already exists in this dictionary, it won't be added again */
            FilesToProcess.TryAdd(e.FullPath, e.FullPath);
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
