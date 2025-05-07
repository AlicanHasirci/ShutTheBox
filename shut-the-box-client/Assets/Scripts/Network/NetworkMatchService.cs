namespace Network
{
    using System;
    using Cysharp.Threading.Tasks;
    using MessagePipe;
    using Nakama;
    using R3;
    using DisposableBag = MessagePipe.DisposableBag;
    using ILogger = Revel.Diagnostics.ILogger;

    public class NetworkMatchService : IMatchService, IDisposable
    {
        public string MatchId { get; private set; }
        public MatchStart MatchStart { get; private set; }
        public ISubscriber<MatchStart> OnMatchStart { get; }
        public ISubscriber<RoundStart> OnRoundStart { get; }
        public ISubscriber<MatchOver> OnMatchOver { get; }

        private readonly IDisposablePublisher<MatchStart> _matchStart;
        private readonly IDisposablePublisher<RoundStart> _roundStart;
        private readonly IDisposablePublisher<MatchOver> _matchOver;
        
        private readonly ILogger _logger;
        private readonly NetworkService _service;
        private readonly IDisposable _disposable;
        private IMatchmakerTicket _ticket;

        public NetworkMatchService(ILogger logger, NetworkService networkService, EventFactory eventFactory)
        {
            _logger = logger;
            _service = networkService;
            
            (_matchStart, OnMatchStart) = eventFactory.CreateEvent<MatchStart>();
            (_roundStart, OnRoundStart) = eventFactory.CreateEvent<RoundStart>();
            (_matchOver, OnMatchOver) = eventFactory.CreateEvent<MatchOver>();
            
            _disposable = DisposableBag.Create(
                Observable
                    .FromEvent(
                        (Action<IMatchState> a) => networkService.Socket.ReceivedMatchState += a,
                        (Action<IMatchState> a) => networkService.Socket.ReceivedMatchState -= a
                    )
                    .Subscribe(ReceivedMatchState),
                Observable
                    .FromEvent(
                        (Action<IMatchmakerMatched> a) =>
                            networkService.Socket.ReceivedMatchmakerMatched += a,
                        (Action<IMatchmakerMatched> a) =>
                            networkService.Socket.ReceivedMatchmakerMatched -= a
                    )
                    .Subscribe(MatchmakerMatched),
                _matchStart,
                _roundStart,
                _matchOver
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

            _ticket = await _service.Socket.AddMatchmakerAsync("*", 2, 2);
            await UniTask.SwitchToMainThread();
            _logger.Info($"Started matchmaking: {_ticket.Ticket}");
        }

        public async void CancelMatchmaking()
        {
            _logger.Info($"Canceling matchmaking: {_ticket.Ticket}");
            await _service.Socket.RemoveMatchmakerAsync(_ticket);
            _ticket = null;
        }

        public async void LeaveMatch()
        {
            _logger.Info($"Leaving match: {_ticket.Ticket}");
            await _service.Socket.LeaveMatchAsync(MatchId);
        }

        private async void MatchmakerMatched(IMatchmakerMatched matched)
        {
            IMatch match = await _service.Socket.JoinMatchAsync(matched).AsUniTask();
            await UniTask.SwitchToMainThread();
            _logger.Info($"Matched: {match.Id}");
            _ticket = null;
        }

        private async void ReceivedMatchState(IMatchState state)
        {
            await UniTask.SwitchToMainThread();
            OpCode opCode = (OpCode)state.OpCode;
            switch (opCode)
            {
                case OpCode.MatchStart:
                    MatchId = state.MatchId;
                    MatchStart = MatchStart.Parser.ParseFrom(state.State);
                    _matchStart.Publish(MatchStart);
                    break;
                case OpCode.RoundStart:
                    RoundStart roundStart = RoundStart.Parser.ParseFrom(state.State);
                    _roundStart.Publish(roundStart);
                    break;
                case OpCode.MatchOver:
                    MatchOver matchOver = MatchOver.Parser.ParseFrom(state.State);
                    _matchOver.Publish(matchOver);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}