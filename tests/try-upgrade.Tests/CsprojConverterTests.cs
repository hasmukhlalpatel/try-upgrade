using Moq;
using System.Xml.Linq;
using try_upgrade.Converters;
using try_upgrade.Services;

namespace try_upgrade.Tests
{
    public class CsprojConverterTests
    {
        private const string TestCsprojContent = @"
            <Project Sdk=""Microsoft.NET.Sdk"">
              <PropertyGroup>
                <TargetFramework>net6.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <Compile Include=""Program.cs"" />
                <Compile Include=""Class1.cs""  ExcludedFromBuild=""true"" />
                <Reference Include=""System.Data"" />
              </ItemGroup>
            </Project>
        ";

        [Fact]
        public void Convert_ShouldConvertCsprojCorrectly()
        {
            var csPRojFilePath = AppDomain.CurrentDomain.BaseDirectory.Split("\\tests\\")[0] + "\\Samples\\Console4x.App\\Console4x.App.csproj";
            var directoryPath = Path.GetDirectoryName(csPRojFilePath);
            var csFiles = Directory.EnumerateFiles(directoryPath, "*.cs").ToArray();
            var csProjFileText = File.ReadAllText(csPRojFilePath);
            //csProjFileText = TestCsprojContent;
            csProjFileText = csProjFileText.Replace("xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"", string.Empty);
            // Arrange
            var xDoc = XDocument.Parse(csProjFileText);
            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock.Setup(x=>x.GetFilesInDirectory(It.IsAny<string>(), It.IsAny<string>())).Returns(()=> csFiles);
            var xDocumentServiceMock = new Mock<IXDocumentService>();
            xDocumentServiceMock
                .Setup(l => l.Load(It.IsAny<string>()))
                .Returns(xDoc);

            var converter = new CsprojConverter("test.csproj", fileServiceMock.Object, xDocumentServiceMock.Object);

            // Act
            converter.Convert();

            // Assert
            // ... (assertions for converted CSPROJ content)
        }

        [Fact]
        public void Convert_ShouldIdentifyExcludedFiles()
        {
            // Arrange
            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock.Setup(f => f.FileExists(It.IsAny<string>())).Returns(true);
            var xDocumentServiceMock = new Mock<IXDocumentService>();
            xDocumentServiceMock.Setup(l => l.Load(It.IsAny<string>())).Returns(XDocument.Parse(TestCsprojContent));
            var converter = new CsprojConverter("test.csproj", fileServiceMock.Object, xDocumentServiceMock.Object);

            // Act
            converter.Convert();

            // Assert
            Assert.Contains("Class1.cs", converter.GetExcludedFiles());
        }

        [Fact]
        public void Convert_ShouldIdentifyExtraFiles()
        {
            // Arrange
            var fileServiceMock = new Mock<IFileService>();
            fileServiceMock.Setup(f => f.GetFilesInDirectory(It.IsAny<string>(), It.IsAny<string>())).Returns(new[] { "Program.cs", "Class1.cs", "ExtraClass.cs" });
            var xDocumentServiceMock = new Mock<IXDocumentService>();
            xDocumentServiceMock.Setup(l => l.Load(It.IsAny<string>())).Returns(XDocument.Parse(TestCsprojContent));
            var converter = new CsprojConverter("test.csproj", fileServiceMock.Object, xDocumentServiceMock.Object);

            // Act
            converter.Convert();

            // Assert
            Assert.Contains("ExtraClass.cs", converter.GetExtraFiles());
        }

        // ... Add more test cases for different scenarios
    }
}