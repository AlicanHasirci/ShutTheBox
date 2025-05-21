namespace Utility
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class ObjectPool<T>
        where T : Component, ObjectPool<T>.IPooledObject
    {
        public interface IPooledObject
        {
            ObjectPool<T> Pool { get; set; }
            void OnReturn();
        }

        public IEnumerable<T> InUse => _inuse;

        public T Prefab { get; }
        private readonly Transform _parent;
        private readonly HashSet<T> _inuse;
        private readonly Stack<T> _idle;

        public ObjectPool(T prefab, Transform parent, int poolSize)
        {
            Prefab = prefab;
            _parent = parent;
            _idle = new Stack<T>(poolSize);
            _inuse = new HashSet<T>();
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T obj = Object.Instantiate(Prefab, _parent);
                obj.gameObject.SetActive(false);
                obj.Pool = this;
                _idle.Push(obj);
            }
        }

        public T Get()
        {
            if (!_idle.TryPop(out T obj))
            {
                obj = Object.Instantiate(Prefab, _parent);
                obj.Pool = this;
            }
            _inuse.Add(obj);
            obj.gameObject.SetActive(true);
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null)
            {
                return;
            }
            _inuse.Remove(obj);
            _idle.Push(obj);
            obj.OnReturn();
            obj.transform.SetParent(_parent);
            obj.gameObject.SetActive(false);
        }

        public void ReturnAll()
        {
            List<T> inUse = _inuse.ToList();
            foreach (T pooledObject in inUse)
            {
                Return(pooledObject);
            }
        }
    }
}
