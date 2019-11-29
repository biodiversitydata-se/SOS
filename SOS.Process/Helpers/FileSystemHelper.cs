using System;
using System.IO;
using System.Threading;

namespace SOS.Process.Helpers
{
    public static class FileSystemHelper
    {
        private static readonly Random Random = new Random();

        /// <summary>
        /// Initializes a new instance of the FileStream class.
        /// If the file is locked, this method waits for the file lock to be released.
        /// </summary>
        /// <returns></returns>
        public static FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for (int numTries = 0; numTries < 10; numTries++)
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

    }
}
