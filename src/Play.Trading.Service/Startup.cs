using System;
using System.Reflection;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Play.Common.Data;
using Play.Common.Identity;
using Play.Common.MassTansit;
using Play.Common.Settings;
using Play.Trading.Service.StateMachines;
using Play.Trading.Service.Exceptions;
using GreenPipes;
using Play.Trading.Service.Settings;
using Play.Inventory.Contracts;
using Play.Identity.Contracts;
using Play.Trading.Entities;
using Microsoft.AspNetCore.SignalR;
using Play.Trading.Service.SignalR;
using Play.Common.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Play.Trading.Service
{
    public class Startup
    {
        private const string allowedOriginsSettingsKey = "AllowedOrigins";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMongoDb()
                    .AddMongoRepository<CatalogItem>("CatalogItems")
                    .AddMongoRepository<InventoryItem>("InventoryItems")
                    .AddMongoRepository<ApplicationUser>("Users");
            services.AddJwtBearerAuthentication();

            AddMassTransit(services);

            services.AddControllers(options =>
            {
                ///To solve conflict happen on using nameOf(ControllerAction)
                options.SuppressAsyncSuffixInActionNames = false;
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Play.Trading.Service", Version = "v1" });
            });

            services.AddSingleton<IUserIdProvider, UserIdProvider>()
                    .AddSingleton<MessageHub>()
                    .AddSignalR();

            services.AddHealthChecks()
                    .AddMongo();           
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Play.Trading.Service v1"));

                app.UseCors(builder =>
                {
                    builder.WithOrigins(Configuration[allowedOriginsSettingsKey])
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                });
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<MessageHub>("/messagehub");
                endpoints.MapPlayEconomyHealthChecks();
            });
        }

        private void AddMassTransit(IServiceCollection services)
        {
            services.AddMassTransit(configure =>
            {
                configure.UsingPlayEconomyMessageBroker(Configuration, retryConfig =>
                {
                    retryConfig.Interval(3, TimeSpan.FromSeconds(5));
                    retryConfig.Ignore(typeof(UnknownItemException));
                });
                configure.AddConsumers(Assembly.GetEntryAssembly());
                configure.AddSagaStateMachine<PurchaseStateMachine, PurchaseState>(sagaConfig =>
                {
                    sagaConfig.UseInMemoryOutbox();
                })
                            .MongoDbRepository(r =>
                            {
                                var mongoDbSettings = Configuration.GetSection(nameof(MongoDbSettings)).Get<MongoDbSettings>();
                                r.Connection = mongoDbSettings.ConnectionString;
                                r.DatabaseName = mongoDbSettings.DbName;
                            });
            });

            var queueSettings = Configuration.GetSection(nameof(QueueSettings)).Get<QueueSettings>();

            EndpointConvention.Map<GrantItems>(new Uri(queueSettings.GrantItemsQueueAddress));

            EndpointConvention.Map<DebitGil>(new Uri(queueSettings.DebitGilQueueAddress));

            EndpointConvention.Map<SubtractItems>(new Uri(queueSettings.SubtractItemsQueueAddress));

            services.AddMassTransitHostedService();

            services.AddGenericRequestClient();

        }
    }
}
