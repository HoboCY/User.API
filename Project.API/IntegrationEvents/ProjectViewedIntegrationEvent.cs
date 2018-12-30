using Project.Domain.AggregatesModel;

namespace Project.API.IntegrationEvents
{
    public class ProjectViewedIntegrationEvent
    {
        public string Company { get; set; }

        public string Introduction { get; set; }

        public string Avatar { get; set; }
        public ProjectViewer Viewer { get; set; }
    }
}
