using System;

namespace Nebula.Core.Elysium.MasterServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Master hNode = new Master();
                string sTitle = "Elysium Master Server";

                if (args.Length == 1)
                {
                    hNode.Start(int.Parse(args[0]));
                    sTitle += $" Net: {args[0]}";
                }
                else if (args.Length == 0)
                {
                    //Manually start
                }
                else
                {
                    throw new Exception("Parameters must be in form:\n <binary port> <web port> <master server address> <master server port>");
                }

                hNode.ChannelClosed += OnChannelClosed;
                hNode.ChannelFaulted += OnChannelClosed;

                ConsoleUI hConsole = new ConsoleUI(hNode, sTitle);
                hConsole.RunAndWait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void OnChannelClosed(Master.NodeReference obj)
        {
            Console.WriteLine("Peer Disconnected");
        }
    }
}
