using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace PocketBookServer
{
#pragma warning disable CA1052 // Static holder types should be Static or NotInheritable

    public class Program
#pragma warning restore CA1052 // Static holder types should be Static or NotInheritable
    {
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args).UseStartup<Startup>();
        }

        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
    }
}