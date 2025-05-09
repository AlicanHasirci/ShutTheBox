using System;
using Player;

namespace Match
{
    using System.Collections.Generic;
    using UnityEngine.Serialization;

    [Serializable]
    public class MatchModel
    {
        public List<PlayerModel> Players;
        public string MatchId;
        public float TurnTime;
        public int TileCount;
        [FormerlySerializedAs("Round")] public int RoundId;
    }
}
