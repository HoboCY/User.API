using Contact.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contact.API.Data
{
    public class ContactContext
    {
        private IMongoDatabase _database;
        private AppSettings _appSettings;

        public ContactContext(IOptionsSnapshot<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
            var client = new MongoClient(_appSettings.MongoConnectionString);
            if (client != null)
            {
                _database = client.GetDatabase(_appSettings.MongoContactDatabase);
            }
        }

        private void CheckOrCreateCollection(string collectionName)
        {
            var collectionNameList = _database.ListCollections().ToList().Select(l => l["name"].AsString);
            if (!collectionNameList.Contains(collectionName))
            {
                _database.CreateCollection(collectionName);
            }
        }

        /// <summary>
        /// 用户通讯录
        /// </summary>
        public IMongoCollection<ContactBook> ContactBooks
        {
            get
            {
                CheckOrCreateCollection("ContactBooks");
                return _database.GetCollection<ContactBook>("ContactBooks");
            }
        }

        /// <summary>
        /// 好友申请请求记录
        /// </summary>
        public IMongoCollection<ContactApplyRequest> ContactApplyRequests
        {
            get
            {
                CheckOrCreateCollection("ContactApplyRequest");
                return _database.GetCollection<ContactApplyRequest>("ContactApplyRequest");
            }
        }
    }
}
