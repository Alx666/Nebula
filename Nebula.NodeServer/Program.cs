using Nebula.Core;
using Nebula.Elysium;

namespace Nebula.NodeServer
{
    class Program
    {
        static void Main(string[] args) => new ConsoleUI(new Node(), "Nebula Node").RunAndWait();
    }
}
