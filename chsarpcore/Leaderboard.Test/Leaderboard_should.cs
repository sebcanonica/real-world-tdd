using Abbotware.Interop.NUnit;
using LeaderboardApp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json.Linq;
using NFluent;
using NUnit.Framework;
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
}

