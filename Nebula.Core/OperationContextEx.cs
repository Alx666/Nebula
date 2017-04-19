using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Core
{
    internal class OperationContextEx : IExtension<OperationContext>
    {

        public static OperationContextEx Current
        {
            get
            {
                OperationContextEx context = OperationContext.Current.Extensions.Find<OperationContextEx>();

                if (context == null)
                {
                    context = new OperationContextEx();
                    OperationContext.Current.Extensions.Add(context);
                }

                return context;
            }
        }

        public T GetCallbackChannel<T>() => OperationContext.Current.GetCallbackChannel<T>();

        public IPEndPoint ListenerEndPoint { get; set; }

        public IPEndPoint RemoteEndPoint
        {
            get
            {
                RemoteEndpointMessageProperty hEndPointProp = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                return new IPEndPoint(IPAddress.Parse(hEndPointProp.Address), hEndPointProp.Port);
            }
        }

        public void Attach(OperationContext owner)
        {
        }

        public void Detach(OperationContext owner)
        {
        }
    }
}
