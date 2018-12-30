using Recommend.API.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recommend.API.Services
{
    public interface IContactService
    {
        /// <summary>
        /// 获取好友列表
        /// </summary>
        /// <param name="userId">用户Id</param>
        /// <returns></returns>
        Task<List<Contact>> GetContactsByUserIdAsync(int userId);
    }
}
