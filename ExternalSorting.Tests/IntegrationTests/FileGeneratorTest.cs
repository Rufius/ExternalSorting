using ExternalSorting.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalSorting.Tests.IntegrationTests {
    internal class FileGeneratorTest {
        [Test]
        public async Task Generate() {
            var generator = new FileGenerator(99999);
            await generator.Generate("test.txt", 1024);

        }
    }
}
