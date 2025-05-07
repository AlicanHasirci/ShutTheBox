// using R3;
// using VContainer;
//
// namespace Debug
// {
//     using System.Collections.Generic;
//     using System.Linq;
//     using Match;
//     using MessagePipe;
//     using Network;
//     using Player;
//     using Sirenix.OdinInspector;
//     using UnityEngine;
//
//     public partial class DebugServices : IMatchService
//     {
//         public MatchModel Model => _matchModel;
//         public ISubscriber<MatchState> OnMatchState { get; private set; }
//         
//         private IDisposablePublisher<MatchState> _matchStatePublisher;
//         private IDisposablePublisher<int> _onRoundStart;
//         
//         [SerializeField, BoxGroup("Match", VisibleIf = "@serviceType == ServiceType.Match && enabled")]
//         private MatchModel _matchModel;
//
//         [Inject]
//         public void MatchService(EventFactory eventFactory)
//         {
//             (_matchStatePublisher, OnMatchState) = eventFactory.CreateEvent<MatchState>();
//             (_onRoundStart, OnRoundStart) = eventFactory.CreateEvent<int>();
//
//             _disposable = Disposable.Combine(_disposable, _matchStatePublisher);
//             _disposable = Disposable.Combine(_disposable, _onRoundStart);
//             
//         }
//
//         [PropertySpace]
//         [BoxGroup("Match")]
//         [Button(ButtonStyle.FoldoutButton)]
//         private void PublishMatchState(MatchState state)
//         {
//             _matchStatePublisher?.Publish(state);
//         }
//         
//         public void StartMatchmaking()
//         {
//             Debug.Log("DebugServices.MatchService.StartMatchmaking");
//             _matchStatePublisher?.Publish(MatchState.Finding);
//         }
//
//         public void CancelMatchmaking()
//         {
//             Debug.Log("DebugServices.MatchService.CancelMatchmaking");
//             _matchStatePublisher?.Publish(MatchState.Idle);
//         }
//
//         public void LeaveMatch()
//         {
//             Debug.Log("DebugServices.MatchService.LeaveMatch");
//             _matchStatePublisher?.Publish(MatchState.Idle);
//         }
//         
//         
//
//         [Button(ButtonStyle.FoldoutButton, Name = "Round Start")]
//         [BoxGroup("Match")]
//         private void PublishRoundStart()
//         {
//             List<string> players = Model.Players.Select(model => model.PlayerId).ToList();
//             List<int> scores = Model.Players.Select(GetScore).ToList();
//             Model.Rounds.Add(new RoundModel(players, scores));
//             _onRoundStart?.Publish(5);
//         }
//
//         private static int GetScore(PlayerModel player)
//         {
//             int score = 0;
//             for (int i = 0; i < player.Tiles.Length; i++)
//             {
//                 TileState tile = player.Tiles[i];
//                 if (tile is TileState.Shut)
//                 {
//                     score += i + 1;
//                 }
//             }
//
//             return score;
//         }
//     }
// }