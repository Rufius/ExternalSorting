using BenchmarkDotNet.Running;
using System.Reflection;

namespace ExternalSorting.Benchmark {
    internal class Program {
        static void Main(string[] args) {
            BenchmarkSwitcher.FromAssembly(Assembly.GetExecutingAssembly()).Run();
        }
    }
}
