using System.Xml.Linq;
using try_upgrade.Services;
//https://gemini.google.com/app/5db6fc21d05b4f83
namespace try_upgrade.Converters
{
    public class CsprojConverter
    {
        private readonly string _csprojPath;
        private readonly IFileService _fileService;
        private readonly IXDocumentService _xDocumentService;
        private readonly List<string> _excludedFiles = new();
        private List<string> _extraFiles = new();

        public CsprojConverter(string csprojPath, IFileService fileService, IXDocumentService xDocumentService)
        {
            _csprojPath = csprojPath;
            _fileService = fileService;
            _xDocumentService = xDocumentService;
        }

        public void Convert()
        {
            var xdoc = _xDocumentService.Load(_csprojPath);
            var directoryPath = _fileService.GetDirectoryPath(_csprojPath);

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
                    var filePath = compileElement.Attribute(CsprojConstants.IncludeAttribute).Value;
                    if (_fileService.FileExists(directoryPath, filePath))
                    {
                        _excludedFiles.Add(compileElement.Attribute(CsprojConstants.IncludeAttribute).Value);
                        compileElement.Remove();
                    }
                }
            }

            // Get all .cs files in the project directory
            var projectFiles = _fileService.GetFilesInDirectory(directoryPath, CsprojConstants.CsFileExtension);

            // Compare with files included in <Compile> tags
            var includedFiles = root.Descendants(CsprojConstants.CompileElementName)
                                  .Select(c => c.Attribute(CsprojConstants.IncludeAttribute).Value)
                                  .ToList();

            // Find extra files
            _extraFiles = projectFiles.Except(includedFiles).ToList();

            _xDocumentService.Save(xdoc, _csprojPath);
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
}
