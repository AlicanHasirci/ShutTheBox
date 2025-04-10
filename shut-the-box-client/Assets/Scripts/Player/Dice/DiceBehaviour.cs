using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Dice
{
    public class DiceBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody body;

        [SerializeField]
        private MeshFilter meshFilter;

        [SerializeField]
        private Transform[] faces;

        public Rigidbody Body => body;
        public Mesh Mesh => meshFilter.sharedMesh;
        public List<FrameData> Frames => _frames;

        private const float Tolerance = 0.1f;
        private const int SleepThreshold = 50;

        [SerializeField]
        private List<FrameData> _frames = new(500);

        private void Awake()
        {
            body.isKinematic = true;
        }

        public void Adjust(int to)
        {
            var r = transform.rotation;
            transform.rotation = _frames[^1].Rotation;
            var from = -1;
            var maxY = 0f;
            for (var i = 0; i < faces.Length; i++)
            {
                var positionY = faces[i].transform.position.y;
                if (positionY < maxY)
                    continue;
                maxY = positionY;
                from = i + 1;
            }
            transform.rotation = r;

            var fromDir = faces[from - 1].localPosition.normalized;
            var toDir = faces[to - 1].localPosition.normalized;
            meshFilter.transform.localRotation = Quaternion.Inverse(
                Quaternion.FromToRotation(fromDir, toDir)
            );
        }

        public bool IsCocked()
        {
            var n = _frames[^1].Rotation * Vector3.up;
            var dot = Mathf.Abs(Vector3.Dot(Vector3.up, n));
            return dot is > Tolerance and < 1 - Tolerance;
        }

        public IEnumerator Record()
        {
            var sleepCount = 0;
            while (sleepCount <= SleepThreshold && _frames.Count < 500)
            {
                if (body.IsSleeping())
                    sleepCount++;
                else
                    _frames.Add(FrameData.FromBody(this));
                yield return null;
            }
        }

        public IEnumerator Playback()
        {
            for (var i = 0; i < _frames.Count; i++)
            {
                _frames[i].Play();
                yield return null;
            }
        }

        public void Clear()
        {
            _frames.Clear();
            meshFilter.transform.localRotation = Quaternion.identity;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }
    }
}
