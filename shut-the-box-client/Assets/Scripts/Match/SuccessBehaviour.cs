using Cysharp.Threading.Tasks;
using DG.Tweening;
using Revel.UI.Util;
using Sirenix.OdinInspector;
using TextTween;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Match
{
    using DG.DemiEditor;

    public class SuccessBehaviour : MonoBehaviour
    {
        [SerializeField] 
        private Image _background;
        
        [SerializeField] 
        private TweenManager _tweenManager;
        
        [SerializeField] 
        private ParticleSystem[] _fireworks;

        private void Awake()
        {
            gameObject.SetActive(false);
            Color color = _background.color;
            color.a = 0;
            _background.color = color;
            _tweenManager.Progress = 0;
        }

        [Button]
        public async UniTask Show()
        {
            gameObject.SetActive(true);
            _background.color = _background.color.SetAlpha(0); 
            await _background.DOFade(1, .25f).ToUniTask();
            DOTween.To(() => _tweenManager.Progress, p => _tweenManager.Progress = p, 1f, .5f);
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