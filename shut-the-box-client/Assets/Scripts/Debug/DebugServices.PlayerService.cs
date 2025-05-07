// using JetBrains.Annotations;
// using R3;
// using Sirenix.OdinInspector.Editor;
//
// namespace Debug
// {
//     using UnityEngine;
//     using System.Collections.Generic;
//     using MessagePipe;
//     using Network;
//     using Sirenix.OdinInspector;
//     using VContainer;
//     using PlayerState = Player.PlayerState;
//     using TileState = Player.TileState;
//
//     public partial class DebugServices : IPlayerService
//     {
//         public ISubscriber<int> OnRoundStart { get; private set; }
//         public ISubscriber<string> OnTurn { get; private set; }
//         public ISubscriber<(string, int)> OnRoll { get; private set; }
//         public ISubscriber<(string, int, TileState)> OnMove { get; private set; }
//         public ISubscriber<(string, IReadOnlyList<TileState>)> OnConfirm { get; private set; }
//         
//         private IDisposablePublisher<string> _onTurn;
//         private IDisposablePublisher<(string, int)> _onRoll;
//         private IDisposablePublisher<(string, int, TileState)> _onMove;
//         private IDisposablePublisher<(string, IReadOnlyList<TileState>)> _onConfirm;
//
//         [BoxGroup("Player", VisibleIf = "@serviceType == ServiceType.Player && enabled"), ShowInInspector]
//         private string _psId;
//         [BoxGroup("Player"), ShowInInspector]
//         private PlayerState _psState;
//         [BoxGroup("Player"), ShowInInspector]
//         private int _psRoll;
//         [BoxGroup("Player"), ShowInInspector, OnCollectionChanged(After = "TileChanged")]
//         private TileState[] _psTiles = new TileState[9];
//         
//
//         [Inject]
//         public void PlayerServices(EventFactory eventFactory)
//         {
//             (_onTurn, OnTurn) = eventFactory.CreateEvent<string>();
//             (_onRoll, OnRoll) = eventFactory.CreateEvent<(string, int)>();
//             (_onMove, OnMove) = eventFactory.CreateEvent<(string, int, TileState)>();
//             (_onConfirm, OnConfirm) = eventFactory.CreateEvent<(string, IReadOnlyList<TileState>)>();
//             
//             _disposable = Disposable.Combine(_disposable, _onTurn);
//             _disposable = Disposable.Combine(_disposable, _onRoll);
//             _disposable = Disposable.Combine(_disposable, _onMove);
//             _disposable = Disposable.Combine(_disposable, _onConfirm);
//         }
//         
//         [Button(ButtonStyle.FoldoutButton, Name = "Turn")]
//         [BoxGroup("Player"), HorizontalGroup("Player/Buttons")]
//         private void PublishTurn()
//         {
//             _onTurn?.Publish(_psId);
//         }
//         
//         [Button(ButtonStyle.FoldoutButton, Name = "Roll")]
//         [BoxGroup("Player"), HorizontalGroup("Player/Buttons")]
//         private void PublishRoll()
//         {
//             _onRoll?.Publish((_psId, _psRoll));
//         }
//
//         [Button(ButtonStyle.FoldoutButton, Name = "Confirm")]
//         [BoxGroup("Player"), HorizontalGroup("Player/Buttons")]
//         private void PlayerConfirm()
//         {
//             _onConfirm?.Publish((_psId, _psTiles));
//         }
//
//         [UsedImplicitly]
//         public void TileChanged(CollectionChangeInfo info)
//         {
//             _onMove?.Publish((_psId, info.Index, _psTiles[info.Index]));
//         }
//         
//         public void Ready()
//         {
//             Debug.Log("DebugServices.PlayerService.PlayerReady");
//         }
//
//         public void Roll()
//         {
//             Debug.Log("DebugServices.PlayerService.PlayerRoll");
//         }
//
//         public void Toggle(int index)
//         {
//             Debug.Log("DebugServices.PlayerService.PlayerToggle");
//         }
//
//         public void Confirm()
//         {
//             Debug.Log("DebugServices.PlayerService.PlayerConfirm");
//         }
//
//         public void Done()
//         {
//             Debug.Log("DebugServices.PlayerService.Done");
//         }
//     }
// }