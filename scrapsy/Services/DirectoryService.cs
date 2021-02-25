using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace scrapsy.Services
{
    public class DirectoryService
    {
        public DirectoryService()
        {
            CurrentDirectory = Directory.GetCurrentDirectory();
            ConfigDirectory = CurrentDirectory + @"\Config";
            AuthenticationDirectory = ConfigDirectory + @"\Authentication";
        }

        public string CurrentDirectory { get; }
        public string ConfigDirectory { get; }
        public string AuthenticationDirectory { get; }

        public static bool CheckIfFilesExist(string path, string searchPattern)
        {
            var di = new DirectoryInfo(path);
            return di.EnumerateFiles(searchPattern).Any();
        }

        public static IEnumerable<FileInfo> GetFilesInDirectory(string path, string searchPattern)
        {
            var di = new DirectoryInfo(path);
            return di.EnumerateFiles(searchPattern);
        }
    }
}