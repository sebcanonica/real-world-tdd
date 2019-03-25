using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Runtime.Serialization.Json;

namespace LeaderboardApp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddRouting()
                .AddHttpClient()
                .AddSingleton<IFootballEventsSource, HttpFootballEventsSource>();
        }

        // Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        { 
            app.UseRouter(router =>
            {
                router.MapGet("/leaderboard", async context =>
                {
                    var eventsSource = context.RequestServices.GetRequiredService<IFootballEventsSource>();
                    var events = await eventsSource.FetchEvents();

                    var leaderboard = LeaderboardComputer.FromEvents(events);

                    context.Response.ContentType = "application/json";
                    var ser = new DataContractJsonSerializer(leaderboard.GetType());
                    ser.WriteObject(context.Response.Body, leaderboard);
                });
            });
        }

        
    }
}