using ExternalSorting.Generator;

namespace ExternalSorting.Tests {
    internal class RandomLineGeneratorTest {
        [Test]
        public async Task Generate() {
            var lineGenerator = new RandomLineGenerator();
            var line = await lineGenerator.GenerateAsync();
            
            Assert.IsNotNull(line);
            Assert.IsNotEmpty(line);
        }
    }
}
