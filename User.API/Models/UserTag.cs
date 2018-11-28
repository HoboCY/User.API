using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Models
{
    public class UserTag
    {
        public int UserId { get; set; }

        public string Tag { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
