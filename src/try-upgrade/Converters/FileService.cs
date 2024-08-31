namespace try_upgrade.Converters
{
    public class FileService
    {
        public FileService(string path)
        {
            FilePath = path;
        }

        public string FilePath { get; private set; }
        public virtual string? GetDirectoryName()
        {
            return Path.GetDirectoryName(FilePath);
        }

        public virtual IEnumerable<string> GetFilesInDirectory(string searchPattern="*.*")
        {
            return Directory.EnumerateFiles(FilePath, searchPattern, SearchOption.AllDirectories);
        }
    }
}
