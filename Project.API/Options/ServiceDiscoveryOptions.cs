using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.Options
{
    public class ServiceDiscoveryOptions
    {
        public string UserServiceName { get; set; }
        public string ServiceName { get; set; }

        public ConsulOptions Consul { get; set; }
    }
}
