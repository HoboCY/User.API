using DnsClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Recommend.API.Data;
using Recommend.API.Infrastructure;
using Recommend.API.IntegrationEventHandlers;
using Recommend.API.Options;
using Recommend.API.Services;
using Resilience;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Recommend.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //配置DbContext
            services.AddDbContext<RecommendContext>(options =>
            {
                options.UseMySql(Configuration.GetConnectionString("MysqlRecommend"));//设置Mysql连接字符串
            });

            #region 依赖注入对象注册
            //配置文件Binding

            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            services.AddSingleton<IDnsQuery>(d =>
            {
                var serviceConfiguration = d.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                return new LookupClient(serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint());
            });

            //注册全局单例ResilienceClientFactory
            services.AddSingleton(typeof(ResilienceClientFactory), sp =>
            {
                var loggger = sp.GetRequiredService<ILogger<ResilienceHttpClient>>();
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var retryCount = 5;
                var expCountAllowedBeforeBreak = 5;

                return new ResilienceClientFactory(loggger, httpContextAccessor, retryCount, expCountAllowedBeforeBreak);
            });

            //注册全局单例IHttpClient
            services.AddSingleton<IHttpClient>(sp =>
            {
                var resilienceClientFactory = sp.GetRequiredService<ResilienceClientFactory>();
                return resilienceClientFactory.GetResilienceHttpClient();
            });

            //注册Scope
            services.AddScoped<IUserService, UserService>()
                .AddScoped<IContactService, ContactService>()
                .AddScoped<ProjectCreatedIntegrationEventHandler>();
            #endregion

            #region JWT配置
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = "recommend_api";
                    options.Authority = "http://localhost:8070";    //网关地址
                });
            #endregion

            #region CAP配置
            services.AddCap(options =>
            {
                options.UseEntityFramework<RecommendContext>();
                options.UseRabbitMQ("47.100.36.224");   //配置RabbitMQ

                options.UseDashboard();     //启用控制面板

                //启用服务发现
                options.UseDiscovery(d =>
                {
                    //服务发现服务器地址配置
                    d.DiscoveryServerHostName = "localhost";
                    d.DiscoveryServerPort = 8500;

                    //当前节点地址配置
                    d.CurrentNodeHostName = "localhost";    //当前Node的地址
                    d.CurrentNodePort = 59852;
                    d.NodeId = 4;
                    d.NodeName = "Recommend.API CAP Node";
                });
            });
            #endregion
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAuthentication();
            //启用CAP中间件
            app.UseCap();
            app.UseMvc();
        }
    }
}
