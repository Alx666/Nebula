using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Nebula.Core
{
    static class OperationContextExtensions
    {
        public static IPEndPoint GetRemoteEndPoint(this OperationContext hContext)
        {
            RemoteEndpointMessageProperty hEndPointProp = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;

            return new IPEndPoint(IPAddress.Parse(hEndPointProp.Address), hEndPointProp.Port);
        }
    }
}
