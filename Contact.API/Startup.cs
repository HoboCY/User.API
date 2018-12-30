using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Contact.API.Data;
using Contact.API.Infrastructure;
using Contact.API.IntegrationEvents.EventHandling;
using Contact.API.Models;
using Contact.API.Options;
using Contact.API.Services;
using DnsClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resilience;

namespace Contact.API
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
            services.Configure<AppSettings>(a =>
            {
                a.MongoConnectionString = Configuration["MongoConnectionString"].ToString();
                a.MongoContactDatabase = Configuration["MongoContactDatabase"].ToString();
            });

            //配置文件Binding
            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));

            #region 依赖注入对象注册
            //注册全局单例IDnsQuery
            services.AddSingleton<IDnsQuery>(d =>
            {
                var serviceConfiguration = d.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                return new LookupClient(serviceConfiguration.Consul.DnsEndpoint.ToIPEndPoint());
            });
            
            //注册全局单例IConsulClient
            services.AddSingleton<IConsulClient>(s => new ConsulClient(cfg =>
            {
                var servicesConfiguration = s.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;
                if (!string.IsNullOrEmpty(servicesConfiguration.Consul.HttpEndpoint))
                {
                    cfg.Address = new Uri(servicesConfiguration.Consul.HttpEndpoint);
                }
            }));

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

            //注册全局单例ContactContext
            services.AddSingleton<ContactContext>();
            services.AddScoped<IContactRepository, MongoContactRepository>()
                .AddScoped<IContactApplyRequestRepository, MongoContactApplyRequestRepository>()
                .AddScoped<UserProfileChangedEventHandler>()
                .AddScoped<IUserService, UserService>();
            #endregion

            #region JWT认证
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;           //是否启用Https
                    options.Audience = "contact_api";               //当前API名称
                    options.Authority = "http://localhost:8070";    //网关地址
                    options.SaveToken = true;                       //是否保存Token
                });
            #endregion

            #region CAP配置
            services.AddCap(options =>
            {
                options.UseMySql("server=47.100.36.224;port=3306;database=beta_user;userid=hobo;password=cy199927");//配置Mysql
                options.UseRabbitMQ("47.100.36.224");   //配置RabbitMQ

                options.UseDashboard();     //启用控制面板
                options.UseDiscovery(d =>   //启用服务发现
                {
                    //服务发现地址配置
                    d.DiscoveryServerHostName = "localhost";    //
                    d.DiscoveryServerPort = 8500;

                    //当前节点地址配置
                    d.CurrentNodeHostName = "localhost";
                    d.CurrentNodePort = 29744;
                    d.NodeId = 2;
                    d.NodeName = "Contact.API CAP Node";
                });
            });
            #endregion
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IApplicationLifetime lifetime,
            IOptions<ServiceDiscoveryOptions> options,
            IConsulClient consulClient)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //启动时注册服务
            lifetime.ApplicationStarted.Register(() =>
            {
                RegisterService(app, options, consulClient);
            });

            //取消时移除服务
            lifetime.ApplicationStopped.Register(() =>
            {
                DeRegisterService(app, options, consulClient);
            });

            //启用CAP中间件
            app.UseCap();
            //启用授权中间件
            app.UseAuthentication();
            app.UseMvc();
        }

        #region 移除服务
        private void DeRegisterService(IApplicationBuilder app, IOptions<ServiceDiscoveryOptions> serviceOptions, IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                consul.Agent.ServiceDeregister(serviceId).GetAwaiter().GetResult();
            }
        }
        #endregion

        #region 注册服务
        private void RegisterService(IApplicationBuilder app,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            var features = app.Properties["server.Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>()
                .Addresses
                .Select(p => new Uri(p));

            foreach (var address in addresses)
            {
                var serviceId = $"{serviceOptions.Value.ServiceName}_{address.Host}:{address.Port}";

                var httpCheck = new AgentServiceCheck()
                {
                    DeregisterCriticalServiceAfter = TimeSpan.FromMinutes(1),
                    Interval = TimeSpan.FromSeconds(30),
                    HTTP = new Uri(address, "HealthCheck").OriginalString
                };

                var registration = new AgentServiceRegistration()
                {
                    Checks = new[] { httpCheck },
                    Address = address.Host,
                    ID = serviceId,
                    Name = serviceOptions.Value.ServiceName,
                    Port = address.Port
                };

                consul.Agent.ServiceRegister(registration).GetAwaiter().GetResult();
            }
        }
        #endregion
    }
}
