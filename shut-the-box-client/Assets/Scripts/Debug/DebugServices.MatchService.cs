namespace Debug
{
    using System;
    using Match;
    using MessagePipe;
    using Network;
    using Player;
    using Sirenix.OdinInspector;
    using UnityEngine;
    using VContainer;
    using DisposableBag = MessagePipe.DisposableBag;
    using PlayerState = Network.PlayerState;
    using TileState = Player.TileState;

    public partial class DebugServices
    {
        public DebugMatchService MatchService;

        [Serializable]
        [HideLabel, BoxGroup("Match", VisibleIf = "@serviceType == ServiceType.Match && enabled")]
        public class DebugMatchService : IMatchService, IDisposable
        {
            public string MatchId => MatchModel.MatchId;
            public ISubscriber<MatchStart> OnMatchStart { get; set; }
            public ISubscriber<RoundStart> OnRoundStart { get; set; }
            public ISubscriber<MatchOver> OnMatchOver { get; set; }

            public MatchModel MatchModel;
            private IDisposablePublisher<MatchStart> MatchStartPublisher { get; set; }
            private IDisposablePublisher<RoundStart> RoundStartPublisher { get; set; }
            private IDisposablePublisher<MatchOver> MatchOverPublisher { get; set; }

            private IDisposable _subscription;

            [Inject]
            public void Inject(EventFactory eventFactory)
            {
                (MatchStartPublisher, OnMatchStart) = eventFactory.CreateEvent<MatchStart>();
                (RoundStartPublisher, OnRoundStart) = eventFactory.CreateEvent<RoundStart>();
                (MatchOverPublisher, OnMatchOver) = eventFactory.CreateEvent<MatchOver>();

                _subscription = DisposableBag.Create(
                    MatchStartPublisher,
                    RoundStartPublisher,
                    MatchOverPublisher
                );
            }

            [Button("Match Start", ButtonStyle.FoldoutButton)]
            public void MatchStartDebug()
            {
                MatchStart ms = new()
                {
                    RoundCount = 3,
                    RoundId = 0,
                    TileCount = 9,
                    TurnTime = 0,
                };
                foreach (PlayerModel model in MatchModel.Players)
                {
                    Player player = new()
                    {
                        PlayerId = model.PlayerId,
                        Score = model.Score,
                        State = (PlayerState)model.State,
                    };
                    foreach (TileState tile in model.Tiles)
                    {
                        player.Tiles.Add((Network.TileState)tile);
                    }

                    foreach (Joker joker in model.Jokers)
                    {
                        player.Jokers.Add(joker);
                    }
                    ms.Players.Add(player);
                }
                MatchStartPublisher.Publish(ms);
            }

            [Button("Round Start", ButtonStyle.FoldoutButton)]
            public void RoundStartDebug(int roundId, Joker[] jokers)
            {
                RoundStart rs = new() { RoundId = roundId };
                foreach (PlayerModel player in MatchModel.Players)
                {
                    JokerChoice jc = new() { PlayerId = player.PlayerId };
                    jc.Jokers.AddRange(jokers);
                    rs.Choices.Add(jc);
                }
                RoundStartPublisher.Publish(rs);
            }

            [Button("Match Over", ButtonStyle.FoldoutButton)]
            public void MatchOverDebug()
            {
                MatchOver mo = new();
                foreach (PlayerModel player in MatchModel.Players)
                {
                    PlayerScore pc = new() { PlayerId = player.PlayerId, Score = player.Score };
                    mo.Scores.Add(pc);
                }
                MatchOverPublisher.Publish(mo);
            }

            public void StartMatchmaking()
            {
                Debug.Log("StartMatchmaking");
            }

            public void CancelMatchmaking()
            {
                Debug.Log("CancelMatchmaking");
            }

            public void PlayerReady()
            {
                Debug.Log("PlayerReady");
            }

            public void LeaveMatch()
            {
                Debug.Log("LeaveMatch");
            }

            public void Dispose()
            {
                _subscription?.Dispose();
            }
        }
    }
}
