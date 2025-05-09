using System;
using System.Collections.Generic;
using MessagePipe;
using Player;
using Revel.SceneManagement;
using UnityEngine;

namespace Match
{
    using Cysharp.Threading.Tasks;
    using Network;
    using Popups;
    using VContainer;
    using Revel.UI.Popup;
    using Utility;

    public class MatchManager : MonoBehaviour
    {
        [SerializeField] 
        private PopupManager _popupManager;
        
        [SerializeField]
        private PlayerBehaviour[] _players;
        
        [SerializeField] 
        private TableInfoBehaviour _tableInfo;

        [Inject] 
        public IMatchPresenter MatchPresenter;
        
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
                MatchService.OnRoundStart.Subscribe(OnRoundStart),
                MatchPresenter.OnMatchResult.Subscribe(OnMatchResult)
            );
        }

        private void OnMatchResult(MatchResult result)
        {
            _popupManager.Show<MatchResultPopup, MatchResult>(result);
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private void OnRoundStart(RoundStart roundStart)
        {
            _tableInfo.SetTableInfo($"Round {MatchPresenter.Model.RoundId + 1}\nStarting..", 5);
        }

        public void Exit()
        {
            MatchPresenter.LeaveMatch();
            SceneController.ChangeTopScene("MenuScene").Forget();
        }
    }
}
