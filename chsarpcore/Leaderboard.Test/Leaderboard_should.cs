using Abbotware.Interop.NUnit;
using LeaderboardApp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NFluent;
using NUnit.Framework;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Leaderboard.Test
{


    [TestFixture]
    public class Leaderboard_should
    {

        [Test,Timeout(6000)]
        [Ignore("System test")]
        public async Task Display_a_leaderboard_with_the_state_of_all_games()
        {
            var server = new TestServer(
                new WebHostBuilder().UseStartup<Startup>()                    
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

    [TestFixture]
    public class HttpFootballEventsSource_should
    {
        [Test, Timeout(6000)]
        //[Ignore("Focus integration test")]
        public async Task Receive_valid_events_from_real_http_service()
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var source = new HttpFootballEventsSource(httpClientFactory);

            var footballEvents = await source.FetchEvents();
            foreach(var footballEvent in footballEvents)
            {
                Check.That(footballEvent.gameId.Split('-')).CountIs(2);
                Check.That(footballEvent.type).IsOneOfThese("game-start", "game-end", "goal");
            }
            Check.That(footballEvents.Where(e => e.type == "goal")).ContainsOnlyElementsThatMatch(e => e.gameId.Contains(e.team));            
        }
    }
}

