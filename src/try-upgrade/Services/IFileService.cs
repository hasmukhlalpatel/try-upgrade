namespace try_upgrade.Services
{
    public interface IFileService
    {
        IEnumerable<string> GetFilesInDirectory(string directoryPath, string fileExtension);
        string GetDirectoryPath(string filePath);
        bool FileExists(string directoryPath, string filePath);
        bool FileExists(string filePath);
    }

}
