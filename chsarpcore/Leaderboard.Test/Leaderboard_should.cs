using Abbotware.Interop.NUnit;
using FluentAssertions;
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

        //[Test,Timeout(6000)]
        [Category("System test")]
        public async Task Display_a_leaderboard_with_the_state_of_all_games()
        {
            var server = new TestServer(
                new WebHostBuilder().UseStartup<Startup>()                    
            );

            var actual = await FetchLeaderboard(server);

            foreach (var game in actual)
            {
                game.Should().HaveElement("home");
                game["home"].Type.Should().Be(JTokenType.String);
                game.Should().HaveElement("visitor");
                game["visitor"].Type.Should().Be(JTokenType.String);
                game.Should().HaveElement("score");
                game["score"].Type.Should().Be(JTokenType.Array);
                game["score"].Should().HaveCount(2);
                foreach (var score in game["score"])
                {
                    score.Type.Should().Be(JTokenType.Integer);
                    score.Value<int>().Should().BeGreaterOrEqualTo(0);
                }
                game.Should().HaveElement("state");
                game["state"].Type.Should().Be(JTokenType.String);
                game["state"].Value<string>().Should().BeOneOf("in progress", "finished");
            }
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
        //[Test, Timeout(6000)]
        [Category("Focused integration test")]
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

        public static void CheckFootballEvents(FootballEvent[] footballEvents)
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
            HttpFootballEventsSource_should.CheckFootballEvents(events);
            _events = events;            
        }

        public Task<FootballEvent[]> FetchEvents()
        {            
            return Task.FromResult(_events);
        }
    }

    [TestFixture]
    public class LeaderboardComputer_should
    {
        [Test]
        public void Return_one_in_progress_game_when_given_only_a_game_start_event()
        {
            var leaderboard = LeaderboardComputer.FromEvents(
                new FootballEvent { type = "game-start", gameId = "uriage-meylan" }
            );

            leaderboard.Should().BeEquivalentTo(
                new Game { home = "Uriage", visitor = "Meylan", state = "in progress", score = new int[] { 0, 0 } }
            );
        }

        [Test]
        public void Return_two_in_progress_game_when_given_two_game_start_events()
        {
            var leaderboard = LeaderboardComputer.FromEvents(
                new FootballEvent { type = "game-start", gameId = "uriage-meylan" },
                new FootballEvent { type = "game-start", gameId = "fontaine-sassenage" }
            );

            leaderboard.Should().BeEquivalentTo(
                new Game { home = "Uriage", visitor = "Meylan", state = "in progress", score = new int[] { 0, 0 } },
                new Game { home = "Fontaine", visitor = "Sassenage", state = "in progress", score = new int[] { 0, 0 } }
            );
        }

        [Test]
        public void Return_a_finished_game_when_given_both_game_start_and_end_events()
        {
            var leaderboard = LeaderboardComputer.FromEvents(
                new FootballEvent { type = "game-start", gameId = "uriage-meylan" },
                new FootballEvent { type = "game-end", gameId = "uriage-meylan" }
            );

            leaderboard.Should().BeEquivalentTo(
                new Game { home = "Uriage", visitor = "Meylan", state = "finished", score = new int[] { 0, 0 } }
            );
        }

        [Test]
        public void Add_point_to_home_team_on_a_goal_event()
        {
            var leaderboard = LeaderboardComputer.FromEvents(
                new FootballEvent { type = "game-start", gameId = "uriage-meylan" },
                new FootballEvent { type = "goal", gameId = "uriage-meylan", team = "uriage" }
            );

            leaderboard.Should().BeEquivalentTo(
                new Game { home = "Uriage", visitor = "Meylan", state = "in progress", score = new int[] { 1, 0 } }
            );
        }

        [Test]
        public void Add_point_to_visitor_team_on_a_goal_event()
        {
            var leaderboard = LeaderboardComputer.FromEvents(
                new FootballEvent { type = "game-start", gameId = "uriage-meylan" },
                new FootballEvent { type = "goal", gameId = "uriage-meylan", team = "meylan" }
            );

            leaderboard.Should().BeEquivalentTo(
                new Game { home = "Uriage", visitor = "Meylan", state = "in progress", score = new int[] { 0, 1 } }
            );
        }
    }
}

