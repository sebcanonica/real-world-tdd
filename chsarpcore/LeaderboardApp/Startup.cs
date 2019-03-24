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
                .AddHttpClient();
        }

        // Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        { 
            app.UseRouter(router =>
            {
                router.MapGet("/leaderboard", async context =>
                {
                    string FirstLetterToUpper(string input)
                    {
                        return char.ToUpper(input[0]) + input.Substring(1);
                    }

                    var clientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                    var eventsClient = clientFactory.CreateClient();
                    var eventsRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5010/events");
                    var eventsResponse = await eventsClient.SendAsync(eventsRequest);
                    var eventsSerializer = new DataContractJsonSerializer(typeof(FootballEvent[]));
                    var eventsStream = await eventsResponse.Content.ReadAsStreamAsync();
                    var events = (FootballEvent[])eventsSerializer.ReadObject(eventsStream);
                    var firstEvent = events[0];
                    var teamNames = firstEvent.gameId.Split('-');
                    var leaderboard = new Game[] {
                                    new Game {
                                        home = FirstLetterToUpper(teamNames[0]),
                                        visitor = FirstLetterToUpper(teamNames[1]),
                                        score = new int[] {0, 0},
                                        state = "in progress"
                                    }
                                };
                    var ser = new DataContractJsonSerializer(leaderboard.GetType());
                    ser.WriteObject(context.Response.Body, leaderboard);
                });
            });
        }

    }
}