using Nebula.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nebula.Elysium
{
    public class Node : Service<INode, INodeCallback>, INode, INodeCallback, IElysiumNodeWebService
    {
        private List<string> m_hTestData;

        public IPEndPoint LocalWebEndPoint { get; private set; }

        public Node() : base("NebulaNode")
        {
            m_hTestData = new List<string>();

        }


        [ConsoleUIMethod]
        public List<string> Query(string sKeywords)
        {
            var hKeywords = sKeywords.Split(new char[] { ' ' });


            return (from s in m_hTestData from k in hKeywords where s.Contains(k) select s).ToList();
        }


        [ConsoleUIMethod]
        public void Store(string sString) => m_hTestData.Add(sString);

        [ConsoleUIMethod]
        public IEnumerable<string> DumpData()
        {
            return m_hTestData;
        }


        [ConsoleUIMethod]
        public List<string> NetQuery(string value)
        {
            List<string> hResult = new List<string>();

            foreach (var item in this.Nodes)
            {
                List<string> hQueryResult = item.Query(value) as List<string>;

                hQueryResult.ForEach(s => hResult.Add(s));
            }

            hResult.AddRange(this.Query(value));

            return hResult;
        }

        [ConsoleUIMethod]
        public void Start(int iNetPort, int iWebPort)
        {
            LocalWebEndPoint = new IPEndPoint(IPAddress.Any, iWebPort);

            for (int i = 0; i < 10; i++)
            {
                m_hTestData.Add(RandomDataGenerator.CreateRecord(iNetPort));
            }

            base.Start(iNetPort);
        }

        //https://msdn.microsoft.com/en-us/library/hh205277(v=vs.110).aspx
        //netsh http add urlacl url=http://+:1001/Elysium user="NT AUTHORITY\NETWORK SERVICE"
        protected override void OnAddService()
        {
            base.OnAddService();
            //BasicHttpBinding hBasicHttpBinding  = new BasicHttpBinding(BasicHttpSecurityMode.None); //SOAP 1.1  => poor security, compatible
            //WSHttpBinding    hWsHttpBinding     = new WSHttpBinding(SecurityMode.None);             //SOAP      => full features
            WebHttpBinding hWebHttpBinding = new WebHttpBinding(WebHttpSecurityMode.None);       //Rest      => XML
            hWebHttpBinding.ReceiveTimeout = TimeSpan.MaxValue;
            hWebHttpBinding.SendTimeout = TimeSpan.MaxValue;
            ServiceEndpoint hEndpoint = m_hHost.AddServiceEndpoint(typeof(IElysiumNodeWebService), hWebHttpBinding, $"http://localhost:{LocalWebEndPoint.Port}/Elysium/");
            WebHttpBehavior hBehaviour = new WebHttpBehavior();

            hEndpoint.EndpointBehaviors.Add(hBehaviour);
        }

        public string Hello()
        {
            return "Hello";
        }

        public string HelloParameter(int value)
        {
            return "Hello " + value;
        }
    }

    static class RandomDataGenerator
    {
        private static List<string> m_hNames = new List<string>() { "Greco", "Barkhia", "Vieri", "Esposito", "Rossi" };
        private static List<string> m_hType  = new List<string>() { "Cartella Medica", "Analisi", "Certificato" };
        private static List<string> m_hYear  = Enumerable.Range(2000, 18).Select(x => x.ToString()).ToList();
        private static Random m_hRand = new Random();
        

        public static string CreateRecord(int iPort)
        {
            return $"{m_hType.RandomElement()} From node: {iPort}{Environment.NewLine}Last Name: {m_hNames.RandomElement()}{Environment.NewLine}Year: {m_hYear.RandomElement()}";            
        }


        private static T RandomElement<T>(this List<T> hThis)
        {
            return hThis[m_hRand.Next(0, hThis.Count)];
        }

    }

}
