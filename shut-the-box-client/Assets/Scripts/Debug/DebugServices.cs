using JetBrains.Annotations;
using Sirenix.OdinInspector;

namespace Debug
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "DebugServices", menuName = "App/DebugServices")]
    public partial class DebugServices : ScriptableObject
    {
        public enum ServiceType
        {
            Network,
            Match,
            Player,
        }

        public bool enabled;

        [UsedImplicitly]
        [EnumToggleButtons]
        [ShowIf("@enabled")]
        public ServiceType serviceType;
    }
}
