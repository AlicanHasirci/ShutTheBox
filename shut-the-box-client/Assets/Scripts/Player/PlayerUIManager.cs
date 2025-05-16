namespace Player
{
    using System;
    using DG.Tweening;
    using MessagePipe;
    using Dice;
    using Network;
    using R3;
    using Revel.UI;
    using UnityEngine;
    using VContainer;
    using DisposableBag = MessagePipe.DisposableBag;

    public class PlayerUIManager : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _rollPanel;

        [SerializeField]
        private RectTransform _turnPanel;
        
        [SerializeField]
        private RectTransform _failPanel;

        [SerializeField]
        private RevelButton _confirmButton;

        [SerializeField]
        private DiceManager _diceManager;

        [Inject]
        public ILocalPlayerPresenter Presenter;
        private PlayerState _playerState = (PlayerState)(-1);
        private IDisposable _disposable;

        private void Awake()
        {
            _disposable = DisposableBag.Create(
                Presenter.CanConfirm.Subscribe(CanConfirm),
                Presenter.OnState.Subscribe(StateChange)
            );
            _rollPanel.anchoredPosition = Vector2.zero;
            _turnPanel.anchoredPosition = Vector2.zero;
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
            DOTween.Kill(_rollPanel);
            DOTween.Kill(_turnPanel);
            DOTween.Kill(_failPanel);
        }

        private void CanConfirm(bool visible)
        {
            _confirmButton.Interactable = visible;
        }

        private void StateChange(PlayerState state)
        {
            if (state == _playerState)
                return;
            _playerState = state;
            
            SetPanelVisibility(_rollPanel, state is PlayerState.Roll);
            SetPanelVisibility(_turnPanel, state is PlayerState.Play);
            SetPanelVisibility(_failPanel, state is PlayerState.Fail);
        }

        public void Select(Joker joker)
        {
            Presenter.Select(joker);
        }

        public void Confirm()
        {
            Presenter.Confirm();
        }

        public void Roll()
        {
            Presenter.Roll();
        }

        public void Done()
        {
            Presenter.Done();
            SetPanelVisibility(_failPanel, false);
        }

        private void SetPanelVisibility(RectTransform panel, bool visible)
        {
            if (visible)
            {
                if (panel.gameObject.activeSelf)
                {
                    return;
                }
                DOTween.Kill(panel);
                panel.gameObject.SetActive(true);
                panel.DOAnchorPosY(panel.sizeDelta.y, .5f).SetEase(Ease.OutBack).SetId(panel);
            } else
            {
                if (!panel.gameObject.activeSelf)
                {
                    return;
                }
                DOTween.Kill(panel);
                panel.DOAnchorPosY(0, .5f).SetEase(Ease.OutBack).SetId(panel)
                    .OnKill(() => panel.gameObject.SetActive(false))
                    .OnComplete(() => panel.gameObject.SetActive(false));
            }
        }
    }
}
