using System;
using System.Collections.Generic;
using MessagePipe;
using Player;
using Revel.SceneManagement;
using UnityEngine;
using VContainer;

namespace Match
{
    public class MatchManager : MonoBehaviour
    {
        [SerializeField]
        private PlayerBehaviour[] _players;

        [Inject]
        public IReadOnlyList<IPlayerPresenter> PlayerPresenters;

        [Inject]
        public ISceneController SceneController;

        [Inject]
        public IMatchService MatchService;

        private IDisposable _disposable;
        
        private void Start()
        {
            int lpp = -1;
            for (int i = 0; i < PlayerPresenters.Count; i++)
            {
                if (PlayerPresenters[i] is not LocalPlayerPresenter)
                    continue;
                lpp = i;
                break;
            }

            for (int i = 0; i < _players.Length; i++)
            {
                int pIndex = (lpp + i) % PlayerPresenters.Count;
                _players[i].Initialize(PlayerPresenters[pIndex]);
                PlayerPresenters[pIndex].Initialize();
            }

            _disposable = DisposableBag.Create(
                MatchService.OnMatchState.Subscribe(OnMatchState)
            );
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private async void OnMatchState(MatchState state)
        {
            string topScene = string.Empty;
            using IEnumerator<string> enumerator = SceneController.Scenes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                topScene = enumerator.Current;
            }

            await SceneController.UnloadSceneAsync(topScene);
            await SceneController.LoadSceneAsync("MenuScene");
        }

        public void Exit()
        {
            MatchService.LeaveMatch();
        }
    }
}
