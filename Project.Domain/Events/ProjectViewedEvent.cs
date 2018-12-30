using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.Domain.Events
{
    /// <summary>
    /// 项目被查看事件
    /// </summary>
    public class ProjectViewedEvent : INotification
    {
        public string Company { get; set; }

        public string Introduction { get; set; }

        public string Avatar { get; set; }

        public ProjectViewer Viewer { get; set; }
    }
}
