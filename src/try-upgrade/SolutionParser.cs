using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace try_upgrade
{
    public class SolutionParser
    {
        public static List<string> ExtractCsprojFiles(string solutionFilePath)
        {
            var csprojFilePaths = new List<string>();

            try
            {
                XDocument xdoc = XDocument.Load(solutionFilePath);
                var projectNodes = xdoc.Descendants("Project");

                foreach (var projectNode in projectNodes)
                {
                    var projectFilePathAttribute = projectNode.Attribute("FilePath");
                    if (projectFilePathAttribute != null)
                    {
                        var projectFilePath = projectFilePathAttribute.Value;
                        if (projectFilePath.EndsWith(".csproj"))
                        {
                            csprojFilePaths.Add(projectFilePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing solution file: {ex.Message}");
            }

            return csprojFilePaths;
        }
    }
}
