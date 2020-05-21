using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SmashBotUltimate {
    public class Program {
        public static void Main (string[] args) {
            CreateHostBuilder (args).Build ().Run ();
        }

        public static IHostBuilder CreateHostBuilder (string[] args) =>
            Host.CreateDefaultBuilder (args)
            .ConfigureWebHostDefaults (webBuilder => {
                webBuilder.UseStartup<Startup> ();
                webBuilder.UseUrls ("http://*:8080");
            });

    }

    public static class IHostExtension {
        public static IWebHost MigrateDatabase<T> (this IWebHost webHost) where T : DbContext {
            using (var scope = webHost.Services.CreateScope ()) {
                var services = scope.ServiceProvider;
                try {
                    var db = services.GetRequiredService<T> ();
                    db.Database.Migrate ();
                }
                catch (Exception ex) {
                    var logger = services.GetRequiredService<ILogger<Program>> ();
                    logger.LogError (ex, "An error occurred while migrating the database.");
                }
            }
            return webHost;
        }
    }
}