using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.Models
{
    public enum EnumRecommendType : int
    {
        Plaform = 1,   //平台
        Friend = 2,    //好友
        Foaf = 3       //二度好友
    }
}
