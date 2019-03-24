using System.Runtime.Serialization;

namespace LeaderboardApp
{
    [DataContract]
    internal class FootballEvent
    {
        [DataMember]
        public string gameId;
    }
}
