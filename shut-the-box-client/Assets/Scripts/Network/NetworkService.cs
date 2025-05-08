
namespace Network
{
    using System;
    using Cysharp.Threading.Tasks;
    using MessagePipe;
    using Nakama;
    using R3;
    using Revel.Native;
    using VContainer;
    using ILogger = Revel.Diagnostics.ILogger;
#if UNITY_EDITOR
    using ParrelSync;
#endif

    public interface INetworkService
    {
        string PlayerId { get; }
        ReactiveProperty<bool> Connected { get; }
        UniTask ConnectAsync();
    }

    public class NetworkService : INetworkService, IDisposable
    {
        private const string SessionPrefName = "nakama.session";

        public string PlayerId { get; private set; }
        public ISocket Socket { get; }
        public ReactiveProperty<bool> Connected { get; } = new();

        [Inject]
        public IPublisher<string, IMatchState> MatchStatePublisher;

        private readonly ILogger _logger;
        private readonly INative _native;
        private readonly IClient _client;

        private ISession _session;
        private IDisposable _disposable;

        public NetworkService(INetworkSettings settings, ILogger logger, INative native)
        {
            _logger = logger;
            _native = native;
            _client = new Client(
                settings.Scheme,
                settings.Host,
                settings.Port,
                settings.Key,
                UnityWebRequestAdapter.Instance
            );
            Socket = _client.NewSocket();

            Socket.Connected += SocketConnected;
            Socket.Closed += SocketClosed;
            Socket.ReceivedError += SocketError;
            Socket.ReceivedMatchState += StateReceived;
        }

        public async UniTask ConnectAsync()
        {
            var authToken = _native.GetKey(SessionPrefName);

            if (!string.IsNullOrEmpty(authToken))
            {
                ISession session = Session.Restore(authToken);
                if (!session.IsExpired)
                {
                    _session = session;
                }
            }

            if (_session == null)
            {
#if UNITY_EDITOR
                string deviceId = ClonesManager.IsClone()
                    ? "ParallelSyncClone"
                    : _native.GetUniqueID();
#else
                string deviceId = _native.GetUniqueID();
#endif
                _session = await _client.AuthenticateDeviceAsync(deviceId);
                _native.SetKey(SessionPrefName, _session.AuthToken);
            }

            if (_session.IsExpired)
            {
                await _client.SessionRefreshAsync(_session);
            }

            if (!Socket.IsConnected && !Socket.IsConnecting)
            {
                await Socket.ConnectAsync(_session);
            }

            PlayerId = _session.UserId;
        }

        public void Dispose()
        {
            Socket.CloseAsync();
            _disposable?.Dispose();
        }

        private async void SocketConnected()
        {
            await UniTask.SwitchToMainThread();
            _logger.Info($"Socket Connected: {_session.UserId}");
            Connected.Value = true;
        }

        private async void SocketClosed()
        {
            await UniTask.SwitchToMainThread();
            _logger.Info($"Socket Disconnected: {_session.UserId}");
            Connected.Value = false;
        }

        private async void StateReceived(IMatchState obj)
        {
            await UniTask.SwitchToMainThread();
            MatchStatePublisher.Publish(obj.MatchId, obj);
        }

        private async void SocketError(Exception error)
        {
            await UniTask.SwitchToMainThread();
            _logger.Error(error.ToString());
        }
    }
}
