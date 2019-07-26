using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Runtime.Caching;
using static System.Console;
using System;

namespace DataProcessor
{
    class Program
    {
        // Items will be checked for expiration on a 20 second loop
        private static MemoryCache FilesToProcess = MemoryCache.Default;

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

                ProcessExistingFiles(directoryToWatch);

                // FileSystemWatcher implements IDisposable, huzzah!
                /* Our file watcher will add items to our concurrent dictionary of work items
                 * while our timer will perodically process all items in that dictionary
                 */
                using (var watcher = new FileSystemWatcher(directoryToWatch))
                //using (var timer = new Timer(ProcessFiles, null, 0, 1000))

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
        /// Add any files in the given directory to the process cache
        /// </summary>
        /// <param name="inputDirectory"></param>
        private static void ProcessExistingFiles(string inputDirectory)
        {
            WriteLine($"Checking {inputDirectory} for existing files");

            foreach (var filePath in Directory.EnumerateFiles(inputDirectory))
            {
                WriteLine($"\t> Found {filePath}");
                AddToCache(filePath);
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
            ProcessSingleFile(e.FullPath);
        }

        private static void FileCreated(object sender, FileSystemEventArgs e)
        {
            WriteLine($"* File created: {e.Name} - Type: {e.ChangeType}");
            ProcessSingleFile(e.FullPath);
        }

        private static void ProcessSingleFile(string filePath)
        {
            //RunProcessOnFile(filePath);

            /* If the file already exists in this dictionary, it won't be added again */
            //FilesToProcess.TryAdd(filePath, filePath);
            AddToCache(filePath);
        }

        private static void AddToCache(string filePath)
        {
            var item = new CacheItem(filePath, filePath);
            var policy = new CacheItemPolicy
            {
                RemovedCallback = ProcessFile,

                // if item hasn't been accessed in 2 seconds, remove from cache. This should be > 1 sec
                SlidingExpiration = TimeSpan.FromSeconds(2)
            };

            // if an item with the same key is attempted to be added, the key won't be re-added, but
            // the sliding expiration will be updated
            FilesToProcess.Add(item, policy);
        }

        // Called when a cached file expires (not accessed for the sliding expiration span)
        private static void ProcessFile(CacheEntryRemovedArguments args)
        {
            var item = args.CacheItem.Key;
            WriteLine($"* Cached item removed: {item} because {args.RemovedReason}");

            if (args.RemovedReason == CacheEntryRemovedReason.Expired)
            {
                RunProcessOnFile(item);
            }
            else
            {
                WriteLine($"WARNING: {item} was removed unexpectedly and may not have been processed (removed reason: {args.RemovedReason})");
            }
        }

        /// <summary>
        /// Process every file in a given directory of the given file type.
        /// </summary>
        /// <param name="directoryPath"></param>
        /// <param name="fileType"></param>
        private static void ProcessDirectory(string directoryPath, string fileType)
        {
            //var allFiles = Directory.GetFiles(directoryPath);  // get all files

            switch (fileType.ToUpper())
            {
                case "TEXT":
                    string[] textFiles = Directory.GetFiles(directoryPath, "*.txt");
                    foreach (var textFilePath in textFiles)
                    {
                        RunProcessOnFile(textFilePath);
                    }
                    break;
                default:
                    WriteLine($"ERROR: {fileType} is not supported");
                    return;
            }
        }


        /// <summary>
        /// New up a file processor and process the given file
        /// </summary>
        /// <param name="filePath"></param>
        private static void RunProcessOnFile(string filePath)
        {
            var fileProcessor = new FileProcessor(filePath);
            fileProcessor.Process();
        }
    }
}
