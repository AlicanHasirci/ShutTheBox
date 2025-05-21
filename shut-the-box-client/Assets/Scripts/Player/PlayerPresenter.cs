
namespace Player
{
    using Network;
    using System;
    using Jokers;
    using MessagePipe;
    using UnityEngine;
    using DisposableBag = MessagePipe.DisposableBag;

    public interface IPlayerPresenter
    {
        PlayerModel Model { get; }
        ISubscriber<PlayerState> OnState { get; }
        IAsyncSubscriber<(int[], bool)> OnRoll { get; }
        ISubscriber<(Joker, int)> OnJokerActivate { get; }
        ISubscriber<JokerSelection> OnJokerSelection { get; }
        ISubscriber<Joker> OnJokerSelect { get; }
        ISubscriber<TileState[]> OnBox { get; }
        ISubscriber<int> OnScore { get; }
        void TileToggle(int index);
        void Initialize();
    }

    public class PlayerPresenter : IPlayerPresenter, IDisposable
    {
        public PlayerModel Model { get; }
        public ISubscriber<PlayerState> OnState { get; }
        public IAsyncSubscriber<(int[], bool)> OnRoll { get; }
        public ISubscriber<(Joker, int)> OnJokerActivate { get; }
        public ISubscriber<JokerSelection> OnJokerSelection { get; }
        public ISubscriber<Joker> OnJokerSelect { get; }
        public ISubscriber<TileState[]> OnBox { get; }
        public ISubscriber<int> OnScore { get; }

        protected readonly IPlayerService Service;
        protected readonly IDisposablePublisher<TileState[]> BoxPublisher;
        private readonly IDisposablePublisher<PlayerState> _statePublisher;
        private readonly IDisposableAsyncPublisher<(int[], bool)> _rollPublisher;
        private readonly IDisposablePublisher<(Joker, int)> _jokerActivatePublisher;
        private readonly IDisposablePublisher<JokerSelection> _jokerSelectionPublisher;
        private readonly IDisposablePublisher<Joker> _jokerSelectPublisher;
        private readonly IDisposablePublisher<int> _scorePublisher;

        private readonly IDisposable _disposable;

        public PlayerPresenter(
            PlayerModel model,
            IPlayerService playerService,
            IMatchService matchService,
            EventFactory eventFactory
        )
        {
            (BoxPublisher, OnBox) = eventFactory.CreateEvent<TileState[]>();
            (_statePublisher, OnState) = eventFactory.CreateEvent<PlayerState>();
            (_rollPublisher, OnRoll) = eventFactory.CreateAsyncEvent< (int[], bool)>();
            (_jokerActivatePublisher, OnJokerActivate) = eventFactory.CreateEvent<(Joker, int)>();
            (_jokerSelectionPublisher, OnJokerSelection) = eventFactory.CreateEvent<JokerSelection>();
            (_jokerSelectPublisher, OnJokerSelect) = eventFactory.CreateEvent<Joker>();
            (_scorePublisher, OnScore) = eventFactory.CreateEvent<int>();

            Model = model;
            Service = playerService;
            _disposable = DisposableBag.Create(
                _statePublisher,
                BoxPublisher,
                _rollPublisher,
                _jokerActivatePublisher,
                _jokerSelectPublisher,
                _scorePublisher,
                matchService.OnRoundStart.Subscribe(OnRoundStart),
                playerService.OnJoker.Subscribe(OnPlayerJoker),
                playerService.OnTurn.Subscribe(OnPlayerTurn),
                playerService.OnRoll.Subscribe(OnPlayerRoll),
                playerService.OnMove.Subscribe(OnPlayerMove),
                playerService.OnConfirm.Subscribe(OnPlayerConfirm)
            );
        }

        public void Initialize()
        {
            _statePublisher.Publish(Model.State);
            BoxPublisher.Publish(Model.Tiles);
            _scorePublisher.Publish(Model.Score);
            if (Model.State is PlayerState.Play)
            {
                _rollPublisher.Publish((Model.Rolls, true));
            }
        }

        public void Reset()
        {
            Model.State = PlayerState.Idle;

            for (var i = 0; i < Model.Rolls.Length; i++)
            {
                Model.Rolls[i] = 0;
            }
            
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
            _statePublisher.Publish(Model.State);
            BoxPublisher.Publish(Model.Tiles);
            for (int i = 0; i < roundStart.Choices.Count; i++)
            {
                JokerChoice choice = roundStart.Choices[i];
                if (IsCurrentPlayer(choice.PlayerId))
                {
                    Joker[] jokers = new Joker[choice.Jokers.Count];
                    for (int j = 0; j < choice.Jokers.Count; j++)
                    {
                        jokers[j] = choice.Jokers[j];
                    }
                    _jokerSelectionPublisher.Publish(new JokerSelection { Jokers = jokers });
                    break;
                }
            }
        }
        
        private void OnPlayerJoker(JokerSelect jokerSelect)
        {
            if (!IsCurrentPlayer(jokerSelect.PlayerId))
            {
                return;
            }
            _jokerSelectPublisher.Publish(jokerSelect.Selected);
        }

        private void OnPlayerTurn(PlayerTurn playerTurn)
        {
            if (!IsCurrentPlayer(playerTurn.PlayerId))
            {
                return;
            }
            SetState(PlayerState.Roll);
        }

        private async void OnPlayerRoll(PlayerRoll playerRoll)
        {
            if (!IsCurrentPlayer(playerRoll.PlayerId))
            {
                return;
            }

            for (int i = 0; i < Model.Rolls.Length; i++)
            {
                Model.Rolls[i] = playerRoll.Rolls[i];
            }
            
            await _rollPublisher.PublishAsync((Model.Rolls, false));
            SetState(HasMoves() ? PlayerState.Play : PlayerState.Fail);
        }

        private void OnPlayerMove(PlayerMove playerMove)
        {
            if (!IsCurrentPlayer(playerMove.PlayerId))
            {
                return;
            }
            Model.Tiles[playerMove.Index] = (TileState)playerMove.State;
            BoxPublisher.Publish(Model.Tiles);
        }

        private void OnPlayerConfirm(PlayerConfirm playerConfirm)
        {
            if (!IsCurrentPlayer(playerConfirm.PlayerId))
            {
                return;
            }

            for (int i = 0; i < Model.Tiles.Length; i++)
            {
                Model.Tiles[i] = (TileState)playerConfirm.Tiles[i];
            }

            for (int i = 0; i < playerConfirm.Jokers.Count; i++)
            {
                JokerScore jokerScore = playerConfirm.Jokers[i];
                if (jokerScore.Joker == Joker.None)
                {
                    continue;
                }
                Debug.Log($"Joker score: {jokerScore.Score}");
                _jokerActivatePublisher.Publish((jokerScore.Joker, jokerScore.Score));
            }

            Model.Score += playerConfirm.Score;
            BoxPublisher.Publish(Model.Tiles);
            _scorePublisher.Publish(Model.Score);
            SetState(PlayerState.Idle);
        }

        private bool IsCurrentPlayer(string playerId)
        {
            return string.Equals(playerId, Model.PlayerId);
        }
        
        private bool HasMoves() {
            return CanMakeSum(Model.TotalRoll);
        }
         
        private bool CanMakeSum(int target, int index = 0) {
            if (target == 0)
            {
                return true;
            }
            if (index >= Model.Tiles.Length || target < 0)
            {
                return false;
            }
            int next = index + 1;
            int value = index + 1;
            return (Model.Tiles[index] is TileState.Open && CanMakeSum(target - value, next)) || CanMakeSum(target, next);
        }

        protected void SetState(PlayerState state)
        {
            if (Model.State == state)
                return;
            Model.State = state;
            _statePublisher.Publish(Model.State);
        }
    }
}
