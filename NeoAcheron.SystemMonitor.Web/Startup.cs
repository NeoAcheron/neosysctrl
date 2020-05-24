using System;
using System.Collections.Generic;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using MQTTnet.AspNetCore;
using NeoAcheron.SystemMonitor.Core;
using NeoAcheron.SystemMonitor.Core.Config;
using NeoAcheron.SystemMonitor.Core.Controllers;
using NeoAcheron.SystemMonitor.Web.Utils;

namespace NeoAcheron.SystemMonitor.Web
{
    public class Startup
    {
        private static readonly AdjusterConfig adjusterConfig = new AdjusterConfig();
        private static readonly SensorConfig sensorConfig = new SensorConfig();
        private static readonly ConnectionInterceptor connectionInterceptor = new ConnectionInterceptor();
        private static readonly RetainedMessagesManager RetainedMessagesManager = new RetainedMessagesManager();
        private static SystemTraverser systemTraverser;

        private static readonly string LocalhostOrigins = "_myLocalhostOrigins";

        private static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseKestrel(o =>
            {
                o.ListenAnyIP(1883, l => l.UseMqtt()); // MQTT pipeline
                o.ListenAnyIP(5000); // Default HTTP pipeline               
            })
        .UseStartup<Startup>()
        .Build();


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
               {
                   options.AddPolicy(name: LocalhostOrigins,
                                     builder =>
                                     {
                                         builder.WithOrigins("http://localhost:5000",
                                                             "http://localhost:4200")
                                        .AllowAnyMethod()
                                        .AllowAnyHeader();
                                     });
               });

            services.AddHostedMqttServer(mqttServerOptions => mqttServerOptions
                    .WithDefaultEndpoint()
                    .WithSubscriptionInterceptor(connectionInterceptor)
                    .WithUnsubscriptionInterceptor(connectionInterceptor)
                    .WithRetainedMessagesManager(RetainedMessagesManager)
                )
                .AddMqttConnectionHandler();

            services
                .AddConnections()
                .AddControllers();

            adjusterConfig.Load();
            sensorConfig.Load();

            systemTraverser = new SystemTraverser(sensorConfig);

            foreach (Adjuster adjuster in adjusterConfig.Adjusters)
            {
                adjuster.Start(systemTraverser);
            }

            services.AddSingleton<SystemTraverser>(systemTraverser);
            services.AddSingleton<AdjusterConfig>(adjusterConfig);
            services.AddSingleton<SensorConfig>(sensorConfig);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                DefaultFileNames = new List<string> { "index.html" }
            });

            app.UseCors(LocalhostOrigins);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(env.ContentRootPath + "/wwwroot/"),
                RequestPath = new PathString("")
            });
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMqtt("/mqtt");
                endpoints.MapControllers();
            });

            app.UseMqttServer(server =>
            {
                server.UseConnectionManager(systemTraverser);
            });
        }
    }
}
