using IdentityServer4;
using IdentityServer4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.Identity
{
    public class Config
    {
        //获取所有资源
        public static IEnumerable<ApiResource> GetResources()
        {
            //所有的可以访问的对象
            return new List<ApiResource>()
            {
                new ApiResource("gateway_api","user service"),
                new ApiResource("user_api","user service"),
                new ApiResource("contact_api","contact service"),
                new ApiResource("project_api","project service"),
                new ApiResource("recommend_api","recommend service")
            };
        }

        //获取所有客户端
        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                 ClientId = "iphone",   //Client的Id
                 RefreshTokenExpiration=TokenExpiration.Sliding,
                 AllowedGrantTypes =new List<string>{ "sms_auth_code"},  //访问模式
                 AllowOfflineAccess=true,
                 RequireClientSecret = false,
                 AlwaysIncludeUserClaimsInIdToken=true,
                 ClientSecrets =
                    {
                     new Secret("secret".Sha256())
                 },
                 AllowedScopes=new List<string> //这个Client允许访问的Resource
                 {
                   "gateway_api",
                   "user_api",
                   "contact_api",
                   "project_api",
                   "recommend_api",
                   IdentityServerConstants.StandardScopes.OpenId,
                   IdentityServerConstants.StandardScopes.Profile,
                   IdentityServerConstants.StandardScopes.OfflineAccess}
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }
    }
}
