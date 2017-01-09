using System.ServiceModel;

namespace Nebula.Shared
{
    public interface INebulaMasterServiceCB
    {
        [OperationContract]
        string Execute(string sBase64Assembly);
    }
}
