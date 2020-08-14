using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmashBotUltimate.Bot;
using SmashBotUltimate.Bot.Modules;
using SmashBotUltimate.Bot.Modules.DBContextService;
using SmashBotUltimate.Bot.Modules.InstructionService;
using SmashBotUltimate.Bot.Modules.SavedDataServices;
using SmashBotUltimate.Models;
namespace SmashBotUltimate {
    public class Startup {
        public Startup (IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices (IServiceCollection services) {

            services.AddDbContext<PlayerContext> (options => {
                var configOptions = Configuration.GetConnectionString ("SmashConnectionSqlite");
                options.UseSqlite (configOptions);
            });

            services.AddSingleton<IResultService, ResultService> ();
            services.AddSingleton<IGuildService, GuildService> ();
            services.AddSingleton<IChannelRedirectionService, ChannelRedirectionService> ();
            services.AddTransient<IRandomUtilitiesService, RandomUtilitiesService> ();
            services.AddTransient<ISavedData<object, TimerData>, TimerService> ();

            services.AddSingleton<ILobbyService, LobbyService> (
                (serviceProvider) => {
                    var context = serviceProvider.GetService<PlayerContext> ();
                    return new LobbyService (context);
                }
            );
            services.AddSingleton<IInteractionService<CoinTossResult, string>, CoinTossService> (
                (serviceProvider) => {
                    return new CoinTossService (5, serviceProvider.GetService<IRandomUtilitiesService> ());
                }
            );
            services.AddSingleton<PlayerDBService> (
                (serviceProvider) => {
                    return new PlayerDBService (serviceProvider.GetService<PlayerContext> ());
                }
            );

            services.AddSingleton<IMatchmakingFilter, MatchmakingFilter> (
                (serviceProvider) => {
                    var db = serviceProvider.GetService<PlayerDBService> ();
                    var matchmakings = serviceProvider.GetServices<IMatchmaking> ();
                    return new MatchmakingFilter (db, matchmakings);
                }
            );

            //TODO: BuildServiceProvider creates another copy of the serivces, find another way.
            //!Using a factory like in CoinTossService we could pass the service provider but we must find a way to initialize the bot
            //?Maybe configure will make it work?
            var serviceProvider = services.BuildServiceProvider ();
            services.AddSingleton<SmashBot> (new SmashBot (serviceProvider));

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