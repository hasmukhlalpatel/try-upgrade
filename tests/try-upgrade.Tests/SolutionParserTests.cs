namespace try_upgrade.Tests
{
    public class SolutionParserTests
    {
        [Fact]
        public void ExtractCsprojFiles_ShouldExtractCorrectCsprojPaths()
        {
            // Arrange
            string solutionContent = @"
                Microsoft Visual Studio Solution File, Format Version 12.00
                # Visual Studio 15
                Project(""E3E38632-312D-454C-912E-DB664B2AE92F"") = ""Project1"", ""Project1.csproj"", ""{FAE04EC0-301B-11D3-BF4B-00C04F79EFBC}""
                EndProject
                Global
                GlobalSection(SolutionConfigurationPlatforms) = preSolution
                EndGlobalSection
                GlobalSection(ProjectConfigurationPlatforms) = postSolution
                EndGlobalSection   

                EndGlobal
            ";

            // Act
            List<string> csprojFilePaths = SolutionParser.ExtractCsprojFiles(solutionContent);

            // Assert
            Assert.Single(csprojFilePaths);
            Assert.Equal("Project1.csproj", csprojFilePaths[0]);
        }

        [Fact]
        public void ExtractCsprojFiles_ShouldHandleInvalidSolutionContent()
        {
            // Arrange
            string invalidSolutionContent = "Invalid solution content";

            // Act
            List<string> csprojFilePaths = SolutionParser.ExtractCsprojFiles(invalidSolutionContent);

            // Assert
            Assert.Empty(csprojFilePaths);
        }

        [Fact]
        public void ExtractCsprojFiles_ShouldHandleEmptySolutionContent()
        {
            // Arrange
            string emptySolutionContent = "";

            // Act
            List<string> csprojFilePaths = SolutionParser.ExtractCsprojFiles(emptySolutionContent);

            // Assert
            Assert.Empty(csprojFilePaths);
        }

        [Fact]
        public void ExtractCsprojFiles_ShouldHandleMultipleCsprojFiles()
        {
            // Arrange
            string solutionContent = @"
                Microsoft Visual Studio Solution File, Format Version 12.00
                # Visual Studio 15
                Project(""E3E38632-312D-454C-912E-DB664B2AE92F"") = ""Project1"", ""Project1.csproj"", ""{FAE04EC0-301B-11D3-BF4B-00C04F79EFBC}""
                EndProject
                Project(""E3E38632-312D-454C-912E-DB664B2AE92F"") = ""Project2"", ""Project2.csproj"", ""{FAE04EC0-301B-11D3-BF4B-00C04F79EFBC}""
                EndProject
                Global
                GlobalSection(SolutionConfigurationPlatforms) = preSolution
                EndGlobalSection
                GlobalSection(ProjectConfigurationPlatforms) = postSolution
                EndGlobalSection   

                EndGlobal
            ";

            // Act
            List<string> csprojFilePaths = SolutionParser.ExtractCsprojFiles(solutionContent);

            // Assert
            Assert.Equal(2, csprojFilePaths.Count);
            Assert.Contains("Project1.csproj", csprojFilePaths);
            Assert.Contains("Project2.csproj", csprojFilePaths);
        }
    }
}