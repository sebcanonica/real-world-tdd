using System.Runtime.Serialization;

namespace LeaderboardApp
{
    [DataContract]
    public class FootballEvent
    {
        [DataMember]
        public string gameId;
        [DataMember]
        public string type;
        [DataMember]
        public string team;
    }
}
