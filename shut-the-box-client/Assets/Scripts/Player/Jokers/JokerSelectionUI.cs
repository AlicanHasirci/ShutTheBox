namespace Player.Jokers
{
    using System;
    using Cysharp.Threading.Tasks;
    using MessagePipe;
    using Network;
    using Revel.UI.Popup;
    using UnityEngine;
    using VContainer;
    using PlayerState = PlayerState;

    public class JokerSelectionUI : PopupBehaviour<JokerSelection>
    {
        [SerializeField]
        private JokerSelectionBehaviour[] _jokers;

        [Inject]
        public ILocalPlayerPresenter LocalPlayerPresenter;

        private IDisposable _subscription;

        private void Awake()
        {
            _subscription = DisposableBag.Create(
                LocalPlayerPresenter.OnState.Subscribe(OnStateReceived)
            );
        }

        private void OnDestroy()
        {
            _subscription?.Dispose();
        }

        private void OnStateReceived(PlayerState obj)
        {
            // if (obj == PlayerState.Roll)
            // {
            //     Hide();
            // }
        }

        public override UniTask OnShow(JokerSelection selection)
        {
            for (int i = 0; i < _jokers.Length; i++)
            {
                JokerSelectionBehaviour jokerSelectionBehaviour = _jokers[i];
                if (i >= selection.Jokers.Length)
                {
                    jokerSelectionBehaviour.gameObject.SetActive(false);
                }
                else
                {
                    Joker joker = selection.Jokers[i];
                    jokerSelectionBehaviour.gameObject.SetActive(true);
                    jokerSelectionBehaviour.Initialize(joker, () => OnClick(joker));
                }
            }
            return base.OnShow(selection);
        }

        private void OnClick(Joker joker)
        {
            LocalPlayerPresenter.Select(joker);
            Hide();
        }
    }
}
