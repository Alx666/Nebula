using Nebula.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nebula.Elysium
{
    public class Node : Service<INode, INodeCallback>, INode, INodeCallback
    {
        private List<string> m_hTestData;

        public Node() : base("NebulaNode")
        {
            m_hTestData = new List<string>();
        }


        [ConsoleUIMethod]
        public List<string> Query(string sKeywords) => (from s in m_hTestData from k in sKeywords.Split(new char[] { ' ' }) where s.Contains(k) select s).ToList();


        [ConsoleUIMethod]
        public void Store(string sString)           => m_hTestData.Add(sString);


        [ConsoleUIMethod]
        public IEnumerable<string> NetQuery(string sKeywords)
        {
            List<string> hResult = new List<string>();

            foreach (var item in this.Nodes)
            {
                List<string> hQueryResult = item.Query(sKeywords) as List<string>;

                hQueryResult.ForEach(s => hResult.Add(s));
            }

            
            return hResult;
        }

        [ConsoleUIMethod]
        public void Start(int iNetPort, int iWebPort)
        {
            WSHttpBinding hHttpBinding = new WSHttpBinding(SecurityMode.None, true);
            m_hHost.AddServiceEndpoint(typeof(INode), hHttpBinding, $"http://localhost:{iWebPort}/Elysium/");
            
            base.Start(iNetPort);
        }
    }

}
