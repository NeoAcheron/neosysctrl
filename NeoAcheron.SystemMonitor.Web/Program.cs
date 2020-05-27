using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Utf8Json;
using Utf8Json.Resolvers;

namespace NeoAcheron.SystemMonitor.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CompositeResolver.RegisterAndSetAsDefault(new[] {
            // resolver custom types first
                AttributeFormatterResolver.Instance,
                DynamicGenericResolver.Instance,
                StandardResolver.CamelCase
            });
            JsonSerializer.SetDefaultResolver(CompositeResolver.Instance);

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
