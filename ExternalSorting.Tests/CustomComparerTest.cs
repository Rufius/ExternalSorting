namespace ExternalSorting.Tests {
    public class CustomComparerTest {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void DifferentTextAndDifferentNumbers() {
            const string line1 = "30432. Something something something";
            const string line2 = "415. Apple";

            var list = new List<string>();
            list.Add(line1);
            list.Add(line2);

            list.Sort(new CustomComparer());

            Assert.That(list[0], Is.EqualTo(line2));
            Assert.That(list[1], Is.EqualTo(line1));
        }

        [Test]
        public void SameTextAndDifferentNumbers() {
            const string line1 = "30432. Something something something";
            const string line2 = "415. Something something something";

            var list = new List<string>();
            list.Add(line1);
            list.Add(line2);

            list.Sort(new CustomComparer());

            Assert.That(list[0], Is.EqualTo(line2));
            Assert.That(list[1], Is.EqualTo(line1));
        }

        [Test]
        public void DifferentTextAndSameNumbers() {
            const string line1 = "415. Something something something";
            const string line2 = "415. Apple";

            var list = new List<string>();
            list.Add(line1);
            list.Add(line2);

            list.Sort(new CustomComparer());

            Assert.That(list[0], Is.EqualTo(line2));
            Assert.That(list[1], Is.EqualTo(line1));
        }
    }
}