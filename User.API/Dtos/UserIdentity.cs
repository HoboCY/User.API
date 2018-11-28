using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Dtos
{
    public class UserIdentity
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        public int UserId { get; set; }

        //以下信息包含在 token-claims 里
        public string Name { get; set; }

        public string Phone { get; set; }

        public string Company { get; set; }

        public string Title { get; set; }

        public string Avatar { get; set; }
    }
}
