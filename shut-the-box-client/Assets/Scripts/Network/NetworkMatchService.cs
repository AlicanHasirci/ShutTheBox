using System;
using Api;
using Cysharp.Threading.Tasks;
using Match;
using MessagePipe;
using Nakama;
using Player;
using R3;
using DisposableBag = MessagePipe.DisposableBag;
using ILogger = Revel.Diagnostics.ILogger;
namespace Network
{

    public class NetworkMatchService : IMatchService, IDisposable
    {
        public MatchModel Model { get; private set; }
        public ISubscriber<MatchState> OnMatchState { get; }
        public ISubscriber<int> OnRoundStart { get; }

        private readonly IDisposablePublisher<MatchState> _matchState;
        private readonly IDisposablePublisher<int> _roundStart;
        
        private readonly ILogger _logger;
        private readonly NetworkService _service;
        private readonly IDisposable _disposable;
        private IMatchmakerTicket _ticket;

        public NetworkMatchService(ILogger logger, NetworkService networkService, EventFactory eventFactory)
        {
            _logger = logger;
            _service = networkService;
            
            (_matchState, OnMatchState) = eventFactory.CreateEvent<MatchState>();
            (_roundStart, OnRoundStart) = eventFactory.CreateEvent<int>();
            
            _disposable = DisposableBag.Create(
                Observable
                    .FromEvent(
                        // ReSharper disable once RedundantLambdaParameterType
                        (Action<IMatchState> a) => networkService.Socket.ReceivedMatchState += a,
                        // ReSharper disable once RedundantLambdaParameterType
                        (Action<IMatchState> a) => networkService.Socket.ReceivedMatchState -= a
                    )
                    .Subscribe(ReceivedMatchState),
                Observable
                    .FromEvent(
                        // ReSharper disable once RedundantLambdaParameterType
                        (Action<IMatchmakerMatched> a) =>
                            networkService.Socket.ReceivedMatchmakerMatched += a,
                        // ReSharper disable once RedundantLambdaParameterType
                        (Action<IMatchmakerMatched> a) =>
                            networkService.Socket.ReceivedMatchmakerMatched -= a
                    )
                    .Subscribe(MatchmakerMatched),
                _matchState
            );
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        public async void StartMatchmaking()
        {
            if (_ticket != null)
            {
                CancelMatchmaking();
            }

            _matchState.Publish(MatchState.Finding);
            _ticket = await _service.Socket.AddMatchmakerAsync("*", 2, 2);
        }

        public async void CancelMatchmaking()
        {
            await _service.Socket.RemoveMatchmakerAsync(_ticket);

            _matchState.Publish(MatchState.Idle);
            _ticket = null;
        }

        public async void LeaveMatch()
        {
            await _service.Socket.LeaveMatchAsync(Model.MatchId);
        }

        private async void ReceivedMatchState(IMatchState state)
        {
            await UniTask.SwitchToMainThread();
            OpCode opCode = (OpCode)state.OpCode;
            switch (opCode)
            {
                case OpCode.MatchStart:
                    MatchStart match = MatchStart.Parser.ParseFrom(state.State);
                    Model = new MatchModel
                    {
                        Players = new PlayerModel[match.Players.Count],
                        MatchId = state.MatchId,
                        RoundCount = match.RoundCount,
                        TileCount = match.TileCount,
                        TurnTime = match.TurnTime,
                    };
                    int i = 0;
                    foreach (Api.Player player in match.Players)
                    {
                        Model.Players[i++] = new PlayerModel(player.PlayerId, match.TileCount);
                    }

                    _matchState.Publish(MatchState.Waiting);
                    break;
                case OpCode.RoundStart:
                    RoundStart roundStart = RoundStart.Parser.ParseFrom(state.State);
                    _roundStart.Publish(roundStart.RoundCount);
                    break;
                case OpCode.MatchOver:
                    _matchState.Publish(MatchState.Idle);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async void MatchmakerMatched(IMatchmakerMatched matched)
        {
            await UniTask.SwitchToMainThread();
            _matchState.Publish(MatchState.Joining);
            await _service.Socket.JoinMatchAsync(matched).AsUniTask();
            _ticket = null;
        }
    }
}