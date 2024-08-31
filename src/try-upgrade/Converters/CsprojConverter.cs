using System.Xml.Linq;

namespace try_upgrade.Converters
{
    public class CsprojConverter
    {
        private readonly string _csprojPath;
        private readonly List<string> _excludedFiles = new();
        private List<string> _extraFiles = new();

        public CsprojConverter(string csprojPath)
        {
            _csprojPath = csprojPath;
        }

        public void Convert()
        {
            var xdoc = XDocument.Load(_csprojPath);
            var root = xdoc.Root;

            // Remove unnecessary elements from <PropertyGroup>
            var propertyGroups = root.Descendants("PropertyGroup");
            foreach (var propertyGroup in propertyGroups)
            {
                propertyGroup.Descendants("TargetFramework").Remove();
                propertyGroup.Descendants("OutputType").Remove();
                propertyGroup.Descendants("AssemblyName").Remove();
                propertyGroup.Descendants("RootNamespace").Remove();
            }

            // Replace <Reference> with <PackageReference>
            var references = root.Descendants("Reference");
            foreach (var reference in references)
            {
                var packageReference = new XElement("PackageReference");
                packageReference.SetAttributeValue("Include", reference.Attribute("Include").Value);
                packageReference.SetAttributeValue("Version", "*");
                reference.ReplaceWith(packageReference);
            }

            // Remove unnecessary elements from <Compile>
            var compileElements = root.Descendants("Compile");
            foreach (var compileElement in compileElements)
            {
                compileElement.Descendants("SubType").Remove();
            }

            // Check for excluded files and remove only if file exists
            foreach (var compileElement in compileElements)
            {
                var excludedAttribute = compileElement.Attribute("ExcludedFromBuild");
                if (excludedAttribute != null && excludedAttribute.Value == "true")
                {
                    var filePath = Path.Combine(Path.GetDirectoryName(_csprojPath), compileElement.Attribute("Include").Value);
                    if (File.Exists(filePath))
                    {
                        _excludedFiles.Add(compileElement.Attribute("Include").Value);
                        compileElement.Remove();
                    }
                }
            }

            // Get all .cs files in the project directory
            var projectFiles = GetFilesInDirectory(Path.GetDirectoryName(_csprojPath));

            // Compare with files included in <Compile> tags
            var includedFiles = root.Descendants("Compile")
                                  .Select(c => c.Attribute("Include").Value)
                                  .ToList();

            // Find extra files
            _extraFiles = projectFiles.Except(includedFiles).ToList();

            // Save the modified file
            xdoc.Save(_csprojPath);
        }

        private IEnumerable<string> GetFilesInDirectory(string directoryPath)
        {
            return Directory.EnumerateFiles(directoryPath, "*.cs", SearchOption.AllDirectories);
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
