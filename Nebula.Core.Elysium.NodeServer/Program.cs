using System;

namespace Nebula.Core.Elysium.NodeServer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Node hNode = new Node();
                string sTitle = "Nebula Node";

                if (args.Length == 4)
                {
                    hNode.Start(int.Parse(args[0]), int.Parse(args[1]), args[2], int.Parse(args[3]));
                    sTitle += $" Net: {args[0]} Rest: {args[1]} MsAddr: {args[2]} MsPort: {args[3]}";
                }
                else if (args.Length == 2)
                {
                    hNode.Start(int.Parse(args[0]), int.Parse(args[1]));
                    sTitle += $"Net:{args[0]} Rest:{args[1]}";
                }
                else if (args.Length == 1)
                {
                    hNode.Start(int.Parse(args[0]));
                    sTitle += $"Net:{args[0]}";
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
                hNode.ChannelFaulted += OnChannelFaulted;

                ConsoleUI hConsole   = new ConsoleUI(hNode, sTitle);
                hConsole.RunAndWait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void OnChannelClosed(IBaseService obj)
        {
            Console.WriteLine("Channel Closed");
        }

        private static void OnChannelFaulted(IBaseService obj)
        {
            Console.WriteLine("Channel Faulted");
        }
    }
}
