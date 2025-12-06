namespace ExternalSorting
{
    internal class Program
    {
        static void Main(string[] args) {
            Sort("sample.txt");
        }

        static void Sort(string filePath) {
            var lines = File.ReadLines(filePath).ToList();
            
            lines.Sort();

            lines.ForEach((line) => Console.WriteLine(line));
        }
    }
}
