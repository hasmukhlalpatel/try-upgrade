using System.Xml.Linq;

namespace try_upgrade.Services
{
    public interface IXDocumentService
    {
        XDocument Load(string filePath);
        void Save(XDocument document, string filePath);
    }

}
