namespace Network
{
    using MessagePipe;

    public interface IPlayerService
    {
        ISubscriber<RoundStart> OnRoundStart { get; }
        ISubscriber<PlayerTurn> OnTurn { get; }
        ISubscriber<PlayerRoll> OnRoll { get; }
        ISubscriber<PlayerMove> OnMove { get; }
        ISubscriber<PlayerConfirm> OnConfirm { get; }

        void Ready();
        void Roll();
        void Toggle(int index);
        void Confirm();
        void Done();
    }
}
