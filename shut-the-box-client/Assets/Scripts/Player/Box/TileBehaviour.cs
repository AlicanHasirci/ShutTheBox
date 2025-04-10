using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Player.Box
{
    public class TileBehaviour : MonoBehaviour, IPointerClickHandler
    {
        private int _index;
        private Vector2 _angles;
        private Action<int> _onClick;
        private TileState _state;
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        public void Initialize(int index, Action<int> onClick, Vector2 angles)
        {
            _index = index;
            _angles = angles;
            _onClick = onClick;
        }

        public void SetState(TileState current)
        {
            if (_state == current)
                return;
            _state = current;
            DOTween.Kill(this);
            var rotation = _transform.localEulerAngles;
            switch (current)
            {
                case TileState.Open:
                    rotation.x = _angles.x;
                    transform.DOLocalRotate(rotation, .25f).SetId(this);
                    break;
                case TileState.Toggle:
                    rotation.x = (_angles.y + _angles.x) * .5f;
                    transform.DOLocalRotate(rotation, .25f).SetId(this);
                    break;
                case TileState.Shut:
                    rotation.x = _angles.y;
                    transform.DOLocalRotate(rotation, .5f).SetEase(Ease.OutBounce).SetId(this);
                    break;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            _onClick?.Invoke(_index);
        }
    }
}
