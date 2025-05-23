namespace Debug
{
    using System;
    using Cysharp.Threading.Tasks;
    using Network;
    using R3;
    using Sirenix.OdinInspector;
    using UnityEngine;

    public partial class DebugServices
    {
        public DebugNetworkService NetworkService;
        
        [Serializable]
        [HideLabel, BoxGroup("Network", VisibleIf = "@serviceType == ServiceType.Network && enabled")]
        public class DebugNetworkService : INetworkService
        {
            public string PlayerId => _playerId;
            public ReactiveProperty<bool> Connected { get; } = new(false);
            
            [SerializeField] 
            private string _playerId = "0";
            
            public UniTask ConnectAsync()
            {
                Connected.Value = true;
                return UniTask.CompletedTask;
            }
            
            public void Connect()
            {
                Connected.Value = true;
            }

            public void Disconnect()
            {
                Connected.Value = false;
            }
        }
    }
}