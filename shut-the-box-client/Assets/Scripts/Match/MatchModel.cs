using System;
using Player;

namespace Match
{
    using System.Collections.Generic;

    public enum MatchState
    {
        Idle,
        Finding,
        Joining,
        Waiting,
        Playing
    }
    
    [Serializable]
    public class MatchModel
    {
        public List<PlayerModel> Players;
        public List<RoundModel> Rounds;
        public string MatchId;
        public float TurnTime;
        public int TileCount;
    }

    [Serializable]
    public class RoundModel
    {
        public IList<string> Players;
        public IList<int> Scores;

        public RoundModel(IList<string> players, IList<int> scores)
        {
            Players = players;
            Scores = scores;
        }
    }
}
