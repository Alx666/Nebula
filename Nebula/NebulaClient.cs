using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Nebula.Shared
{
    public class NebulaClient
    {
        public int Id                           { get; set; }

        public string Machine                   { get; set; }

        public IPEndPoint Address               { get; set; }

        public INebulaMasterServiceCB Callback  { get; set; }

        public List<NebulaModuleInfo> Modules   { get; set; }
    }
}
