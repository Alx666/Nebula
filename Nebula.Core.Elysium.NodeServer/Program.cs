using Nebula.Core;
using Nebula.Core.Elysium;

namespace Nebula.Core.Elysium.NodeServer
{
    class Program
    {
        static void Main(string[] args) => new ConsoleUI(new Node(), "Nebula Node").RunAndWait();
    }
}
