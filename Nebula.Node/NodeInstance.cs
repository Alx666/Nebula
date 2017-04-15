using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Nebula.Core;

namespace Nebula.Node
{
    internal class NodeInstance
    {
        private Nebula.Core.Node m_hNode;

        public NodeInstance()
        {
            m_hNode = new Core.Node();
            m_hNode.NodeConnected   += OnNodeConnected;
            m_hNode.NodeFaulted     += OnNodeDisconnected;
        }

        private void OnNodeDisconnected(IPEndPoint obj)
        {
            Console.WriteLine("Node Disconnected " + obj.ToString());
        }

        private void OnNodeConnected(IPEndPoint obj)
        {
            Console.WriteLine("Node Connected " + obj.ToString());
        }

        [ConsoleUIMethod]
        public void Start(int iPort)
        {
            m_hNode.Start(iPort);
        }


        [ConsoleUIMethod]
        public IEnumerable<IPEndPoint> Connect(int iPort)
        {
            return m_hNode.Connect("127.0.0.1", iPort);
        }

        [ConsoleUIMethod]
        public IEnumerable<IPEndPoint> EnumerateNodes()
        {
            return m_hNode.EnumerateNodes();
        }


    }
}
