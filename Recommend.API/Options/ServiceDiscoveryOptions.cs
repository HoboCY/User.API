using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.Options
{
    public class ServiceDiscoveryOptions
    {
        public string UserServiceName { get; set; }

        public string ContactServiceName { get; set; }

        public ConsulOptions Consul { get; set; }
    }
}
