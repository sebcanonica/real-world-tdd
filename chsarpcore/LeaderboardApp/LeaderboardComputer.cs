using System.Linq;

namespace LeaderboardApp
{
    public class LeaderboardComputer
    {
        public static Game[] FromEvents(params FootballEvent[] events)
        {

            return events
                .GroupBy(e => e.gameId)
                .Select(g =>
                {
                    var teamNames = g.Key.Split('-');
                    return new Game
                    {
                        home = FirstLetterToUpper(teamNames[0]),
                        visitor = FirstLetterToUpper(teamNames[1]),
                        state = ComputeGameState(g),
                        score = ComputeScore(g)                        
                    };
                }).ToArray();
        }
        
        private static string FirstLetterToUpper(string input)
        {
            return char.ToUpper(input[0]) + input.Substring(1);
        }

        private static string ComputeGameState(IGrouping<string, FootballEvent> g)
        {
            return g.Any(e => e.type.Equals("game-end")) ? "finished" : "in progress";
        }

        private static int[] ComputeScore(IGrouping<string, FootballEvent> g)
        {
            var goals = g.Where(e => e.type.Equals("goal"));
            var homeScore = goals.Where(e => e.gameId.StartsWith(e.team)).Count();
            var visitorScore = goals.Count() - homeScore;
            return new int[] { homeScore, visitorScore };            
        }
    }
}
