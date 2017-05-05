using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;


namespace Nebula.Core.Elysium
{
    [ServiceContract(Name = "Elysium")]
    public interface IElysiumNodeWebService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/netquery?value={value}", ResponseFormat = WebMessageFormat.Xml)]
        List<string> NetQuery(string value);
    }
}
