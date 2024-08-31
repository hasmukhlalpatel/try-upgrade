using System.IO;
using System.Xml.Linq;
//https://gemini.google.com/app/5db6fc21d05b4f83
namespace try_upgrade.Converters
{
    public interface IXDocumentLoader
    {
        XDocument Load(string filePath);
    }

    public interface IXDocumentSaver
    {
        void Save(XDocument document, string filePath);
    }

    public class FileSystemDocumentLoader : IXDocumentLoader
    {
        public XDocument Load(string filePath)
        {
            return XDocument.Load(filePath);
        }
    }

    public class FileSystemDocumentSaver : IXDocumentSaver
    {
        public void Save(XDocument document, string filePath)
        {
            document.Save(filePath);
        }
    }
    public interface IFileService
    {
        IEnumerable<string> GetFilesInDirectory(string directoryPath, string fileExtension);
        bool FileExists(string filePath);
    }

    public class FileSystemFileService : IFileService
    {
        public IEnumerable<string> GetFilesInDirectory(string directoryPath, string fileExtension)
        {
            return Directory.EnumerateFiles(directoryPath, fileExtension, SearchOption.AllDirectories);
        }

        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
    }

    public class CsprojConverter
    {
        private readonly string _csprojPath;
        private readonly IFileService _fileService;
        private readonly IXDocumentLoader _loader;
        private readonly IXDocumentSaver _saver;
        private readonly List<string> _excludedFiles = new();
        private readonly List<string> _extraFiles = new();

        public CsprojConverter(string csprojPath, IFileService fileService, IXDocumentLoader loader, IXDocumentSaver saver)
        {
            _csprojPath = csprojPath;
            _fileService = fileService;
            _loader = loader;
            _saver = saver;
        }

        public void Convert()
        {
            var xdoc = _loader.Load(_csprojPath);
            var root = xdoc.Root;

            // Remove unnecessary elements from <PropertyGroup>
            var propertyGroups = root.Descendants("PropertyGroup");
            foreach (var propertyGroup in propertyGroups)
            {
                propertyGroup.Descendants(CsprojConstants.TargetFrameworkElementName).Remove();
                propertyGroup.Descendants(CsprojConstants.OutputTypeElementName).Remove();
                propertyGroup.Descendants(CsprojConstants.AssemblyNameElementName).Remove();
                propertyGroup.Descendants(CsprojConstants.RootNamespaceElementName).Remove();
            }

            // Replace <Reference> with <PackageReference>
            var references = root.Descendants(CsprojConstants.ReferenceElementName);
            foreach (var reference in references)
            {
                var packageReference = new XElement(CsprojConstants.PackageReferenceElementName);
                packageReference.SetAttributeValue(CsprojConstants.IncludeAttribute, reference.Attribute(CsprojConstants.IncludeAttribute).Value);
                packageReference.SetAttributeValue(CsprojConstants.VersionAttribute, "*");
                reference.ReplaceWith(packageReference);
            }

            // Remove unnecessary elements from <Compile>
            var compileElements = root.Descendants(CsprojConstants.CompileElementName);
            foreach (var compileElement in compileElements)
            {
                compileElement.Descendants(CsprojConstants.SubTypeElementName).Remove();
            }

            // Check for excluded files and remove only if file exists
            foreach (var compileElement in compileElements)
            {
                var excludedAttribute = compileElement.Attribute(CsprojConstants.ExcludedFromBuildAttribute);
                if (excludedAttribute != null && excludedAttribute.Value == CsprojConstants.TrueValue)
                {
                    var filePath = _fileService.Combine(_fileService.GetDirectoryName(_csprojPath), compileElement.Attribute(CsprojConstants.IncludeAttribute).Value);
                    if (_fileService.FileExists(filePath))
                    {
                        _excludedFiles.Add(compileElement.Attribute(CsprojConstants.IncludeAttribute).Value);
                        compileElement.Remove();
                    }
                }
            }

            // Get all .cs files in the project directory
            var projectFiles = _fileService.GetFilesInDirectory(_fileService.GetDirectoryName(_csprojPath), CsprojConstants.CsFileExtension);

            // Compare with files included in <Compile> tags
            var includedFiles = root.Descendants(CsprojConstants.CompileElementName)
                                  .Select(c => c.Attribute(CsprojConstants.IncludeAttribute).Value)
                                  .ToList();

            // Find extra files
            _extraFiles = projectFiles.Except(includedFiles).ToList();

            _saver.Save(xdoc, _csprojPath);
        }

        public IReadOnlyList<string> GetExcludedFiles()
        {
            return _excludedFiles.AsReadOnly();
        }

        public IReadOnlyList<string> GetExtraFiles()
        {
            return _extraFiles.AsReadOnly();
        }
    }

    public static class CsprojConstants
    {
        public const string TargetFrameworkElementName = "TargetFramework";
        public const string OutputTypeElementName = "OutputType";
        public const string AssemblyNameElementName = "AssemblyName";
        public const string RootNamespaceElementName = "RootNamespace";
        public const string ReferenceElementName = "Reference";
        public const string PackageReferenceElementName = "PackageReference";
        public const string IncludeAttribute = "Include";
        public const string VersionAttribute = "Version";
        public const string SubTypeElementName = "SubType";
        public const string ExcludedFromBuildAttribute = "ExcludedFromBuild";
        public const string ComponentValue = "Component";
        public const string TrueValue = "true";
        public const string CsFileExtension = "*.cs";
        public const string CompileElementName = "Compile";
    }
}
