namespace Match
{
    using System;
    using System.Collections.Generic;
    using MessagePipe;
    using Network;
    using Player;
    using TileState = Player.TileState;

    public enum MatchEvent
    {
        Searching,
        Canceled,
        Joined,
        Left
    }

    public interface IMatchPresenter
    {
        MatchModel Model { get; }
        void StartMatchmaking();
        void CancelMatchmaking();
        void LeaveMatch();
        ISubscriber<MatchEvent> OnMatchEvent { get; }
    }
    
    public class MatchPresenter : IMatchPresenter, IDisposable
    {
        public MatchModel Model { get; private set; }
        public ISubscriber<MatchEvent> OnMatchEvent { get; }
        
        private readonly IMatchService _matchService;
        private readonly IPublisher<MatchEvent> _publisher;
        private readonly IDisposable _disposable;

        public MatchPresenter(IMatchService matchService, ISubscriber<MatchEvent> subscriber, IPublisher<MatchEvent> publisher)
        {
            _matchService = matchService;
            OnMatchEvent = subscriber;
            _publisher = publisher;
            _disposable = DisposableBag.Create(
                _matchService.OnMatchStart.Subscribe(OnMatchStart),
                _matchService.OnRoundStart.Subscribe(OnRoundStart),
                _matchService.OnMatchOver.Subscribe(OnMatchOver)
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
                Rounds = new List<RoundModel>(match.RoundCount),
                MatchId = _matchService.MatchId,
                TileCount = match.TileCount,
                TurnTime = match.TurnTime,
            };
            
            foreach (Player player in match.Players)
            {
                PlayerModel playerModel = new(player.PlayerId, match.TileCount);
                for (int i = 0; i < playerModel.Tiles.Length; i++)
                {
                    playerModel.Tiles[i] = (TileState)player.Tiles[i]; 
                }
                Model.Players.Add(playerModel);
            }
            _publisher.Publish(MatchEvent.Joined);
        }

        private void OnRoundStart(RoundStart roundStart)
        {
            Model.Rounds.Add(new RoundModel(roundStart.Score.Players, roundStart.Score.Scores));
        }

        private void OnMatchOver(MatchOver matchOver)
        {
            
        }

        public void StartMatchmaking()
        {
            _matchService.StartMatchmaking();
            _publisher.Publish(MatchEvent.Searching);
        }

        public void CancelMatchmaking()
        {
            _matchService.CancelMatchmaking();
            _publisher.Publish(MatchEvent.Canceled);
        }

        public void LeaveMatch()
        {
            _matchService.LeaveMatch();
            _publisher.Publish(MatchEvent.Left);
        }
    }
}