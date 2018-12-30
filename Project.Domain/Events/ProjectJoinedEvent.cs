using MediatR;
using Project.Domain.AggregatesModel;

namespace Project.Domain.Events
{
    /// <summary>
    /// 项目被加入事件
    /// </summary>
    public class ProjectJoinedEvent : INotification
    {
        public string Company { get; set; }

        public string Introduction { get; set; }

        public string Avatar { get; set; }
        public ProjectContributor Contributor { get; set; }
    }
}
