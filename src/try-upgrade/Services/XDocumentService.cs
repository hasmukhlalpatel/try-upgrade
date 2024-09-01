using System.Xml.Linq;

namespace try_upgrade.Services
{

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

}
