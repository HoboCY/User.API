using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.Identity.Services
{
   public interface IUserService
    {
        /// <summary>
        /// 检查手机号是否已经注册，否则创建
        /// </summary>
        /// <param name="phone"></param>
        Task<Dtos.UserInfo> CheckOrCreate(string phone);
    }
}
