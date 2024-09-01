namespace try_upgrade.Services
{

    public class FileSystemFileService : IFileService
    {
        public IEnumerable<string> GetFilesInDirectory(string directoryPath, string fileExtension)
        {
            return Directory.EnumerateFiles(directoryPath, fileExtension, SearchOption.AllDirectories);
        }
        public string GetDirectoryPath(string filePath)
        {
            return Path.GetDirectoryName(filePath);
        }

        public bool FileExists(string directoryPath, string filePath)
        {
            return File.Exists(Path.Combine(directoryPath, filePath));
        }
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }

}
