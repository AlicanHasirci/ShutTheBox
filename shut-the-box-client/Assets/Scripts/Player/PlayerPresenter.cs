using System;
using System.Collections.Generic;
using MessagePipe;
using DisposableBag = MessagePipe.DisposableBag;

namespace Player
{
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
            EventFactory eventFactory
        )
        {
            (StatePublisher, OnState) = eventFactory.CreateEvent<PlayerState>();
            (BoxPublisher, OnBox) = eventFactory.CreateEvent<TileState[]>();
            (RollPublisher, OnRoll) = eventFactory.CreateAsyncEvent< (int, bool)>();

            Model = model;
            Service = playerService;
            _disposable = DisposableBag.Create(
                RollPublisher,
                BoxPublisher,
                playerService.OnRoundStart.Subscribe(_ => RoundStartReceived()),
                playerService.OnTurn.Subscribe(_ => StateReceived(PlayerState.Roll), Filter),
                playerService.OnRoll.Subscribe(s => RollReceived(s.Item2),s => Filter(s.Item1)),
                playerService.OnMove.Subscribe(s => MoveReceived(s.Item2, s.Item3),s => Filter(s.Item1)),
                playerService.OnConfirm.Subscribe(s => BoxReceived(s.Item2), s => Filter(s.Item1))
            );
        }

        public void Initialize()
        {
            StateReceived(Model.State);
            BoxReceived(Model.Tiles);
            if (Model.State is PlayerState.Play)
            {
                RollReceived(Model.Roll, true);
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

        public virtual void RoundStartReceived()
        {
            Reset();
            StateReceived(Model.State);
            BoxReceived(Model.Tiles);
        }

        protected virtual void StateReceived(PlayerState state)
        {
            StatePublisher.Publish(state);
        }

        protected virtual void MoveReceived(int index, TileState state)
        {
            Model.Tiles[index] = state;
            BoxPublisher.Publish(Model.Tiles);
        }

        protected virtual void BoxReceived(IReadOnlyList<TileState> tiles)
        {
            for (int i = 0; i < tiles.Count; i++)
            {
                Model.Tiles[i] = tiles[i];
            }
            BoxPublisher.Publish(Model.Tiles);
        }

        protected virtual async void RollReceived(int value, bool skip = false)
        {
            Model.Roll = value;
            await RollPublisher.PublishAsync((value, skip));
        }

        private bool Filter(string playerId)
        {
            return playerId == Model.PlayerId;
        }
        
    }
}
