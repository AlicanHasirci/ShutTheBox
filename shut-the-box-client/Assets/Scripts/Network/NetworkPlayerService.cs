namespace Network
{
    using System;
    using Cysharp.Threading.Tasks;
    using Google.Protobuf;
    using MessagePipe;
    using Nakama;
    using R3;
    using DisposableBag = MessagePipe.DisposableBag;
    using ILogger = Revel.Diagnostics.ILogger;

    public class NetworkPlayerService : IPlayerService, IDisposable
    {
        public ISubscriber<JokerSelect> OnJoker { get; }
        public ISubscriber<PlayerTurn> OnTurn { get; }
        public ISubscriber<PlayerRoll> OnRoll { get; }
        public ISubscriber<PlayerMove> OnMove { get; }
        public ISubscriber<PlayerConfirm> OnConfirm { get; }

        private readonly ILogger _logger;
        private readonly string _playerId;
        private readonly ISocket _socket;
        private readonly IMatchService _matchService;

        private readonly IDisposablePublisher<JokerSelect> _onJoker;
        private readonly IDisposablePublisher<PlayerTurn> _onTurn;
        private readonly IDisposablePublisher<PlayerRoll> _onRoll;
        private readonly IDisposablePublisher<PlayerMove> _onMove;
        private readonly IDisposablePublisher<PlayerConfirm> _onConfirm;

        private readonly IDisposable _disposable;

        public NetworkPlayerService(
            ILogger logger,
            IMatchService matchService,
            NetworkService networkService,
            EventFactory eventFactory
        )
        {
            _logger = logger;
            _matchService = matchService;
            _socket = networkService.Socket;
            _playerId = networkService.PlayerId;

            (_onJoker, OnJoker) = eventFactory.CreateEvent<JokerSelect>();
            (_onTurn, OnTurn) = eventFactory.CreateEvent<PlayerTurn>();
            (_onRoll, OnRoll) = eventFactory.CreateEvent<PlayerRoll>();
            (_onMove, OnMove) = eventFactory.CreateEvent<PlayerMove>();
            (_onConfirm, OnConfirm) = eventFactory.CreateEvent<PlayerConfirm>();

            _disposable = DisposableBag.Create(
                Observable
                    .FromEvent(
                        // ReSharper disable once RedundantLambdaParameterType
                        (Action<IMatchState> a) => networkService.Socket.ReceivedMatchState += a,
                        // ReSharper disable once RedundantLambdaParameterType
                        (Action<IMatchState> a) => networkService.Socket.ReceivedMatchState -= a
                    )
                    .Subscribe(ReceivedMatchState),
                _onJoker,
                _onTurn,
                _onRoll,
                _onMove,
                _onConfirm
            );
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        private async void ReceivedMatchState(IMatchState state)
        {
            await UniTask.SwitchToMainThread();
            OpCode opCode = (OpCode)state.OpCode;
            _logger.Debug($"Received match state: {opCode}");
            switch (opCode)
            {
                case OpCode.PlayerSelect:
                    JokerSelect jokerSelect = JokerSelect.Parser.ParseFrom(state.State);
                    _onJoker.Publish(jokerSelect);
                    break;
                case OpCode.PlayerTurn:
                    PlayerTurn playerTurn = PlayerTurn.Parser.ParseFrom(state.State);
                    _onTurn.Publish(playerTurn);
                    break;
                case OpCode.PlayerRoll:
                    PlayerRoll playerRoll = PlayerRoll.Parser.ParseFrom(state.State);
                    _onRoll.Publish(playerRoll);
                    break;
                case OpCode.PlayerMove:
                    PlayerMove playerMove = PlayerMove.Parser.ParseFrom(state.State);
                    _onMove.Publish(playerMove);
                    break;
                case OpCode.PlayerConf:
                    PlayerConfirm playerConfirm = PlayerConfirm.Parser.ParseFrom(state.State);
                    _onConfirm.Publish(playerConfirm);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Select(Joker joker)
        {
            JokerSelect select = new() { Selected = joker };
            _socket.SendMatchStateAsync(
                _matchService.MatchId,
                (long)OpCode.PlayerSelect,
                select.ToByteArray()
            );
        }

        public void Roll()
        {
            _socket.SendMatchStateAsync(
                _matchService.MatchId,
                (long)OpCode.PlayerRoll,
                string.Empty
            );
        }

        public void Toggle(int index)
        {
            _logger.Info($"Player toggled {index}.");
            PlayerMove move = new() { PlayerId = _playerId, Index = index };
            _socket.SendMatchStateAsync(
                _matchService.MatchId,
                (long)OpCode.PlayerMove,
                move.ToByteArray()
            );
        }

        public void Confirm()
        {
            _logger.Info("Player confirmed.");
            _socket.SendMatchStateAsync(
                _matchService.MatchId,
                (long)OpCode.PlayerConf,
                string.Empty
            );
        }

        public void Done()
        {
            _logger.Info("Player done.");
            _socket.SendMatchStateAsync(
                _matchService.MatchId,
                (long)OpCode.PlayerFail,
                string.Empty
            );
        }
    }
}
