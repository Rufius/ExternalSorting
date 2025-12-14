using ExternalSorting.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
