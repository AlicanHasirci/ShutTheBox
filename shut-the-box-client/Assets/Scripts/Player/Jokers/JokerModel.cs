namespace Player.Jokers
{
    using System;
    using Network;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public struct JokerSelection
    {
        public Joker[] Jokers;
    }
    
    [Serializable]
    public class JokerModel
    {
        [ReadOnly]
        public Joker Type;
        public Sprite Icon;
        public string Name;
        public string Description;
    }
}