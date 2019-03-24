using System.Threading.Tasks;

namespace LeaderboardApp
{
    public interface IFootballEventsSource
    {
        Task<FootballEvent[]> FetchEvents();
    }
}