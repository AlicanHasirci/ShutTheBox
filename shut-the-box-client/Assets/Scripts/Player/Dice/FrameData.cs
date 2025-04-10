using System;
using UnityEngine;

namespace Player.Dice
{
    [Serializable]
    public struct FrameData
    {
        public readonly Rigidbody Body;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;

        public static FrameData FromBody(DiceBehaviour die)
        {
            return new FrameData(die.Body, die.Body.position, die.Body.rotation);
        }

        public static FrameData FromTransform(DiceBehaviour die)
        {
            return new FrameData(die.Body, die.transform.position, die.transform.rotation);
        }

        public FrameData(Rigidbody body, Vector3 position, Quaternion quaternion)
            : this()
        {
            Body = body;
            Position = position;
            Rotation = quaternion;
        }

        public void Play()
        {
            Body.transform.position = Position;
            Body.transform.rotation = Rotation;
        }
    }
}
