using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SOS.Lib.Helpers
{
    public static class FileSystemHelper
    {
        private static readonly Random Random = new Random();

        /// <summary>
        ///     Initializes a new instance of the FileStream class and opens the file.
        ///     If the file is locked, this method waits for the file lock to be released.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static FileStream WaitForFileAndThenOpenIt(string fullPath)
        {
            return WaitForFile(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        /// <summary>
        ///     Initializes a new instance of the FileStream class.
        ///     If the file is locked, this method waits for the file lock to be released.
        /// </summary>
        /// <returns></returns>
        public static FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for (var numTries = 0; numTries < 10; numTries++)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(fullPath, mode, access, share);
                    return fs;
                }
                catch (IOException)
                {
                    if (fs != null)
                    {
                        fs.Dispose();
                    }

                    Thread.Sleep(Random.Next(25, 100)); // Sleep between 25 to 100 ms
                }
            }

            return null;
        }

        /// <summary>
        /// Waits for a file to be ready and not longer locked by another process.
        /// Usage: await FileSystemHelper.IsFileReady(filePath).ContinueWith(t => { ... });
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async Task IsFileReady(string filename)
        {
            await Task.Run(() =>
            {
                if (!File.Exists(filename))
                {
                    throw new IOException("File does not exist!");
                }

                var isReady = false;
                while (!isReady)
                {
                    // If the file can be opened for exclusive access it means that the file
                    // is no longer locked by another process.
                    try
                    {
                        using FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None);
                        isReady = inputStream.Length > 0;
                    }
                    catch (Exception e)
                    {
                        // Check if the exception is related to an IO error.
                        if (e.GetType() == typeof(IOException))
                        {
                            isReady = false;
                        }
                        else
                        {
                            // Rethrow the exception as it's not an exclusively-opened-exception.
                            throw;
                        }
                    }
                }
            });
        }
    }
}