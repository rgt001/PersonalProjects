using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LOLApi.Classes
{
    public class Spectate
    {
        public long gameId { get; set; }
        public int mapId { get; set; }
        public string gameMode { get; set; }
        public string gameType { get; set; }
        public Participant[] participants { get; set; }
        public Observers observers { get; set; }
        public string platformId { get; set; }
        public object[] bannedChampions { get; set; }
        public long gameStartTime { get; set; }
        public long gameLength { get; set; }
    }

    public class Observers
    {
        public string encryptionKey { get; set; }
    }

    public class Participant
    {
        public int teamId { get; set; }
        public int spell1Id { get; set; }
        public int spell2Id { get; set; }
        public int championId { get; set; }
        public int profileIconId { get; set; }
        public string summonerName { get; set; }
        public bool bot { get; set; }
        public string summonerId { get; set; }
        public object[] gameCustomizationObjects { get; set; }
        public Perks perks { get; set; }
    }

    public class Perks
    {
        public int[] perkIds { get; set; }
        public int perkStyle { get; set; }
        public int perkSubStyle { get; set; }
    }

}
