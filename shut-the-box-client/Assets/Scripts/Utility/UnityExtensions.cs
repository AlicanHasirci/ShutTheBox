using UnityEngine;

namespace Utility
{
    public static class UnityExtensions
    {
        public static Vector3 RandomPoint(this Bounds bounds)
        {
            var extents = bounds.extents;
            return bounds.center
                + new Vector3(
                    Random.Range(-extents.x, extents.x),
                    Random.Range(-extents.y, extents.y),
                    Random.Range(-extents.z, extents.z)
                );
        }

        public static Color SetAlpha(this Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }
    }
}
