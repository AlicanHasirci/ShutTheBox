
namespace Player
{
    using Network;
    using System;
    using Jokers;
    using MessagePipe;
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
        protected readonly IDisposablePublisher<PlayerState> StatePublisher;
        protected readonly IDisposablePublisher<TileState[]> BoxPublisher;
        protected readonly IDisposableAsyncPublisher<(int[], bool)> RollPublisher;
        protected readonly IDisposablePublisher<(Joker, int)> JokerActivatePublisher;
        protected readonly IDisposablePublisher<JokerSelection> JokerSelectionPublisher;
        protected readonly IDisposablePublisher<Joker> JokerSelectPublisher;
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
            (RollPublisher, OnRoll) = eventFactory.CreateAsyncEvent< (int[], bool)>();
            (JokerActivatePublisher, OnJokerActivate) = eventFactory.CreateEvent<(Joker, int)>();
            (JokerSelectionPublisher, OnJokerSelection) = eventFactory.CreateEvent<JokerSelection>();
            (JokerSelectPublisher, OnJokerSelect) = eventFactory.CreateEvent<Joker>();
            (ScorePublisher, OnScore) = eventFactory.CreateEvent<int>();

            Model = model;
            Service = playerService;
            _disposable = DisposableBag.Create(
                StatePublisher,
                BoxPublisher,
                RollPublisher,
                JokerActivatePublisher,
                JokerSelectPublisher,
                ScorePublisher,
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
            StatePublisher.Publish(Model.State);
            BoxPublisher.Publish(Model.Tiles);
            ScorePublisher.Publish(Model.Score);
            if (Model.State is PlayerState.Play)
            {
                RollPublisher.Publish((Model.Rolls, true));
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
            StatePublisher.Publish(Model.State);
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
                    JokerSelectionPublisher.Publish(new JokerSelection { Jokers = jokers });
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
            JokerSelectPublisher.Publish(jokerSelect.Selected);
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

            for (int i = 0; i < Model.Rolls.Length; i++)
            {
                Model.Rolls[i] = playerRoll.Rolls[i];
            }
            
            await RollPublisher.PublishAsync((Model.Rolls, false));
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

            for (int i = 0; i < playerConfirm.Jokers.Count; i++)
            {
                JokerScore jokerScore = playerConfirm.Jokers[i];
                JokerActivatePublisher.Publish((jokerScore.Joker, jokerScore.Score));
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
