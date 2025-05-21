namespace Player
{
    using System;
    using Cysharp.Threading.Tasks;
    using Jokers;
    using MessagePipe;
    using Network;
    using TMPro;
    using UI;
    using UnityEditor;
    using UnityEngine;
    using VContainer;

    public class PlayerUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _scoreText;
        
        [SerializeField] 
        private JokerCardUI _jokerPlayerUI;
        
        [Inject]
        public IJokerDatabase JokerDatabase;
        
        private IDisposable _disposable;

        public void Initialize(IPlayerPresenter presenter)
        {
            _jokerPlayerUI.Initialize(3);
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

        private void OnJokerSelect(Joker joker)
        {
            _jokerPlayerUI.Add(joker, true);
        }

        private void OnJokerActivate((Joker Type, int Score) jokerScore)
        {
            JokerCardBehaviour card = _jokerPlayerUI.Get(jokerScore.Type);
            RectTransform rt = (RectTransform)card.transform;
            card.Activate();
            ToastFactory.Instance.ScoreText(_jokerPlayerUI.transform, rt.anchoredPosition, jokerScore.Score);
        }

        private void OnScore(int score)
        {
            _scoreText.text = score.ToString();
        }
    }
}