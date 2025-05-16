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
    using Player.Jokers;
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
        private PlayerUI[] _playersUIs;
        
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
            
            for (int i = 0; i < _playersUIs.Length; i++)
            {
                int pIndex = (lpp + i) % PlayerPresenters.Count;
                _playersUIs[i].Initialize(PlayerPresenters[pIndex]);
            }

            for (int i = 0; i < _players.Length; i++)
            {
                int pIndex = (lpp + i) % PlayerPresenters.Count;
                _players[i].Initialize(PlayerPresenters[pIndex]);
                PlayerPresenters[pIndex].Initialize();
            }

            _disposable = DisposableBag.Create(
                MatchPresenter.OnMatchResult.Subscribe(OnMatchResult),
                MatchPresenter.OnJokerSelection.Subscribe(OnJokerSelection)
            );
            MatchPresenter.PlayerReady();
        }

        private void OnJokerSelection(JokerSelection obj)
        {
            _popupManager.Show<JokerSelectionUI, JokerSelection>(obj);
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
            for (int i = 0; i < roundStart.Choices.Count; i++)
            {
                JokerChoice choice = roundStart.Choices[i];
                // if (choice.PlayerId.Equals())
            }

            // roundStart.Choices
        }

        public void Exit()
        {
            MatchPresenter.LeaveMatch();
            SceneController.ChangeTopScene("MenuScene").Forget();
        }
    }
}
