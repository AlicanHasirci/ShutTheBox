namespace Player
{
    using System;
    using Jokers;
    using MessagePipe;
    using Network;
    using TMPro;
    using UnityEngine;
    using VContainer;

    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _scoreText;
        
        [SerializeField] 
        private JokerPlayerUI _jokerPlayerUI;
        
        [Inject]
        public IJokerDatabase JokerDatabase;
        
        private IDisposable _disposable;

        public void Initialize(IPlayerPresenter presenter)
        {
            Debug.Log("PlayerUI initialized");
            _disposable = DisposableBag.Create(
                presenter.OnScore.Subscribe(OnScore),
                presenter.OnJokerActivate.Subscribe(OnJokerActivate),
                presenter.OnJokerSelect.Subscribe(OnJokerSelect)
            );
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private void OnJokerSelect(Joker obj)
        {
        }

        private void OnJokerActivate((Joker, int) obj)
        {
        }

        private void OnScore(int score)
        {
            Debug.Log("On Score");
            _scoreText.text = score.ToString();
        }
    }
}