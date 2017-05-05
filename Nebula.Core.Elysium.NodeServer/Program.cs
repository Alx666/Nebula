namespace Nebula.Core.Elysium.NodeServer
{
    class Program
    {
        static void Main(string[] args) => new ConsoleUI(new Node(args), "Nebula Node").RunAndWait();
    }
}
