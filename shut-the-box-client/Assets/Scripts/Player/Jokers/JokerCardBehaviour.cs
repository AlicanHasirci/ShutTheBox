namespace Player.Jokers
{
    using System;
    using DG.Tweening;
    using Network;
    using Revel.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class JokerCardBehaviour
        : MonoBehaviour,
            IPointerDownHandler,
            IPointerUpHandler,
            IPointerExitHandler
    {
        [Serializable]
        public struct SizeChange
        {
            public Vector2 Size;
            public float Duration;
            public Ease Ease;
        }

        [Serializable]
        public struct MoveCard
        {
            public float Duration;
        }

        [Serializable]
        public struct PunchScale
        {
            public Vector3 Punch;
            public float Duration;
        }

        [SerializeField]
        private RevelImage _jokerImage;

        [SerializeField]
        private TMP_Text _jokerName;

        [SerializeField]
        private TMP_Text _jokerDescription;

        [SerializeField]
        private SizeChange _shrink;

        [SerializeField]
        private SizeChange _enlarge;

        [SerializeField]
        private PunchScale _punch;

        [SerializeField]
        private MoveCard _move;

        private JokerModel _model;
        private bool _animating;
        private JokerCardUI _cardUI;
        private RectTransform _rectTransform;

        public Joker Type => _model.Type;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
        }

        public void Initialize(JokerModel model, JokerCardUI cardUI)
        {
            _model = model;
            _cardUI = cardUI;
            _jokerImage.Sprite = model.Icon;
            _jokerName.text = model.Name;
            _jokerDescription.text = model.Description;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _cardUI.Select(this);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _cardUI.Deselect(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // _cardUI.Deselect(this);
        }

        public void Activate()
        {
            transform.DOPunchScale(_punch.Punch, _punch.Duration);
        }

        public Tween Move(Vector2 position, Quaternion rotation)
        {
            return DOTween
                .Sequence()
                .Append(_rectTransform.DOMove(position, _move.Duration))
                .Join(_rectTransform.DORotateQuaternion(rotation, _move.Duration));
        }

        public Tween Shrink(bool instant = false)
        {
            return ApplySizeChange(_shrink, instant);
        }

        public Tween Enlarge(bool instant = false)
        {
            return ApplySizeChange(_enlarge, instant);
        }

        private Tween ApplySizeChange(SizeChange sizeChange, bool instant = false)
        {
            if (instant)
            {
                _rectTransform.sizeDelta = sizeChange.Size;
                return null;
            }
            return _rectTransform
                .DOSizeDelta(sizeChange.Size, sizeChange.Duration, true)
                .SetEase(sizeChange.Ease);
        }
    }
}
