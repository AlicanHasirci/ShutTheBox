namespace UI {
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using Revel.UI.Util;
    using TMPro;
    using UnityEngine;
    using Utility;

    public class Toast : MonoBehaviour, ObjectPool<Toast>.IPooledObject {
        [SerializeField, OwnComponent(true)] private TMP_Text _text;

        public float fadeInTime = .25f;
        public float fadeOutTime = .25f;
        public float moveDistance = 100;
        public AnimationCurve scaleCurve;
        public AnimationCurve fadeCurve;
        public float moveTime = 1;
        public AnimationCurve ease = AnimationCurve.Linear(0, 0, 1, 1);
        public float duration = 1;
        
        public ObjectPool<Toast> Pool { get; set; }

        public string Text {
            get => _text.text;
            set => _text.text = value;
        }

        private void Awake() {
            _text = GetComponent<TMP_Text>();
        }
        
        public void OnReturn()
        {
        }
		
        public UniTask ScrollText(string text, Vector2 position) {
            RectTransform rectTransform = (RectTransform) transform;
            gameObject.SetActive(true);
            _text.text = text;
            Vector2 canvasSize = ((RectTransform) _text.canvas.rootCanvas.transform).sizeDelta;
            Rect textRect = _text.GetPixelAdjustedRect();
            textRect.center = position;
            Vector2 offset = new(
                Mathf.Min(textRect.xMin, 0) + Mathf.Max(textRect.xMax - canvasSize.x, 0),
                Mathf.Min(textRect.yMin, 0) + Mathf.Max(textRect.yMax - canvasSize.y, 0)
            );
            rectTransform.anchoredPosition = position - offset;
            rectTransform.localScale = Vector3.zero;
            DOTween.Kill(this);
            return DOTween.Sequence().SetId(this)
                .Append(rectTransform.DOScale(1, moveTime).SetEase(scaleCurve))
                .Join(rectTransform.DOShakeRotation(moveTime, 45f))
                .SetEase(ease).ToUniTask();
        }

        public void FloatingText(string text, Vector3 position, bool keepInBounds) {
            var rectTransform = (RectTransform) transform;
            gameObject.SetActive(true);
            _text.text = text;
            var textRect = _text.GetPixelAdjustedRect();
            textRect.center = position;
            _text.color = _text.color.SetAlpha(0);
            rectTransform.position = position;
            rectTransform.localScale = Vector3.one;

            if(keepInBounds) {
                var canvasWidth = ((RectTransform)_text.canvas.rootCanvas.transform).sizeDelta.x;
                var distRight = canvasWidth - (rectTransform.anchoredPosition.x + rectTransform.rect.width / 2);
                var distLeft = rectTransform.anchoredPosition.x - rectTransform.rect.width / 2;
                if (distRight < 0) {
                    rectTransform.anchoredPosition += new Vector2(distRight, 0);
                }
                if (distLeft < 0) {
                    rectTransform.anchoredPosition += new Vector2(-distLeft, 0);
                }
            }

            DOTween.Complete(this);
            DOTween.Sequence().SetId(this)
                .Append(_text.DOFade(1, fadeInTime))
                .Append(rectTransform.DOAnchorPosY(moveDistance, moveTime).SetRelative(true))
                .Append(_text.DOFade(0, fadeOutTime))
                .SetEase(ease)
                .OnComplete(() => Pool.Return(this))
                .OnKill(() => Pool.Return(this));
        }

        public UniTask FloatingText(string text, Vector2 position) {
            _text.text = text;
            RectTransform rectTransform = (RectTransform) transform;
            Rect textRect = _text.GetPixelAdjustedRect();
            textRect.center = position;
            _text.color = _text.color.SetAlpha(0);
            rectTransform.anchoredPosition = position;
            rectTransform.localScale = Vector3.one;
            return DOTween.Sequence().SetId(this)
                .Append(_text.DOFade(1, fadeInTime))
                .Append(rectTransform.DOAnchorPosY(moveDistance, moveTime).SetRelative(true))
                .Append(_text.DOFade(0, fadeOutTime))
                .SetEase(ease).ToUniTask();
        }

        public async UniTask FloatingTextAlternative(string text, Vector2 position) {
            var rectTransform = (RectTransform) transform;
            gameObject.SetActive(true);
            _text.text = text;
            var textRect = _text.GetPixelAdjustedRect();
            textRect.center = position;
            _text.color = _text.color.SetAlpha(0);
            rectTransform.anchoredPosition = position;
            rectTransform.localScale = Vector3.one;
            DOTween.Complete(this);

            rectTransform.DOAnchorPosY(moveDistance, moveTime).SetRelative(true).SetEase(ease);
            float t = 0;
            while (t < 1) {
                t += Time.deltaTime / duration;
                t = Mathf.Clamp01(t);
                _text.alpha = fadeCurve.Evaluate(t);
                rectTransform.localScale = Vector3.one * scaleCurve.Evaluate(t);
                await UniTask.Yield(cancellationToken: this.GetCancellationTokenOnDestroy());
            }
            Pool.Return(this);
        }

        private void OnDestroy() {
            DOTween.Complete(this);
        }
    }
}