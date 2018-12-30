using Project.Domain.AggregatesModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Project.API.IntegrationEvents
{
    public class ProjectJoinedIntegrationEvent
    {
        public string Company { get; set; }

        public string Introduction { get; set; }

        public string Avatar { get; set; }

        public ProjectContributor Contributor { get; set; }
    }
}
