using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Dtos
{
    public class BaseUserInfo
    {
        public BaseUserInfo()
        {
            Tags = new string[] { };
        }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string Phone { get; set; }
        public string Avatar { get; set; }
        public string[] Tags { get; set; }
    }
}
