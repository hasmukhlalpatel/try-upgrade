using try_upgrade.Converters;

namespace try_upgrade.Tests
{
    public class CsprojConverterTests
    {
        [Fact]
        public void Test1()
        {
            CsprojConverter converter = new CsprojConverter(".\test.csproj");

        }
    }
}