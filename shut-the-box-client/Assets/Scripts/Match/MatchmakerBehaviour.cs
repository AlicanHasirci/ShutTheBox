namespace Match
{
    using System;
    using MessagePipe;
    using Popups;
    using Revel.SceneManagement;
    using UnityEngine;
    using Utility;
    using VContainer;

    public class MatchmakerBehaviour : MonoBehaviour
    {
        [Inject] 
        public IPublisher<bool, InfoPopup.Payload> InfoPopupPublisher;
        
        [Inject]
        public ISceneController SceneController;

        [Inject]
        public IMatchPresenter MatchPresenter;

        private IDisposable _disposable;

        private void Awake()
        {
            _disposable = DisposableBag.Create(
                MatchPresenter.OnMatchEvent.Subscribe(OnMatchEvent)
            );
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private void OnMatchEvent(MatchEvent matchEvent)
        {
            switch (matchEvent)
            {
                case MatchEvent.Searching:
                    InfoPopupPublisher.Publish(true, new InfoPopup.Payload("Matchmaking", "Cancel", CancelMatch));
                    break;
                case MatchEvent.Canceled:
                    InfoPopupPublisher.Publish(false, default);
                    break;
                case MatchEvent.Joined:
                    InfoPopupPublisher.Publish(false, default);
                    _ = SceneController.ChangeTopScene("GameScene");
                    break;
            }
        }

        public void FindMatch()
        {
            MatchPresenter.StartMatchmaking();
        }

        private void CancelMatch()
        {
            MatchPresenter.CancelMatchmaking();
        }
    }
}
