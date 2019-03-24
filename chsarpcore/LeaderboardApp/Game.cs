using System.Runtime.Serialization;

namespace LeaderboardApp
{
    [DataContract]
    internal class Game
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
}
