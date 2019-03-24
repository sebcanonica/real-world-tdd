using Microsoft.AspNetCore.Hosting;

namespace LeaderboardApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseStartup<Startup>()
                .UseUrls("http://localhost:5020")
                .Build();
            host.Run();
        }
    }
}
