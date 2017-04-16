using Nebula.Core;

namespace Nebula.MasterServer
{
    class Program
    {
        static void Main(string[] args) => new ConsoleUI(new Master(), "Master Server").RunAndWait();
    }
}
