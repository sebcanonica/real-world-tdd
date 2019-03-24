using Abbotware.Interop.NUnit;
using FluentAssertions.Json;
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

            var actual = await FetchLeaderboard(server);

            dynamic firstGame = actual.First;
            Check.ThatDynamic(firstGame.home.Value).IsEqualTo("Lyon");
            Check.ThatDynamic(firstGame.visitor.Value).IsEqualTo("Marseille");
            Check.ThatDynamic(firstGame.state.Value).IsEqualTo("in progress");
            Check.ThatDynamic(firstGame.score.Count).IsEqualTo(2);
        }

        [Test]
        public async Task Display_a_leaderboard_with_1_game_started()
        {
            var server = CreateServer(
                new FootballEvent { type = "game-start", gameId = "uriage-meylan" }
            );

            var actual = await FetchLeaderboard(server);

            actual.Should().BeEquivalentTo(JToken.Parse(@"[
                { ""home"": ""Uriage"", ""visitor"": ""Meylan"", ""state"": ""in progress"", ""score"": [0, 0] }
            ]"));
        }

        private TestServer CreateServer(params FootballEvent[] fakeEvents)
        {
            var fakeSource = new FakeFootballEventSource(fakeEvents);
            return new TestServer(
                new WebHostBuilder()
                    .UseStartup<Startup>()
                    .ConfigureTestServices(services => services.AddSingleton<IFootballEventsSource>(fakeSource))
            );
        }

        private async Task<JArray> FetchLeaderboard(TestServer server)
        {
            var client = server.CreateClient();
            var response = await client.GetAsync("/leaderboard");
            response.EnsureSuccessStatusCode();
            var resultString = await response.Content.ReadAsStringAsync();
            return JArray.Parse(resultString);
        }
    }

    [TestFixture]
    public class HttpFootballEventsSource_should
    {
        [Test, Timeout(6000)]
        [Ignore("Focus integration test")]
        public async Task Receive_valid_events_from_real_http_service()
        {
            var serviceProvider = new ServiceCollection().AddHttpClient().BuildServiceProvider();
            var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
            var source = new HttpFootballEventsSource(httpClientFactory);

            var footballEvents = await source.FetchEvents();

            CheckFootballEvents(footballEvents);
        }

        [Test]
        public async Task Receive_valid_events_from_fake()
        {
            var source = new FakeFootballEventSource(new FootballEvent[] {
                new FootballEvent { type = "game-start", gameId = "lyon-marseille" },
                new FootballEvent { type = "game-start", gameId = "paris-monaco" },
                new FootballEvent { type = "game-end", gameId = "paris-monaco" },
                new FootballEvent { type = "goal", gameId = "lyon-marseille", team = "lyon" }
            });

            var footballEvents = await source.FetchEvents();

            CheckFootballEvents(footballEvents);
        }

        private static void CheckFootballEvents(FootballEvent[] footballEvents)
        {
            foreach (var footballEvent in footballEvents)
            {
                Check.That(footballEvent.gameId.Split('-')).CountIs(2);
                Check.That(footballEvent.type).IsOneOfThese("game-start", "game-end", "goal");
            }
            Check.That(footballEvents.Where(e => e.type == "goal")).ContainsOnlyElementsThatMatch(e => e.gameId.Contains(e.team));
        }
    }

    internal class FakeFootballEventSource : IFootballEventsSource
    {

        FootballEvent[] _events;

        public FakeFootballEventSource(FootballEvent[] events)
        {
            _events = events;
        }

        public Task<FootballEvent[]> FetchEvents()
        {
            return Task.FromResult(_events);
        }
    }
}

