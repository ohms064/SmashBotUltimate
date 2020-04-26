using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmashBotUltimate.Bot;
using SmashBotUltimate.Bot.Modules;
using SmashBotUltimate.Models;
namespace SmashBotUltimate {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {

            services.AddSingleton<IResultService, ResultService> ();
            services.AddSingleton<IGuildService, GuildService> ();
            services.AddSingleton<IChannelRedirectionService, ChannelRedirectionService> ();
            services.AddSingleton<IRandomUtilitiesService, RandomUtilitiesService> ();

            //TODO: This creates another copy of the serivces, find another way.
            services.AddSingleton<SmashBot> (new SmashBot (services.BuildServiceProvider ()));

            services.AddControllers ();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure (IApplicationBuilder app, IWebHostEnvironment env) {
            if (env.IsDevelopment ()) {
                app.UseDeveloperExceptionPage ();
            }

            app.UseHttpsRedirection ();

            app.UseRouting ();

            app.UseAuthorization ();

            app.UseEndpoints (endpoints => {
                endpoints.MapControllers ();
            });
        }
    }
}