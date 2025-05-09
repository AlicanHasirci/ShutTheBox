
namespace Player.Dice
{
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using Random = UnityEngine.Random;
    
    public class DiceManager : MonoBehaviour
    {
        [SerializeField]
        private DiceBehaviour[] _dice;

        [SerializeField]
        private Bounds[] _zones;

        [SerializeField]
        private float _velocity;

        [SerializeField]
        private float _elevation;

        [SerializeField]
        private float _rotation;

        private FrameData[] _iFrames;
        private Vector3[] _positions;
        private bool _cached;

        public void Awake()
        {
            _iFrames = new FrameData[_dice.Length];
            _positions = new Vector3[_dice.Length];
            for (var i = 0; i < _dice.Length; i++)
            {
                _positions[i] = _dice[i].transform.position;
            }
        }

        [Button]
        public void ResetDice()
        {
            for (var i = 0; i < _dice.Length; i++)
            {
                var die = _dice[i];
                die.transform.position = _positions[i];
                die.transform.rotation = Quaternion.identity;
            }

            _cached = false;
        }

        [Button]
        public void CacheRollData()
        {
            for (var i = 0; i < _dice.Length; i++)
            {
                _dice[i].Body.isKinematic = false;
                _iFrames[i] = FrameData.FromTransform(_dice[i]);
            }
            do
            {
                foreach (DiceBehaviour d in _dice)
                {
                    d.Clear();
                }

                for (int i = 0; i < _dice.Length; i++)
                {
                    var die = _dice[i];
                    die.Frames.Add(_iFrames[i]);

                    var p0 = _iFrames[i].Position;
                    var p2 = transform.TransformPoint(RandomPoint(_zones[i % _zones.Length]));
                    var p1 = p2 + Vector3.up * _elevation;

                    var torque = Random.onUnitSphere * _rotation;
                    var sampleCount = _velocity / Time.fixedDeltaTime;
                    for (var j = 1; j < sampleCount; j++)
                    {
                        var t = j / sampleCount;
                        var p = GetPoint(p0, p1, p2, t);
                        var r =
                            die.Frames[^1].Rotation
                            * Quaternion.Euler(torque.x, torque.y, torque.z);
                        die.Frames.Add(new FrameData(die.Body, p, r));
                    }

                    var latest = die.Frames[^1];
                    die.Body.position = die.transform.position = latest.Position;
                    die.Body.rotation = die.transform.rotation = latest.Rotation;
                    die.Body.linearVelocity =
                        (latest.Position - die.Frames[^2].Position) / Time.fixedDeltaTime;
                    die.Body.angularVelocity = torque;
                }

                var enumerators = _dice.Select(d => d.Record()).ToArray();
                while (enumerators.Aggregate(true, (b, e) => b && e.MoveNext()))
                {
                    Physics.Simulate(Time.fixedDeltaTime);
                }
            } while (CheckCocked());

            for (var i = 0; i < _dice.Length; i++)
            {
                _dice[i].Body.isKinematic = true;
                _dice[i].Frames[0].Play();
            }
            _cached = true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            var p = 1 - t;
            return p0 * p * p + 2 * p1 * t * p + p2 * t * t;
        }

        [Button]
        public async UniTask RollDice(int[] roll, bool skip = false, CancellationToken token = default)
        {
            if (!_cached)
            {
                CacheRollData();
            }

            for (int i = 0; i < _dice.Length; i++)
            {
                _dice[i].Frames[^1].Play();
                _dice[i].Adjust(roll[i]);
            }

            if (skip) return;
            await Playback(token);
        }

        private async UniTask Playback(CancellationToken token = default)
        {
            var uniTasks = _dice.Select(d =>
                d.Playback().ToUniTask(PlayerLoopTiming.FixedUpdate, cancellationToken: token)
            );
            await UniTask.WhenAll(uniTasks);
        }

        private bool CheckCocked()
        {
            return _dice.Aggregate(false, (current, t) => current | t.IsCocked());
        }

        private void OnDrawGizmosSelected()
        {
            if (_dice == null)
                return;

            foreach (Bounds zone in _zones)
            {
                DrawZone(zone);
            }

            foreach (var die in _dice)
            {
                if (die.Frames == null)
                    continue;
                for (var i = 1; i < die.Frames.Count; i += 10)
                {
                    Gizmos.color = Color.Lerp(
                        Color.cyan,
                        Color.magenta,
                        i / (float)die.Frames.Count
                    );
                    Gizmos.DrawMesh(die.Mesh, 0, die.Frames[i].Position, die.Frames[i].Rotation);
                }
            }
        }

        private static Vector3 RandomPoint(Bounds bounds)
        {
            var extents = bounds.extents;
            return bounds.center
                + new Vector3(
                    Random.Range(-extents.x, extents.x),
                    Random.Range(-extents.y, extents.y),
                    Random.Range(-extents.z, extents.z)
                );
        }

        private readonly Vector3[] _points = new Vector3[8];

        private void DrawZone(Bounds bounds)
        {
            var min = bounds.min;
            var max = bounds.max;
            _points[0] = min;
            _points[1] = new Vector3(min.x, min.y, max.z);
            _points[2] = new Vector3(max.x, min.y, max.z);
            _points[3] = new Vector3(max.x, min.y, min.z);
            _points[4] = new Vector3(min.x, max.y, min.z);
            _points[5] = new Vector3(min.x, max.y, max.z);
            _points[6] = new Vector3(max.x, max.y, max.z);
            _points[7] = new Vector3(max.x, max.y, min.z);

            Gizmos.color = Color.green;
            for (var i = 0; i < _points.Length; i++)
            {
                _points[i] = transform.TransformPoint(_points[i]);
            }

            for (var o = 0; o < 2; o++)
            {
                o *= 4;
                for (var i = 0; i < 4; i++)
                {
                    var f = i + o;
                    var t = (i + 1) % 4 + o;
                    Gizmos.DrawLine(_points[f], _points[t]);
                }
            }
            for (var i = 0; i < 4; i++)
            {
                var t = i + 4;
                Gizmos.DrawLine(_points[i], _points[t]);
            }
        }
    }
}
