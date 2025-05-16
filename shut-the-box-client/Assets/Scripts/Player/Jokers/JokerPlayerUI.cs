namespace Player.Jokers
{
    using System;
    using System.Collections.Generic;
    using DG.Tweening;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class JokerPlayerUI : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Serializable]
        public struct Placement
        {
            public Vector3 Position;
            public Quaternion Rotation;
        }

        public float StartAngle = 90;
        public float Radius = 5f;
        public float Slice = 45f;
        public JumpAnimation JumpAnimation;
        public List<JokerSelectionBehaviour> Cards;
        
        private JokerSelectionBehaviour _selected;

        private void Awake()
        {
        }

        public void Add(JokerSelectionBehaviour jokerSelectionBehaviour)
        {
            
        }


        public void PositionObjects()
        {
            if (Cards == null) return;
            for (int i = 0; i < Cards.Count; i++)
            {
                JokerSelectionBehaviour card = Cards[i];
                Placement t = CalculateTransforms(i, Cards.Count);
                card.transform.position = t.Position;
                card.transform.rotation = t.Rotation;
            }
        }

        public Placement CalculateTransforms(int index, int count)
        {
            float sweep = Slice * (count - 1);
            float angle = 90 + sweep * .5f + Slice * -index;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 fanCenter = transform.position;
            float x = fanCenter.x
                      + Radius * Mathf.Cos(rad) * transform.right.x
                      + Radius * Mathf.Sin(rad) * transform.up.x;
            float y = fanCenter.y
                      + Radius * Mathf.Cos(rad) * transform.right.y
                      + Radius * Mathf.Sin(rad) * transform.up.y;
            float z = fanCenter.z
                      + Radius * Mathf.Cos(rad) * transform.right.z
                      + Radius * Mathf.Sin(rad) * transform.up.z;
            Vector3 position = new(x, y, z);
            return new Placement
            {
                Position = position,
                Rotation = Quaternion.AngleAxis(angle - 90, transform.forward)
            };
        }

        private void Select(JokerSelectionBehaviour target)
        {
            if (_selected == target) return;
            DeselectCard();
            _selected = target;

            _selected.transform.DOKill();
            _selected.transform.DOLocalJump(
                _selected.transform.localPosition + JumpAnimation.delta,
                JumpAnimation.power,
                JumpAnimation.number,
                JumpAnimation.duration);
        }

        private void DeselectCard()
        {
            if (_selected == null) return;

            MoveToOriginalPosition();
            _selected = null;
        }

        private void MoveToOriginalPosition()
        {
            if (_selected == null)
            {
                return;
            }

            int index = Cards.IndexOf(_selected);
            Placement t = CalculateTransforms(index, Cards.Count);
            _selected.transform.DOKill();
            _selected.transform.DOMove(t.Position, .35f);
            _selected.transform.DORotateQuaternion(t.Rotation, .35f);
            _selected.transform.DOScale(Vector3.one, .35f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (TryGetBehaviour(eventData, out JokerSelectionBehaviour target))
            {
                Select(target);
            }
            else
            {
                DeselectCard();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            DeselectCard();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!TryGetBehaviour(eventData, out JokerSelectionBehaviour card)) return;
            if (card != _selected)
            {
                Select(card);
            }
        }

        private static bool TryGetBehaviour(PointerEventData eventData, out JokerSelectionBehaviour card)
        {
            GameObject go = eventData.pointerCurrentRaycast.gameObject;
            card = null;
            if (go == null)
            {
                return false;
            }

            card = go.GetComponent<JokerSelectionBehaviour>();
            return card != null;
        }
    }

    [Serializable]
    public struct JumpAnimation
    {
        [SerializeField] public Vector3 delta;
        [SerializeField] public float power;
        [SerializeField] public int number;
        [SerializeField] public float duration;

        public void Jump(Transform t)
        {
            t.DOLocalJump(t.localPosition + delta, power, number, duration).Play();
        }

        public void JumpBack(Transform t)
        {
            t.DOLocalMove(t.localPosition - delta, duration).Play();
        }
    }
}