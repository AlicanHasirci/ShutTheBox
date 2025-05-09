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
    using TMPro;

    public class PlayerBehaviour : MonoBehaviour, IDisposable
    {
        private const string ScoreFormat = "Score: {0}";
        
        [SerializeField]
        private BoxBehaviour _boxBehaviour;

        [SerializeField]
        private TMP_Text _scoreText;

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
                presenter.OnScore.Subscribe(ScoreChange),
                _boxBehaviour.OnClick.Subscribe(presenter.TileToggle)
            );
        }

        private void ScoreChange(int score)
        {
            _scoreText.text = string.Format(ScoreFormat, score);
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

        private async UniTask OnPlayerRoll((int[] rolls, bool skip) roll, CancellationToken token = default)
        {
            await _diceManager.RollDice(_presenter.Model.Rolls, roll.skip, token);
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
