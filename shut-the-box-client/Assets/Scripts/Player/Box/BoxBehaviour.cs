using R3;
using UnityEngine;

namespace Player.Box
{
    public class BoxBehaviour : MonoBehaviour
    {
        [SerializeField]
        private TileBehaviour[] tiles;

        [SerializeField]
        private Vector2 angles;
        public Subject<int> OnClick { get; } = new();

        private void Awake()
        {
            for (var i = 0; i < tiles.Length; i++)
            {
                var tile = tiles[i];
                tile.Initialize(i, OnSelect, angles);
            }
        }

        public void SetState(TileState[] state)
        {
            for (var i = 0; i < tiles.Length; i++)
            {
                tiles[i].SetState(state[i]);
            }
        }

        private void OnSelect(int index)
        {
            OnClick.OnNext(index);
        }
    }
}
