using System;
using System.Collections.Generic;
using MessagePipe;
using DisposableBag = MessagePipe.DisposableBag;

namespace Player
{
    using Network;

    public interface IPlayerPresenter
    {
        ISubscriber<PlayerState> OnState { get; }
        IAsyncSubscriber<(int, bool)> OnRoll { get; }
        ISubscriber<TileState[]> OnBox { get; }
        void TileToggle(int index);
        void Initialize();
    }

    public class PlayerPresenter : IPlayerPresenter, IDisposable
    {
        public ISubscriber<PlayerState> OnState { get; }
        public IAsyncSubscriber<(int, bool)> OnRoll { get; }
        public ISubscriber<TileState[]> OnBox { get; }

        protected readonly PlayerModel Model;
        protected readonly IPlayerService Service;
        protected readonly IDisposablePublisher<PlayerState> StatePublisher;
        protected readonly IDisposablePublisher<TileState[]> BoxPublisher;
        protected readonly IDisposableAsyncPublisher<(int, bool)> RollPublisher;

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

            Model = model;
            Service = playerService;
            _disposable = DisposableBag.Create(
                StatePublisher,
                BoxPublisher,
                RollPublisher,
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
            await RollPublisher.PublishAsync((playerRoll.Roll, true));
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
            throw new NotImplementedException();
        }

        protected bool IsCurrentPlayer(string playerId)
        {
            return string.Equals(playerId, Model.PlayerId);
        }
    }
}
