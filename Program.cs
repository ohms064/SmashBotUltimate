using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SmashBotUltimate.Models;

namespace SmashBotUltimate {
    public class Program {
        public static void Main (string[] args) {
            CreateHostBuilder (args).Build ().MigrateDatabase<PlayerContext> ().Run ();
        }

        public static IHostBuilder CreateHostBuilder (string[] args) =>
            Host.CreateDefaultBuilder (args)
            .ConfigureWebHostDefaults (webBuilder => {
                webBuilder.UseStartup<Startup> ();
                webBuilder.UseUrls ("http://*:8080");

            });

    }

    public static class IHostExtension {
        public static IHost MigrateDatabase<T> (this IHost webHost) where T : DbContext {
            using (var scope = webHost.Services.CreateScope ()) {
                var services = scope.ServiceProvider;
                try {
                    var context = services.GetRequiredService<T> ();
                    context.Database.Migrate ();
                } catch (Exception ex) {
                    var logger = services.GetRequiredService<ILogger<Program>> ();
                    logger.LogError (ex, "An error occurred while migrating the database.");
                }
            }
            return webHost;
        }
    }
}