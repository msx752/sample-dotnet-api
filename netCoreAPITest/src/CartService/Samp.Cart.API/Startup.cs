using MassTransit;
using Microsoft.EntityFrameworkCore;
using Samp.Basket.Database.Migrations;
using Samp.Cart.Database;
using Samp.Contract;
using Samp.Core.Extensions;
using Samp.Core.Model;

namespace Samp.Cart.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider isp)
        {
            app.UseGlobalStartupConfigures(env);
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGlobalStartupServices<CartApplicationSettings>(Configuration);

            var IdentityContext = new DbContextParameter<CartDbContext, CartDbContextSeed>((provider, opt) =>
                    opt.UseInMemoryDatabase(databaseName: nameof(CartDbContext)).EnableSensitiveDataLogging());

            services.AddCustomDbContext(IdentityContext);

            services.AddMassTransit(x =>
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", c =>
                    {
                        c.Username("guest");
                        c.Password("guest");
                    });
                });
            });
            services.AddSingleton<IMessageBus, MessageBus>();
        }
    }
}