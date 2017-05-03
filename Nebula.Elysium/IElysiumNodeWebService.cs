using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;


namespace Nebula.Elysium
{
    [ServiceContract(Name = "Elysium")]
    public interface IElysiumNodeWebService
    {
        [OperationContract]
        [WebGet(UriTemplate = "/hello", ResponseFormat = WebMessageFormat.Xml)]
        string Hello();

        [OperationContract]
        [WebGet(UriTemplate = "/helloparameter?value={value}", ResponseFormat = WebMessageFormat.Xml)]
        string HelloParameter(int value);

        [OperationContract]
        [WebGet(UriTemplate = "/netquery?value={value}", ResponseFormat = WebMessageFormat.Xml)]
        List<string> NetQuery(string value);
    }
}
