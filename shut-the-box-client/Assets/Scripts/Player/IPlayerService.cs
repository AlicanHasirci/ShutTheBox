using System.Collections.Generic;
using MessagePipe;

namespace Player
{
    public interface IPlayerService
    {
        ISubscriber<int> OnRoundStart { get; }
        ISubscriber<string> OnTurn { get; }
        ISubscriber<(string, int)> OnRoll { get; }
        ISubscriber<(string, int, TileState)> OnMove { get; }
        ISubscriber<(string, IReadOnlyList<TileState>)> OnConfirm { get; }

        void Ready();
        void Roll();
        void Toggle(int index);
        void Confirm();
        void Done();
    }
}
