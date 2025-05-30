namespace Player.Jokers
{
    using System;
    using System.Collections.Generic;
    using DG.Tweening;
    using Network;
    using UnityEngine;
    using VContainer;
    using Quaternion = UnityEngine.Quaternion;
    using Vector3 = UnityEngine.Vector3;

    public class JokerCardUI : MonoBehaviour
    {
        [Serializable]
        public struct Placement
        {
            public Vector3 Position;
            public Quaternion Rotation;
        }

        [Inject]
        public IJokerDatabase JokerDatabase;

        [SerializeField]
        private JokerCardBehaviour _cardPrefab;

        [SerializeField]
        private Transform _display;

        public float StartAngle = 90;
        public float Radius = 5f;
        public float Slice = 45f;

        public float CanvasScale
        {
            get
            {
                if (_canvas == null)
                {
                    _canvas = GetComponentInParent<Canvas>();
                }
                return _canvas.scaleFactor;
            }
        }
        private Canvas _canvas;
        private List<JokerCardBehaviour> _cards;

        public void Initialize(int count)
        {
            _cards = new List<JokerCardBehaviour>(count);
        }

        public void Add(Joker joker, bool instant = false)
        {
            foreach (JokerCardBehaviour card in _cards)
            {
                if (card.Type == joker)
                    return;
            }

            if (instant)
            {
                Placement placement = CalculateTransforms(_cards.Count, _cards.Capacity);
                JokerCardBehaviour card = Create(placement.Position, placement.Rotation);
                card.Initialize(JokerDatabase[joker], this);
                card.Enlarge(true);
                card.Shrink();
                _cards.Add(card);
            }
        }

        public JokerCardBehaviour Get(Joker joker)
        {
            foreach (JokerCardBehaviour card in _cards)
            {
                if (card.Type == joker)
                    return card;
            }

            return null;
        }

        public JokerCardBehaviour Create(Vector3 position, Quaternion rotation)
        {
            return Instantiate(_cardPrefab, position, rotation, transform);
        }

        public Placement CalculateTransforms(int index, int count)
        {
            float sweep = Slice * (count - 1);
            float angle = 90 + sweep * .5f + Slice * -index;
            float rad = angle * Mathf.Deg2Rad;
            float radius = Radius * CanvasScale;
            Vector3 fanCenter = transform.position;
            float x =
                fanCenter.x
                + radius * Mathf.Cos(rad) * transform.right.x
                + radius * Mathf.Sin(rad) * transform.up.x;
            float y =
                fanCenter.y
                + radius * Mathf.Cos(rad) * transform.right.y
                + radius * Mathf.Sin(rad) * transform.up.y;
            float z =
                fanCenter.z
                + radius * Mathf.Cos(rad) * transform.right.z
                + radius * Mathf.Sin(rad) * transform.up.z;
            Vector3 position = new(x, y, z);
            return new Placement
            {
                Position = position,
                Rotation = Quaternion.AngleAxis(angle - 90, transform.forward),
            };
        }

        public void Select(JokerCardBehaviour target)
        {
            target.transform.SetParent(transform.parent, true);
            target.transform.SetAsLastSibling();
            DOTween.Kill(target);
            DOTween
                .Sequence()
                .Join(target.Enlarge())
                .Join(target.Move(_display.position, Quaternion.identity))
                .SetId(target);
        }

        public void Deselect(JokerCardBehaviour target)
        {
            int index = _cards.IndexOf(target);
            Placement placement = CalculateTransforms(index, _cards.Capacity);
            target.transform.SetParent(transform, true);
            target.transform.SetSiblingIndex(index);
            DOTween.Kill(target);
            DOTween
                .Sequence()
                .Join(target.Shrink())
                .Join(target.Move(placement.Position, placement.Rotation))
                .SetId(target);
        }
    }
}
