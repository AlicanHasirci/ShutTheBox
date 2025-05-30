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
    using TileState = Network.TileState;

    public partial class DebugServices
    {
        public DebugPlayerService PlayerService;

        [Serializable]
        [HideLabel, BoxGroup("Player", VisibleIf = "@serviceType == ServiceType.Player && enabled")]
        public class DebugPlayerService : IPlayerService, IDisposable
        {
            public ISubscriber<JokerSelect> OnJoker { get; set; }
            public ISubscriber<PlayerTurn> OnTurn { get; set; }
            public ISubscriber<PlayerRoll> OnRoll { get; set; }
            public ISubscriber<PlayerMove> OnMove { get; set; }
            public ISubscriber<PlayerConfirm> OnConfirm { get; set; }

            private IDisposablePublisher<JokerSelect> SelectPublisher { get; set; }
            private IDisposablePublisher<PlayerTurn> TurnPublisher { get; set; }
            private IDisposablePublisher<PlayerRoll> RollPublisher { get; set; }
            private IDisposablePublisher<PlayerMove> MovePublisher { get; set; }
            private IDisposablePublisher<PlayerConfirm> ConfirmPublisher { get; set; }

            private IDisposable _subscription;

            public string PlayerId;
            private IMatchPresenter _matchPresenter;

            [Inject]
            private void Inject(EventFactory eventFactory, IMatchPresenter matchPresenter)
            {
                _matchPresenter = matchPresenter;
                (SelectPublisher, OnJoker) = eventFactory.CreateEvent<JokerSelect>();
                (TurnPublisher, OnTurn) = eventFactory.CreateEvent<PlayerTurn>();
                (RollPublisher, OnRoll) = eventFactory.CreateEvent<PlayerRoll>();
                (MovePublisher, OnMove) = eventFactory.CreateEvent<PlayerMove>();
                (ConfirmPublisher, OnConfirm) = eventFactory.CreateEvent<PlayerConfirm>();

                _subscription = DisposableBag.Create(
                    SelectPublisher,
                    TurnPublisher,
                    RollPublisher,
                    MovePublisher,
                    ConfirmPublisher
                );
            }

            [Button("Joker", ButtonStyle.FoldoutButton)]
            public void JokerSelectDebug(Joker joker)
            {
                JokerSelect js = new() { PlayerId = PlayerId, Selected = joker };
                SelectPublisher.Publish(js);
            }

            [Button("Turn", ButtonStyle.FoldoutButton)]
            public void TurnDebug()
            {
                TurnPublisher.Publish(new PlayerTurn { PlayerId = PlayerId });
            }

            [Button("Roll", ButtonStyle.FoldoutButton)]
            public void RollDebug(int[] rolls)
            {
                PlayerRoll pr = new() { PlayerId = PlayerId };
                pr.Rolls.AddRange(rolls);
                RollPublisher.Publish(pr);
            }

            [Button("Move", ButtonStyle.FoldoutButton)]
            public void MoveDebug(int i, TileState tile)
            {
                MovePublisher.Publish(
                    new PlayerMove
                    {
                        PlayerId = PlayerId,
                        Index = i,
                        State = tile,
                    }
                );
            }

            [Button("Confirm", ButtonStyle.FoldoutButton)]
            public void ConfirmDebug(int score, bool boxShut)
            {
                PlayerConfirm pc = new()
                {
                    PlayerId = PlayerId,
                    Score = score,
                    BoxShut = boxShut,
                };
                foreach (PlayerModel playerModel in _matchPresenter.Model.Players)
                {
                    if (PlayerId == playerModel.PlayerId)
                    {
                        foreach (TileState tile in playerModel.Tiles)
                        {
                            pc.Tiles.Add(tile);
                        }

                        foreach (Joker joker in playerModel.Jokers)
                        {
                            pc.Jokers.Add(new JokerScore { Joker = joker, Score = 5 });
                        }
                    }
                }
                ConfirmPublisher.Publish(pc);
            }

            public void Roll()
            {
                Debug.Log("Roll");
            }

            public void Select(Joker joker)
            {
                Debug.Log($"Select {joker}");
            }

            public void Toggle(int index)
            {
                Debug.Log($"Toggle {index}");
            }

            public void Confirm()
            {
                Debug.Log("Confirm");
            }

            public void Done()
            {
                Debug.Log("Done");
            }

            public void Dispose()
            {
                _subscription?.Dispose();
            }
        }
    }
}
