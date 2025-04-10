using UnityEngine;

namespace Network
{
    public interface INetworkSettings
    {
        string Host { get; }
        int Port { get; }
        string Scheme { get; }
        string Key { get; }
    }

    [CreateAssetMenu(
        fileName = "NetworkSettings",
        menuName = "Scriptables/NetworkSettings",
        order = 0
    )]
    public class NetworkSettings : ScriptableObject, INetworkSettings
    {
        [field: SerializeField]
        public string Host { private set; get; }

        [field: SerializeField]
        public int Port { private set; get; }

        [field: SerializeField]
        public string Scheme { private set; get; }

        [field: SerializeField]
        public string Key { private set; get; }
    }
}
