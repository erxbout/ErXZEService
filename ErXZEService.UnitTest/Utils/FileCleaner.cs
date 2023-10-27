using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ErXZEService.UnitTest.Utils
{
    public static class FileCleaner
    {
        public static void Cleanup(IEnumerable<string> filePaths)
        {
            foreach (var file in filePaths)
            {
                while (File.Exists(file))
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch (IOException e)
                    {
                        if (e.Message.Contains("used by another process"))
                        {
                            Debug.WriteLine($"Cannot access file to clean up {file}, wait for closing by process");
                            Thread.Sleep(1000);
                        }
                    }
                }
            }
        }
    }
}
