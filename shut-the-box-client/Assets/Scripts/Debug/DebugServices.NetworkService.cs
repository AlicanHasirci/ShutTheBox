// namespace Debug
// {
//     using Cysharp.Threading.Tasks;
//     using Network;
//     using R3;
//     using Sirenix.OdinInspector;
//     using UnityEngine;
//
//     public partial class DebugServices : INetworkService
//     {
//         public string PlayerId => _playerId;
//         public ReactiveProperty<bool> Connected { get; } = new(false);
//         
//         [SerializeField] 
//         [BoxGroup("Network", VisibleIf = "@serviceType == ServiceType.Network && enabled")]
//         private string _playerId = "local-player";
//         
//         public UniTask ConnectAsync()
//         {
//             Connected.Value = true;
//             return UniTask.CompletedTask;
//         }
//         
//         [BoxGroup("Network"), Button, ShowIf("@!Connected.Value")]
//         public void Connect()
//         {
//             Connected.Value = true;
//         }
//
//         [BoxGroup("Network"), Button, ShowIf("@Connected.Value")]
//         public void Disconnect()
//         {
//             Connected.Value = false;
//         }
//     }
// }