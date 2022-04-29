using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CRM_Upack_kz.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebStudio.Services;

namespace CRM_Upack_kz
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var userManager = services.GetRequiredService<UserManager<User>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var context = services.GetRequiredService<UpackContext>();
                await RoleInitializer.Initializer(roleManager, userManager);
            }
            catch (Exception exception)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(exception, "Возникло исключение при инициализации ролей");
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }
}