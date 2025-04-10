using System;
using Player;

namespace Match
{
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
        public PlayerModel[] Players;
        public string MatchId;
        public float TurnTime;
        public int RoundCount;
        public int TileCount;
    }
}
