using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _4Chal_Live.Controls
{
    public class Spectator
    {
        public gameList[] gameList { get; set; }
    }
    public class gameList
    {
        public int gameId { get; set; }
        public string platformId { get; set; }
        public long gameStartTime { get; set; }

        public participants[] participants { get; set; }
        public observers observers { get; set; }
        public bannedChampions[] bannedChampions { get; set; }
    }

    public class participants
    {
        public int teamId { get; set; }
        public int Spell1Id { get; set; }
        public int Spell2Id { get; set; }
        public int championId { get; set; }
        public string summonerName { get; set; }

    }

    public class observers
    {
        public string encryptionKey { get; set; }
    }

    public class bannedChampions
    {
        public int championId { get; set; }
        public int teamId { get; set; }
    }


}

