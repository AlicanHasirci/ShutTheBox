using System;
using Sirenix.OdinInspector;

namespace Player
{
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

    public struct PlayerData
    {
        public PlayerState State;
        public TileState[] Tiles;
        public int Roll;
    }

    [Serializable]
    public class PlayerModel
    {
        public string PlayerId;
        public PlayerState State;
        public TileState[] Tiles;
        public int Roll;

        public PlayerModel(string playerId, int tileCount)
        {
            PlayerId = playerId;
            State = PlayerState.Idle;
            Tiles = new TileState[tileCount];
            Roll = -1;

            for (var i = 0; i < tileCount; i++)
            {
                Tiles[i] = TileState.Open;
            }
        }
    }
}
