using Abbotware.Interop.NUnit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NFluent;
using NUnit.Framework;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Leaderboard.Test
{


    [TestFixture]
    public class Leaderboard_should
    {

        [Test,Timeout(6000)]
        public async Task Display_a_leaderboard_with_the_state_of_all_games()
        {
            var server = new TestServer(
                new WebHostBuilder()
                    .ConfigureServices( services =>
                    {
                        services
                            .AddRouting()
                            .AddHttpClient();
                    })
                    .Configure(app =>
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
                                var eventsSerializer = new DataContractJsonSerializer(typeof(FootbalEvent[]));
                                var eventsStream = await eventsResponse.Content.ReadAsStreamAsync();
                                var events = (FootbalEvent[])eventsSerializer.ReadObject(eventsStream);
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
                    })
            );
            var client = server.CreateClient();

            var result = await client.GetAsync("/leaderboard");

            result.EnsureSuccessStatusCode();
            var resultString = await result.Content.ReadAsStringAsync();
            var resultLeaderboard = JArray.Parse(resultString);
            dynamic firstGame = resultLeaderboard.First;
            Check.ThatDynamic(firstGame.home.Value).IsEqualTo("Lyon");
            Check.ThatDynamic(firstGame.visitor.Value).IsEqualTo("Marseille");
            Check.ThatDynamic(firstGame.state.Value).IsEqualTo("in progress");
            Check.ThatDynamic(firstGame.score.Count).IsEqualTo(2);
        }
    }


    [DataContract]
    public class Game
    {
        [DataMember]
        public string home;
        [DataMember]
        public string visitor;
        [DataMember]
        public int[] score;
        [DataMember]
        public string state;
    }

    [DataContract]
    internal class FootbalEvent
    {
        [DataMember]
        public string gameId;
    }
}

