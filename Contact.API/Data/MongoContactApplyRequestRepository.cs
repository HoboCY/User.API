using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contact.API.Models;
using MongoDB.Driver;
using System.Linq;
using System.Threading;

namespace Contact.API.Data
{
    public class MongoContactApplyRequestRepository : IContactApplyRequestRepository
    {
        private readonly ContactContext _contactContext;
        public MongoContactApplyRequestRepository(ContactContext contactContext)
        {
            _contactContext = contactContext;
        }

        public async Task<bool> AddRequestAsync(ContactApplyRequest request, CancellationToken cancellationToken)
        {
            //如果有请求记录就修改申请时间，否则就新增一条请求记录
            var filter = Builders<ContactApplyRequest>.Filter.Where(r => r.UserId == request.UserId
            && r.ApplierId == request.ApplierId);
            if ((await _contactContext.ContactApplyRequests.CountDocumentsAsync(filter)) > 0)
            {
                var update = Builders<ContactApplyRequest>.Update.Set(r => r.ApplyTime, DateTime.Now);
                var result = await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);
                return result.MatchedCount == result.ModifiedCount && result.MatchedCount == 1;
            }
            await _contactContext.ContactApplyRequests.InsertOneAsync(request, null, cancellationToken);
            return true;
        }

        /// <summary>
        /// 通过好友请求
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="applierId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<bool> ApprovalAsync(int userId, int applierId, CancellationToken cancellationToken)
        {
            //根据当前用户id匹配，没有匹配项则用当前用户id新增一条，有则通过好友请求，更新处理时间
            if (!(await _contactContext.ContactBooks.FindSync(c => c.UserId == userId).ToListAsync()).Any())
            {
                await _contactContext.ContactBooks.InsertOneAsync(new ContactBook { UserId = userId });
            }

            var result = await _contactContext.ContactApplyRequests.UpdateOneAsync(c => c.UserId == userId,
                Builders<ContactApplyRequest>.Update.Set(c => c.Approvaled, 1).Set(c => c.HandledTime, DateTime.Now));
            return result.MatchedCount == result.ModifiedCount && result.ModifiedCount == 1;
            //if ((await _contactContext.ContactBooks.CountDocumentsAsync(c => c.UserId == userId)) == 0)
            //{
            //    await _contactContext.ContactBooks.InsertOneAsync(new ContactBook { UserId = userId });
            //}

            //var filter = Builders<ContactApplyRequest>.Filter.Where(r => r.UserId == userId
            //&& r.ApplierId == applierId);

            //var update = Builders<ContactApplyRequest>.Update
            //    .Set(r => r.Approvaled, 1)
            //    .Set(r => r.HandledTime, DateTime.Now);

            //var result = await _contactContext.ContactApplyRequests.UpdateOneAsync(filter, update, null, cancellationToken);
            //return result.MatchedCount == result.ModifiedCount && result.MatchedCount == 1;
        }

        /// <summary>
        /// 获取好友请求列表
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<List<ContactApplyRequest>> GetRequestListAsync(int userId, CancellationToken cancellationToken)
        {
            //根据当前用户id获取好友请求列表
            return (await _contactContext.ContactApplyRequests.FindAsync(r => r.UserId == userId)).ToList(cancellationToken);
        }
    }
}
