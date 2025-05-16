namespace Network
{
    using MessagePipe;

    public interface IMatchService
    {
        public string MatchId { get; }
        ISubscriber<MatchStart> OnMatchStart { get; }
        ISubscriber<RoundStart> OnRoundStart { get; }
        ISubscriber<MatchOver> OnMatchOver { get; }

        void StartMatchmaking();
        void CancelMatchmaking();
        void PlayerReady();
        void LeaveMatch();
    }
}
