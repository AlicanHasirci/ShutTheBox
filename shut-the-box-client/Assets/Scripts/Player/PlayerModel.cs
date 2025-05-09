using System;
using Sirenix.OdinInspector;

namespace Player
{
    using UnityEngine.Serialization;

    [EnumToggleButtons]
    public enum TileState
    {
        Open = 0,
        Toggle = 1,
        Shut = 2,
    }

    public enum PlayerState
    {
        Idle = 0,
        Roll = 1,
        Play = 2,
        Fail = 3,
    }

    [Serializable]
    public class PlayerModel
    {
        public string PlayerId;
        public PlayerState State;
        public TileState[] Tiles;
        public int[] Rolls;
        public int Score;

        public int TotalRoll
        {
            get
            {
                int total = 0;
                for (int i = 0; i < Rolls.Length; i++)
                {
                    total += Rolls[i];
                }

                return total;
            }
        }

        public PlayerModel(string playerId, int tileCount, int diceCount)
        {
            PlayerId = playerId;
            State = PlayerState.Idle;
            Tiles = new TileState[tileCount];
            Rolls = new int[diceCount];

            for (var i = 0; i < tileCount; i++)
            {
                Tiles[i] = TileState.Open;
            }
        }
    }
}
