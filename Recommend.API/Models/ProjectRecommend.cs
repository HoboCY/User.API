using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.Models
{
    public class ProjectRecommend
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int FromUserId { get; set; }

        public string FromUserName { get; set; }

        public string FromUserAvatar { get; set; }

        public EnumRecommendType RecommendType { get; set; }

        /// <summary>
        /// 项目Id
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// 项目Logo
        /// </summary>
        public string ProjectAvatar { get; set; }

        /// <summary>
        /// 公司
        /// </summary>
        public string Company { get; set; }

        /// <summary>
        /// 项目简介
        /// </summary>
        public string Introduction { get; set; }

        /// <summary>
        /// 项目标签
        /// </summary>
        public string Tags { get; set; }

        /// <summary>
        /// 融资阶段
        /// </summary>
        public string FinStage { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime RecommendTime { get; set; }
    }
}
