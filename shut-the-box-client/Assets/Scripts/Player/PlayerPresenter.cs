
namespace Player
{
    using Network;
    using System;
    using MessagePipe;
    using DisposableBag = MessagePipe.DisposableBag;

    public interface IPlayerPresenter
    {
        ISubscriber<PlayerState> OnState { get; }
        IAsyncSubscriber<(int, bool)> OnRoll { get; }
        ISubscriber<TileState[]> OnBox { get; }
        ISubscriber<int> OnScore { get; }
        void TileToggle(int index);
        void Initialize();
    }

    public class PlayerPresenter : IPlayerPresenter, IDisposable
    {
        public ISubscriber<PlayerState> OnState { get; }
        public IAsyncSubscriber<(int, bool)> OnRoll { get; }
        public ISubscriber<TileState[]> OnBox { get; }
        public ISubscriber<int> OnScore { get; }

        protected readonly PlayerModel Model;
        protected readonly IPlayerService Service;
        protected readonly IDisposablePublisher<PlayerState> StatePublisher;
        protected readonly IDisposablePublisher<TileState[]> BoxPublisher;
        protected readonly IDisposableAsyncPublisher<(int, bool)> RollPublisher;
        protected readonly IDisposablePublisher<int> ScorePublisher;

        private readonly IDisposable _disposable;

        public PlayerPresenter(
            PlayerModel model,
            IPlayerService playerService,
            IMatchService matchService,
            EventFactory eventFactory
        )
        {
            (StatePublisher, OnState) = eventFactory.CreateEvent<PlayerState>();
            (BoxPublisher, OnBox) = eventFactory.CreateEvent<TileState[]>();
            (RollPublisher, OnRoll) = eventFactory.CreateAsyncEvent< (int, bool)>();
            (ScorePublisher, OnScore) = eventFactory.CreateEvent<int>();

            Model = model;
            Service = playerService;
            _disposable = DisposableBag.Create(
                StatePublisher,
                BoxPublisher,
                RollPublisher,
                ScorePublisher,
                matchService.OnRoundStart.Subscribe(OnRoundStart),
                playerService.OnTurn.Subscribe(OnPlayerTurn),
                playerService.OnRoll.Subscribe(OnPlayerRoll),
                playerService.OnMove.Subscribe(OnPlayerMove),
                playerService.OnConfirm.Subscribe(OnPlayerConfirm)
            );
        }

        public void Initialize()
        {
            StatePublisher.Publish(Model.State);
            BoxPublisher.Publish(Model.Tiles);
            ScorePublisher.Publish(Model.Score);
            if (Model.State is PlayerState.Play)
            {
                RollPublisher.Publish((Model.Roll, true));
            }
        }

        public void Reset()
        {
            Model.State = PlayerState.Idle;
            Model.Roll = -1;

            for (var i = 0; i < Model.Tiles.Length; i++)
            {
                Model.Tiles[i] = TileState.Open;
            }
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        public virtual void TileToggle(int index) { }

        private void OnRoundStart(RoundStart roundStart)
        {
            Reset();
            StatePublisher.Publish(Model.State);
            BoxPublisher.Publish(Model.Tiles);
        }

        protected virtual void OnPlayerTurn(PlayerTurn playerTurn)
        {
            if (!IsCurrentPlayer(playerTurn.PlayerId))
            {
                return;
            }
            StatePublisher.Publish(PlayerState.Roll);
        }

        protected virtual async void OnPlayerRoll(PlayerRoll playerRoll)
        {
            if (!IsCurrentPlayer(playerRoll.PlayerId))
            {
                return;
            }
            Model.Roll = playerRoll.Roll;
            await RollPublisher.PublishAsync((playerRoll.Roll, false));
        }

        protected virtual void OnPlayerMove(PlayerMove playerMove)
        {
            if (!IsCurrentPlayer(playerMove.PlayerId))
            {
                return;
            }
            Model.Tiles[playerMove.Index] = (TileState)playerMove.State;
            BoxPublisher.Publish(Model.Tiles);
        }

        protected virtual void OnPlayerConfirm(PlayerConfirm playerConfirm)
        {
            if (!IsCurrentPlayer(playerConfirm.PlayerId))
            {
                return;
            }

            for (int i = 0; i < Model.Tiles.Length; i++)
            {
                Model.Tiles[i] = (TileState)playerConfirm.Tiles[i];
            }

            Model.State = PlayerState.Idle;
            Model.Score += playerConfirm.Score;
            BoxPublisher.Publish(Model.Tiles);
            StatePublisher.Publish(Model.State);
            ScorePublisher.Publish(Model.Score);
        }

        protected bool IsCurrentPlayer(string playerId)
        {
            return string.Equals(playerId, Model.PlayerId);
        }
    }
}
