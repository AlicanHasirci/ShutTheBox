namespace UI
{
    using Sirenix.OdinInspector;
    using UnityEngine;
    using Utility;

    public class ToastFactory : MonoSingleton<ToastFactory>
    {
        [SerializeField]
        private Toast _toastPrefab;

        private ObjectPool<Toast> _toastPool;

        private void Awake()
        {
            _toastPool = new ObjectPool<Toast>(_toastPrefab, transform, 10);
        }

        [Button]
        public async void ScoreText(Transform parent, Vector2 position, int score)
        {
            Toast toast = _toastPool.Get();
            toast.transform.SetParent(parent);
            await toast.FloatingText(score.ToString(), position);
            _toastPool.Return(toast);
        }
    }
}
