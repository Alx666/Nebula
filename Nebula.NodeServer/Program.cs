using Nebula.Core;

namespace Nebula.NodeServer
{
    class Program
    {
        static void Main(string[] args) => new ConsoleUI(new Node(), "Nebula Node").RunAndWait();
    }
}
