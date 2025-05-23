using System;
using Player;

namespace Match
{
    using System.Collections.Generic;

    [Serializable]
    public class MatchModel
    {
        public List<PlayerModel> Players;
        public string MatchId;
        public float TurnTime;
        public int TileCount;
        public int RoundId;
    }
}
