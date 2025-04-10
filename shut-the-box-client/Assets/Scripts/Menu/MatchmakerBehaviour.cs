using System;
using Match;
using MessagePipe;
using Popups;
using Revel.SceneManagement;
using UnityEngine;
using VContainer;
using DisposableBag = MessagePipe.DisposableBag;

namespace Menu
{
    public class MatchmakerBehaviour : MonoBehaviour
    {
        [Inject] 
        public IPublisher<bool, InfoPopup.Payload> InfoPopupPublisher;
        
        [Inject]
        public ISceneController SceneController;

        [Inject]
        public IMatchService MatchService;

        private IDisposable _disposable;

        private void Awake()
        {
            _disposable = DisposableBag.Create(
                MatchService.OnMatchState.Subscribe(OnMatchStateChanged)
            );
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private void OnMatchStateChanged(MatchState state)
        {
            if (state is MatchState.Finding)
            {
                InfoPopupPublisher.Publish(true, new InfoPopup.Payload("Matchmaking", "Cancel", CancelMatch));
            }
            if (state is MatchState.Waiting)
            {
                InfoPopupPublisher.Publish(false, default);
                GoToMatch();
            }
        }

        public void FindMatch()
        {
            MatchService.StartMatchmaking();
        }

        private void CancelMatch()
        {
            InfoPopupPublisher.Publish(false, default);
            MatchService.CancelMatchmaking();
        }

        private async void GoToMatch()
        {
            var topScene = string.Empty;
            using var enumerator = SceneController.Scenes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                topScene = enumerator.Current;
            }

            await SceneController.UnloadSceneAsync(topScene);
            await SceneController.LoadSceneAsync("GameScene");
        }
    }
}
