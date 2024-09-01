using System.Xml.Linq;

namespace try_upgrade.Services
{
    public interface IXDocumentService
    {
        XDocument Load(string filePath);
        void Save(XDocument document, string filePath);
    }

    public class XDocumentService : IXDocumentService
    {
        public XDocument Load(string filePath)
        {
            return XDocument.Load(filePath);
        }
        public void Save(XDocument document, string filePath)
        {
            document.Save(filePath);
        }
    }

    public interface IFileService
    {
        IEnumerable<string> GetFilesInDirectory(string directoryPath, string fileExtension);
        string GetDirectoryPath(string filePath);
        bool FileExists(string directoryPath, string filePath);
        bool FileExists(string filePath);
    }

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
