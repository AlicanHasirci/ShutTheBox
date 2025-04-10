using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Player.Box;
using Player.Dice;
using R3;
using UnityEngine;
using DisposableBag = MessagePipe.DisposableBag;

namespace Player
{
    public class PlayerBehaviour : MonoBehaviour, IDisposable
    {
        [SerializeField]
        private BoxBehaviour _boxBehaviour;

        [SerializeField]
        private DiceManager _diceManager;

        private IDisposable _disposable;

        public void Initialize(IPlayerPresenter presenter)
        {
            _disposable = DisposableBag.Create(
                presenter.OnState.Subscribe(StateChange),
                presenter.OnRoll.Subscribe(RollReceived),
                presenter.OnBox.Subscribe(_boxBehaviour.SetState),
                _boxBehaviour.OnClick.Subscribe(presenter.TileToggle)
            );
        }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        private void StateChange(PlayerState state)
        {
            if (state is not PlayerState.Roll)
                return;
            _diceManager.ResetDice();
            _diceManager.CacheRollData();
        }

        private async UniTask RollReceived((int value, bool skip) roll, CancellationToken token = default)
        {
            await _diceManager.RollDice(roll.value, roll.skip, token);
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
