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
                MatchService.OnMatchOver.Subscribe(OnMatchOver),
                MatchService.OnRoundStart.Subscribe(OnRoundStart)
            );
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private void OnMatchOver(MatchOver matchOver)
        {
            InfoPopup.Payload payload = new("Game Over", "Quit", Exit);
            _popupManager.Show<InfoPopup, InfoPopup.Payload>(payload);
        }

        private void OnRoundStart(RoundStart roundStart)
        {
            int count = MatchPresenter.Model.Rounds.Count;
            _tableInfo.SetTableInfo($"Round {count + 1}\nStarting..", roundStart.Interval);
        }

        private void Exit()
        {
            MatchService.LeaveMatch();
            SceneController.ChangeTopScene("MenuScene").Forget();
        }
    }
}
