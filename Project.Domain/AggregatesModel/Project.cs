using Project.Domain.Events;
using Project.Domain.SeedWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Project.Domain.AggregatesModel
{
    public class Project : Entity, IAggregateRoot
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 项目logo
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 公司名称
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 源BP文件地址
        /// </summary>
        public string OriginBPFile { get; set; }

        /// <summary>
        /// 转换后的BP文件地址
        /// </summary>
        public string FormatBPFile { get; set; }

        /// <summary>
        /// 是否显示敏感信息
        /// </summary>
        public bool ShowSecurityInfo { get; set; }

        /// <summary>
        /// 公司所在省Id
        /// </summary>
        public int ProvinceId { get; set; }

        /// <summary>
        /// 公司所在省名称
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 公司所在城市Id
        /// </summary>
        public int CityId { get; set; }

        /// <summary>
        /// 公司所在城市名称
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 区域Id
        /// </summary>
        public int AreaId { get; set; }

        /// <summary>
        /// 区域名称
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 公司成立时间
        /// </summary>
        public DateTime RegisterTime { get; set; }

        /// <summary>
        /// 项目基本信息
        /// </summary>
        public string Introduction { get; set; }

        /// <summary>
        /// 出让股份比例
        /// </summary>
        public string FinPercentage { get; set; }

        /// <summary>
        /// 融资阶段
        /// </summary>
        public string FinStage { get; set; }

        /// <summary>
        /// 融资金额 单位(万)
        /// </summary>
        public int FinMoney { get; set; }

        /// <summary>
        /// 收入 单位(万)
        /// </summary>
        public int Income { get; set; }

        /// <summary>
        /// 利润 单位(万)
        /// </summary>
        public int Revenue { get; set; }

        /// <summary>
        /// 估值 单位(万)
        /// </summary>
        public int Valuation { get; set; }

        /// <summary>
        /// 佣金分配方式
        /// </summary>
        public int BrokerageOptions { get; set; }

        /// <summary>
        /// 是否委托给finbook
        /// </summary>
        public bool OnPlatfrom { get; set; }

        /// <summary>
        /// 可见范围设置
        /// </summary>
        public ProjectVisibleRule VisibleRule { get; set; }

        /// <summary>
        /// 根引用项目Id
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>
        /// 上级引用项目Id
        /// </summary>
        public int ReferenceId { get; set; }

        /// <summary>
        /// 项目标签
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 项目属性：行业领域、融资币种
        /// </summary>
        public List<ProjectProperty> Properties { get; set; }

        /// <summary>
        /// 贡献者列表
        /// </summary>
        public List<ProjectContributor> Contributors { get; set; }

        /// <summary>
        /// 查看者
        /// </summary>
        public List<ProjectViewer> Viewers { get; set; }

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }

        public DateTime CreatedTime { get; private set; }

        /// <summary>
        /// 复制项目
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private Project CloneProject(Project source = null)
        {
            if (source == null)
            {
                source = this;
            }

            var newProject = new Project
            {
                UserId = source.UserId,
                Avatar = source.Avatar,
                Company = source.Company,
                OriginBPFile = source.OriginBPFile,
                FormatBPFile = source.FormatBPFile,
                ShowSecurityInfo = source.ShowSecurityInfo,
                ProvinceId = source.ProvinceId,
                Province = source.Province,
                CityId = source.CityId,
                City = source.City,
                AreaId = source.AreaId,
                Area = source.Area,
                RegisterTime = source.RegisterTime,
                Introduction = source.Introduction,
                FinMoney = source.FinMoney,
                FinPercentage = source.FinPercentage,
                FinStage = source.FinStage,
                Income = source.Income,
                Revenue = source.Revenue,
                Valuation = source.Valuation,
                BrokerageOptions = source.BrokerageOptions,
                OnPlatfrom = source.OnPlatfrom,
                VisibleRule = source.VisibleRule == null ? null : new ProjectVisibleRule
                {
                    Visible = source.VisibleRule.Visible,
                    Tags = source.VisibleRule.Tags
                },
                SourceId = source.SourceId,
                ReferenceId = source.ReferenceId,
                Tags = source.Tags,
                Properties = new List<ProjectProperty> { },
                Contributors = new List<ProjectContributor> { },
                Viewers = new List<ProjectViewer> { },
                UpdateTime = source.UpdateTime,
                CreatedTime = DateTime.Now
            };
            newProject.Properties = new List<ProjectProperty> { };
            foreach (var item in source.Properties)
            {
                newProject.Properties.Add(new ProjectProperty
                (
                    item.Key,
                    item.Text,
                    item.Value
                ));
            }
            return newProject;
        }

        /// <summary>
        /// 参与者得到项目拷贝
        /// </summary>
        /// <param name="contributorId"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public Project ContributorFork(int contributorId, Project source = null)
        {
            if (source == null)
                source = this;
            var newProject = CloneProject(source);

            newProject.UserId = contributorId;
            newProject.SourceId = source.SourceId == 0 ? source.Id : source.SourceId;
            newProject.ReferenceId = source.ReferenceId == 0 ? source.Id : source.ReferenceId;
            newProject.UpdateTime = DateTime.Now;
            return newProject;
        }

        public Project()
        {
            Viewers = new List<ProjectViewer>();
            Contributors = new List<ProjectContributor>();
            AddDomainEvent(new ProjectCreatedEvent { Project = this });
        }

        /// <summary>
        /// 添加查看者
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userName"></param>
        /// <param name="avatar"></param>
        public void AddViewer(int userId, string userName, string avatar)
        {
            var viewer = new ProjectViewer
            {
                UserId = userId,
                UserName = userName,
                Avatar = avatar,
                CreatedTime = DateTime.Now
            };

            if (!Viewers.Any(v => v.UserId == UserId))
            {
                Viewers.Add(viewer);

                AddDomainEvent(new ProjectViewedEvent
                {
                    Company = Company,
                    Introduction = Introduction,
                    Avatar = Avatar,
                    Viewer = viewer
                });
            }
        }

        public void AddContributor(ProjectContributor contributor)
        {
            if (!Contributors.Any(v => v.UserId == UserId))
            {
                Contributors.Add(contributor);
                AddDomainEvent(new ProjectJoinedEvent
                {
                    Company = Company,
                    Introduction = Introduction,
                    Avatar = Avatar,
                    Contributor = contributor
                });
            }
        }

    }
}