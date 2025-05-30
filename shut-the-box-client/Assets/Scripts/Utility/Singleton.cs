namespace Utility
{
    using UnityEngine;

    public interface ISingleton
    {
        void Initialize();
    }

    public static class Singletons
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            var root = new GameObject();
            root.SetActive(false);
            var singletons = Resources.Load<GameObject>("Singletons");
            if (singletons == null)
                return;
            var instance = Object.Instantiate(singletons, root.transform);
            foreach (var singleton in instance.GetComponentsInChildren<ISingleton>(true))
            {
                singleton.Initialize();
            }
            Object.DontDestroyOnLoad(root);
            instance.transform.DetachChildren();
            Object.Destroy(root);
        }
    }

    public abstract class MonoSingleton : MonoBehaviour, ISingleton
    {
        private protected MonoSingleton() { }

        public void Initialize()
        {
            MakeCurrent();
        }

        protected abstract void MakeCurrent();
    }

    public abstract class MonoSingleton<T> : MonoSingleton
        where T : MonoSingleton<T>
    {
        public static T Instance { get; private set; }

        protected override void MakeCurrent()
        {
            Instance = (T)this;
        }
    }
}
