using System;
using UnityEngine;

namespace Utility
{
    [Serializable]
    public struct RandomVector3
    {
        public Vector3 min;
        public Vector3 max;

        public static implicit operator Vector3(RandomVector3 r) =>
            new(
                UnityEngine.Random.Range(r.min.x, r.max.x),
                UnityEngine.Random.Range(r.min.y, r.max.y),
                UnityEngine.Random.Range(r.min.z, r.max.z)
            );
    }
}
