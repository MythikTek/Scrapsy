using System.IO;

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
    }
}