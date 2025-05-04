using System;
using System.Collections.Generic;
using Api;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Match;
using MessagePipe;
using Nakama;
using Player;
using R3;
using DisposableBag = MessagePipe.DisposableBag;
using ILogger = Revel.Diagnostics.ILogger;
using TileState = Player.TileState;

namespace Network
{
    public class NetworkPlayerService : IPlayerService, IDisposable
    {
        public ISubscriber<int> OnRoundStart { get; }
        public ISubscriber<string> OnTurn { get; }
        public ISubscriber<(string, int)> OnRoll { get; }
        public ISubscriber<(string, int, TileState)> OnMove { get; }
        public ISubscriber<(string, IReadOnlyList<TileState>)> OnConfirm { get; }

        private readonly ILogger _logger;
        private readonly string _playerId;
        private readonly ISocket _socket;
        private readonly IMatchService _matchService;

        private readonly IDisposablePublisher<int> _onRoundStart;
        private readonly IDisposablePublisher<string> _onTurn;
        private readonly IDisposablePublisher<(string, int)> _onRoll;
        private readonly IDisposablePublisher<(string, int, TileState)> _onMove;
        private readonly IDisposablePublisher<(string, IReadOnlyList<TileState>)> _onConfirm;

        private readonly List<TileState> _tileStates = new();
        private readonly IDisposable _disposable;

        private string MatchId => _matchService?.Model?.MatchId;

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

            (_onRoundStart, OnRoundStart) = eventFactory.CreateEvent<int>();
            (_onTurn, OnTurn) = eventFactory.CreateEvent<string>();
            (_onRoll, OnRoll) = eventFactory.CreateEvent<(string, int)>();
            (_onMove, OnMove) = eventFactory.CreateEvent<(string, int, TileState)>();
            (_onConfirm, OnConfirm) = eventFactory.CreateEvent<(string, IReadOnlyList<TileState>)>();

            _disposable = DisposableBag.Create(
                Observable
                    .FromEvent(
                        // ReSharper disable once RedundantLambdaParameterType
                        (Action<IMatchState> a) => networkService.Socket.ReceivedMatchState += a,
                        // ReSharper disable once RedundantLambdaParameterType
                        (Action<IMatchState> a) => networkService.Socket.ReceivedMatchState -= a
                    )
                    .Subscribe(ReceivedMatchState),
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
                case OpCode.RoundStart:
                    RoundStart roundStart = RoundStart.Parser.ParseFrom(state.State);
                    _onRoundStart.Publish(roundStart.Interval);
                    break;
                case OpCode.PlayerTurn:
                    PlayerTurn playerTurn = PlayerTurn.Parser.ParseFrom(state.State);
                    _onTurn.Publish(playerTurn.PlayerId);
                    break;
                case OpCode.PlayerRoll:
                    PlayerRoll playerRoll = PlayerRoll.Parser.ParseFrom(state.State);
                    _onRoll.Publish((playerRoll.PlayerId, playerRoll.Roll));
                    break;
                case OpCode.PlayerMove:
                    PlayerMove playerMove = PlayerMove.Parser.ParseFrom(state.State);
                    _onMove.Publish((playerMove.PlayerId, playerMove.Index, (TileState)(int)playerMove.State));
                    break;
                case OpCode.PlayerConf:
                    PlayerConfirm playerConfirm = PlayerConfirm.Parser.ParseFrom(state.State);
                    _onConfirm.Publish((playerConfirm.PlayerId, ConvertTileStates(playerConfirm.Tiles)));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Ready()
        {
            _socket.SendMatchStateAsync(MatchId, (long)OpCode.PlayerReady, string.Empty);
        }

        public void Roll()
        {
            _socket.SendMatchStateAsync(MatchId, (long)OpCode.PlayerRoll, string.Empty);
        }

        public void Toggle(int index)
        {
            _logger.Info($"Player toggled {index}.");
            PlayerMove move = new() { PlayerId = _playerId, Index = index };
            _socket.SendMatchStateAsync(MatchId, (long)OpCode.PlayerMove, move.ToByteArray());
        }

        public void Confirm()
        {
            _logger.Info("Player confirmed.");
            _socket.SendMatchStateAsync(MatchId, (long)OpCode.PlayerConf, string.Empty);
        }

        public void Done()
        {
            _logger.Info("Player done.");
            _socket.SendMatchStateAsync(MatchId, (long)OpCode.PlayerFail, string.Empty);
        }

        private IReadOnlyList<TileState> ConvertTileStates(IList<Api.TileState> tileStates)
        {
            _tileStates.Clear();
            foreach (var state in tileStates)
            {
                _tileStates.Add((TileState)state);
            }

            return _tileStates;
        }
    }
}