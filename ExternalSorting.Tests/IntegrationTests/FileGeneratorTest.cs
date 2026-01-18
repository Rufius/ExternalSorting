using ExternalSorting.Generator;

namespace ExternalSorting.Tests.IntegrationTests {
    internal class FileGeneratorTest {
        [Test]
        public async Task Generate() {
            var lineGenerator = new RandomLineGenerator();
            var duplicateTextLineProcessor = new DuplicateTextLineProcessor(4, 100, lineGenerator);
            var generator = new FileGenerator(99999, duplicateTextLineProcessor, null);
            await generator.GenerateAsync("test.txt", 1024*1024*1);
        }
    }
}
