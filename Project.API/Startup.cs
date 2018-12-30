using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Consul;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Project.Domain.AggregatesModel;
using Project.Infrastructure.Repositories;
using Project.Infrastructure;
using Project.API.Applications.Service;
using Project.API.Applications.Queries;
using Project.API.Options;

namespace Project.API
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
            services.AddDbContext<ProjectContext>(options =>
            {
                //配置Mysql连接字符串
                options.UseMySql(Configuration.GetConnectionString("MysqlProject"), sql =>
                 {
                    //DbContext不在当前项目时使用
                    sql.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
                 });
            });

            services.AddScoped<IRecommendService, TestRecommendService>()
                .AddScoped<IProjectQueries, ProjectQueries>(sp=>
                {
                    return new ProjectQueries(sp.GetRequiredService<ProjectContext>());
                })
                .AddScoped<IProjectRepository, ProjectRepository>();


            //配置信息Bind
            services.Configure<ServiceDiscoveryOptions>(Configuration.GetSection("ServiceDiscovery"));
            
            //注册全局单例IConsulClient
            services.AddSingleton<IConsulClient>(p => new ConsulClient(cfg =>
            {
                var serviceConfiguration = p.GetRequiredService<IOptions<ServiceDiscoveryOptions>>().Value;

                if (!string.IsNullOrEmpty(serviceConfiguration.Consul.HttpEndpoint))
                {
                    cfg.Address = new Uri(serviceConfiguration.Consul.HttpEndpoint);
                }
            }));

            #region JWT认证
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;           //是否启用Https
                    options.Audience = "project_api";               //当前API名称
                    options.Authority = "http://localhost:8070";    //网关地址
                    options.SaveToken = true;                       //是否保存Token
                });
            #endregion

            //添加MediatR
            services.AddMediatR();
            //services.AddMediatR(typeof(Domain.AggregatesModel.Project).Assembly);//handlers或者events在其他dll时使用

            #region CAP配置
            services.AddCap(options =>
            {
                options.UseEntityFramework<ProjectContext>()    //添加DbContext
                .UseRabbitMQ("47.100.36.224") //RabbitMQ地址
                .UseDashboard();        //启用Dashboard

                //启用服务发现
                options.UseDiscovery(d =>
                {
                    //服务发现服务器地址配置
                    d.DiscoveryServerHostName = "localhost";
                    d.DiscoveryServerPort = 8500;

                    //当前节点地址配置
                    d.CurrentNodeHostName = "localhost";
                    d.CurrentNodePort = 54793;
                    d.NodeId = 3;
                    d.NodeName = "Project.API CAP Node";
                });
            });
            #endregion

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
            IHostingEnvironment env,
            IApplicationLifetime applicationLifeTime,
            IOptions<ServiceDiscoveryOptions> serviceOptions,
            IConsulClient consul)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            //启动时注册服务
            applicationLifeTime.ApplicationStarted.Register(() =>
            {
                RegisterService(app, serviceOptions, consul);
            });

            //取消时移除服务
            applicationLifeTime.ApplicationStopped.Register(() =>
            {
                DeRegisterService(app, serviceOptions, consul);
            });

            //启用授权中间件
            app.UseAuthentication();
            //启用CAP中间件
            app.UseCap();
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
