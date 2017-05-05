namespace Nebula.Core.Elysium.MasterServer
{
    class Program
    {
        static void Main(string[] args) => new ConsoleUI(new Master(), "Master Server").RunAndWait();
    }
}
