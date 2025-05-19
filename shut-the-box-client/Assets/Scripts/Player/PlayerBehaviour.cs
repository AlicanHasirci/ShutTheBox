namespace Player
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using MessagePipe;
    using Box;
    using Dice;
    using R3;
    using UnityEngine;
    using DisposableBag = MessagePipe.DisposableBag;

    public class PlayerBehaviour : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private BoxBehaviour _boxBehaviour;

        [SerializeField]
        private DiceManager _diceManager;
        
        private IPlayerPresenter _presenter;
        private IDisposable _disposable;

        public void Initialize(IPlayerPresenter presenter)
        {
            _presenter = presenter;
            _disposable = DisposableBag.Create(
                presenter.OnState.Subscribe(StateChange),
                presenter.OnRoll.Subscribe(OnPlayerRoll),
                presenter.OnBox.Subscribe(_boxBehaviour.SetState),
                _boxBehaviour.OnClick.Subscribe(presenter.TileToggle)
            );
        }

        private void StateChange(PlayerState state)
        {
            if (state is not PlayerState.Roll)
                return;
            _diceManager.ResetDice();
            _diceManager.CacheRollData();
        }

        private async UniTask OnPlayerRoll((int[] rolls, bool skip) roll, CancellationToken token = default)
        {
            await _diceManager.RollDice(_presenter.Model.Rolls, roll.skip, token);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }
    }
}
