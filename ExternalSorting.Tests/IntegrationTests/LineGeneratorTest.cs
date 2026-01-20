using ExternalSorting.Generator;

namespace ExternalSorting.Tests.IntegrationTests {
    internal class LineGeneratorTest {
        [Test]
        public async Task Generate() {
            var lineGenerator = new ApiLineGenerator();
            var line = await lineGenerator.GenerateAsync();

            Assert.IsNotNull(line);
            Assert.IsNotEmpty(line);
        }
    }
}
