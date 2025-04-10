using MessagePipe;

namespace Match
{
    public interface IMatchService
    {
        MatchModel Model { get; }
        ISubscriber<MatchState> OnMatchState { get; }
        ISubscriber<int> OnRoundStart { get; }

        void StartMatchmaking();
        void CancelMatchmaking();
        void LeaveMatch();
    }
}
