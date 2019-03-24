using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace LeaderboardApp
{
    public class HttpFootballEventsSource : IFootballEventsSource
    {
        IHttpClientFactory _clientFactory;

        public HttpFootballEventsSource(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<FootballEvent[]> FetchEvents()
        {
            var eventsClient = _clientFactory.CreateClient();
            var eventsRequest = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5010/events");
            var eventsResponse = await eventsClient.SendAsync(eventsRequest);
            var eventsSerializer = new DataContractJsonSerializer(typeof(FootballEvent[]));
            var eventsStream = await eventsResponse.Content.ReadAsStreamAsync();
            return (FootballEvent[])eventsSerializer.ReadObject(eventsStream);
        }
    }
}
