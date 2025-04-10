using R3;
using VContainer;

namespace Debug
{
    using Match;
    using MessagePipe;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public partial class DebugServices : IMatchService
    {
        public MatchModel Model => _matchModel;
        public ISubscriber<MatchState> OnMatchState { get; private set; }
        
        private IDisposablePublisher<MatchState> _matchStatePublisher;
        
        [SerializeField, BoxGroup("Match", VisibleIf = "@serviceType == ServiceType.Match && enabled")]
        private MatchModel _matchModel;

        [Inject]
        public void MatchService(EventFactory eventFactory)
        {
            (_matchStatePublisher, OnMatchState) = eventFactory.CreateEvent<MatchState>();

            _disposable = Disposable.Combine(_disposable, _matchStatePublisher);
        }

        [PropertySpace]
        [BoxGroup("Match")]
        [Button(ButtonStyle.FoldoutButton)]
        private void PublishMatchState(MatchState state)
        {
            _matchStatePublisher?.Publish(state);
        }
        
        public void StartMatchmaking()
        {
            Debug.Log("DebugServices.MatchService.StartMatchmaking");
            _matchStatePublisher?.Publish(MatchState.Finding);
        }

        public void CancelMatchmaking()
        {
            Debug.Log("DebugServices.MatchService.CancelMatchmaking");
            _matchStatePublisher?.Publish(MatchState.Idle);
        }

        public void LeaveMatch()
        {
            Debug.Log("DebugServices.MatchService.LeaveMatch");
            _matchStatePublisher?.Publish(MatchState.Idle);
        }
    }
}