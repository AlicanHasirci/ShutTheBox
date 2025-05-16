namespace Network
{
    using MessagePipe;

    public interface IPlayerService
    {
        ISubscriber<JokerSelect> OnJoker { get; }
        ISubscriber<PlayerTurn> OnTurn { get; }
        ISubscriber<PlayerRoll> OnRoll { get; }
        ISubscriber<PlayerMove> OnMove { get; }
        ISubscriber<PlayerConfirm> OnConfirm { get; }

        void Roll();
        void Select(Joker joker);
        void Toggle(int index);
        void Confirm();
        void Done();
    }
}
