namespace Match
{
    using System;
    using System.Collections.Generic;
    using MessagePipe;
    using Network;
    using Player;
    using Player.Jokers;
    using TileState = Player.TileState;

    public enum MatchEvent
    {
        Searching,
        Canceled,
        Joined,
        Left,
    }

    public enum ResultType
    {
        Tie,
        Win,
        Lose,
    }

    public struct MatchResult
    {
        public ResultType Type;
        public int Player { get; set; }
        public int Opponent { get; set; }
    }

    public interface IMatchPresenter
    {
        MatchModel Model { get; }
        void StartMatchmaking();
        void CancelMatchmaking();
        void LeaveMatch();
        void PlayerReady();
        ISubscriber<MatchEvent> OnMatchEvent { get; }
        ISubscriber<MatchResult> OnMatchResult { get; }
        ISubscriber<JokerSelection> OnJokerSelection { get; }
    }

    public class MatchPresenter : IMatchPresenter, IDisposable
    {
        public MatchModel Model { get; private set; }
        public ISubscriber<MatchEvent> OnMatchEvent { get; }
        public ISubscriber<MatchResult> OnMatchResult { get; }
        public ISubscriber<JokerSelection> OnJokerSelection { get; }

        private readonly INetworkService _networkService;
        private readonly IMatchService _matchService;
        private readonly IDisposablePublisher<MatchEvent> _eventPublisher;
        private readonly IDisposablePublisher<MatchResult> _resultPublisher;
        private readonly IDisposablePublisher<JokerSelection> _jokerPublisher;
        private readonly IDisposable _disposable;

        public MatchPresenter(
            INetworkService networkService,
            IMatchService matchService,
            EventFactory eventFactory
        )
        {
            _networkService = networkService;
            _matchService = matchService;
            (_eventPublisher, OnMatchEvent) = eventFactory.CreateEvent<MatchEvent>();
            (_resultPublisher, OnMatchResult) = eventFactory.CreateEvent<MatchResult>();
            (_jokerPublisher, OnJokerSelection) = eventFactory.CreateEvent<JokerSelection>();
            _disposable = DisposableBag.Create(
                _matchService.OnMatchStart.Subscribe(OnMatchStart),
                _matchService.OnRoundStart.Subscribe(OnRoundStart),
                _matchService.OnMatchOver.Subscribe(OnMatchOver),
                _eventPublisher,
                _resultPublisher,
                _jokerPublisher
            );
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        private void OnMatchStart(MatchStart match)
        {
            Model = new MatchModel
            {
                Players = new List<PlayerModel>(match.Players.Count),
                MatchId = _matchService.MatchId,
                TileCount = match.TileCount,
                TurnTime = match.TurnTime,
                RoundId = 0,
            };

            foreach (Player player in match.Players)
            {
                PlayerModel playerModel = new(
                    player.PlayerId,
                    match.TileCount,
                    player.Rolls.Count,
                    match.RoundCount
                );
                for (int i = 0; i < playerModel.Tiles.Length; i++)
                {
                    playerModel.Tiles[i] = (TileState)player.Tiles[i];
                }
                playerModel.Score = player.Score;
                Model.Players.Add(playerModel);
            }
            _eventPublisher.Publish(MatchEvent.Joined);
        }

        private void OnRoundStart(RoundStart roundStart)
        {
            Model.RoundId = roundStart.RoundId;
            for (int i = 0; i < roundStart.Choices.Count; i++)
            {
                JokerChoice choice = roundStart.Choices[i];
                if (choice.PlayerId.Equals(_networkService.PlayerId))
                {
                    Joker[] jokers = new Joker[choice.Jokers.Count];
                    for (int j = 0; j < choice.Jokers.Count; j++)
                    {
                        jokers[j] = choice.Jokers[j];
                    }
                    _jokerPublisher.Publish(new JokerSelection { Jokers = jokers });
                    break;
                }
            }
        }

        private void OnMatchOver(MatchOver matchOver)
        {
            MatchResult result = new();
            for (int i = 0; i < matchOver.Scores.Count; i++)
            {
                PlayerScore score = matchOver.Scores[i];
                if (score.PlayerId.Equals(_networkService.PlayerId))
                {
                    result.Player = score.Score;
                }
                else
                {
                    result.Opponent = score.Score;
                }
            }

            if (result.Player == result.Opponent)
            {
                result.Type = ResultType.Tie;
            }
            else if (result.Opponent > result.Player)
            {
                result.Type = ResultType.Lose;
            }
            else
            {
                result.Type = ResultType.Win;
            }
            _resultPublisher.Publish(result);
        }

        public void StartMatchmaking()
        {
            _matchService.StartMatchmaking();
            _eventPublisher.Publish(MatchEvent.Searching);
        }

        public void CancelMatchmaking()
        {
            _matchService.CancelMatchmaking();
            _eventPublisher.Publish(MatchEvent.Canceled);
        }

        public void LeaveMatch()
        {
            _matchService.LeaveMatch();
            _eventPublisher.Publish(MatchEvent.Left);
        }

        public void PlayerReady()
        {
            _matchService.PlayerReady();
        }
    }
}
