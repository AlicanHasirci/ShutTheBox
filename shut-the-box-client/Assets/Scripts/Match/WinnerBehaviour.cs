namespace Match
{
    using DG.Tweening;
    using Revel.UI.Util;
    using UnityEngine;
    using UnityEngine.UI;
    using Random = UnityEngine.Random;
    using TextTween;
    using Utility;

    public class WinnerBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private Image _background;

        [SerializeField] 
        private TextTweenManager _tweenManager;
        
        [SerializeField] 
        private ParticleSystem[] _fireworks;

        private void Awake()
        {
            Color color = _background.color;
            color.a = 0;
            _background.color = color;
        }

        public async void OnEnable()
        {
            _tweenManager.Progress = 0;
            _background.color = _background.color.SetAlpha(0); 
            await _background.DOFade(.9f, .25f).ToUniTask();
            DOTween.To(() => _tweenManager.Progress, p => _tweenManager.Progress = p, 1, 2f);
            foreach (ParticleSystem firework in _fireworks)
            {
                ParticleSystem.MainModule fireworkMain = firework.main;
                float delay = Random.Range(.1f, 1f);
                fireworkMain.startDelay = delay;
                firework.Play();
            }
        }
    }
}