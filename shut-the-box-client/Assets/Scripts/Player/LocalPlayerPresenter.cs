using System;
using MessagePipe;
using R3;

namespace Player
{
    using Network;

    public interface ILocalPlayerPresenter : IPlayerPresenter
    {
        ReactiveProperty<bool> CanConfirm { get; }
        public void Ready();
        public void Confirm();
        public void Roll();
        public void Done();
    }

    public class LocalPlayerPresenter : PlayerPresenter, ILocalPlayerPresenter
    {
        public ReactiveProperty<bool> CanConfirm { get; }
        private PlayerState _state;

        public LocalPlayerPresenter(
            PlayerModel model,
            IPlayerService playerService,
            IMatchService matchService,
            EventFactory eventFactory
        )
            : base(model, playerService, matchService, eventFactory)
        {
            CanConfirm = new ReactiveProperty<bool>(false);
        }

        public void Ready()
        {
            Service.Ready();
        }

        public void Roll()
        {
            if (_state is PlayerState.Idle)
                return;
            SetState(PlayerState.Idle);
            StatePublisher.Publish(_state);
            Service.Roll();
        }

        public override void TileToggle(int index)
        {
            if (_state is not PlayerState.Play)
                return;
            if (!Toggle(index))
                return;
            Service.Toggle(index);
            BoxPublisher.Publish(Model.Tiles);
            CanConfirm.Value = IsTurnComplete();
        }

        public void Confirm()
        {
            if (_state is not PlayerState.Play)
            {
                return;
            }
            SetState(PlayerState.Idle);
            Service.Confirm();
            for (int i = 0; i < Model.Tiles.Length; i++)
            {
                if (Model.Tiles[i] is not TileState.Toggle)
                {
                    continue;
                }
                Model.Tiles[i] = TileState.Shut;
            }
            BoxPublisher.Publish(Model.Tiles);
            CanConfirm.Value = false;
        }

        public void Done()
        {
            Service.Done();
        }

        protected override void OnPlayerTurn(PlayerTurn playerTurn)
        {
            if (!IsCurrentPlayer(playerTurn.PlayerId))
            {
                return;
            }
            SetState(PlayerState.Roll);
        }

        protected override async void OnPlayerRoll(PlayerRoll playerRoll)
        {
            if (!IsCurrentPlayer(playerRoll.PlayerId))
            {
                return;
            }
            base.OnPlayerRoll(playerRoll);
            SetState(HasMoves() ? PlayerState.Play : PlayerState.Fail);
        }

        private bool Toggle(int index)
        {
            TileState tile = Model.Tiles[index];
            switch (tile)
            {
                case TileState.Shut:
                    return false;
                case TileState.Open:
                    Model.Tiles[index] = TileState.Toggle;
                    break;
                case TileState.Toggle:
                    Model.Tiles[index] = TileState.Open;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return true;
        }

        private bool IsTurnComplete()
        {
            int sum = 0;
            for (int i = 0; i < Model.Tiles.Length; i++)
            {
                if (Model.Tiles[i] is not TileState.Toggle)
                    continue;
                sum += i + 1;
            }
            return Model.TotalRoll == sum;
        }

        private bool HasMoves() {
            return CanMakeSum(Model.TotalRoll);
        }
         
        private bool CanMakeSum(int target, int index = 0) {
            if (target == 0)
            {
                return true;
            }
            if (index >= Model.Tiles.Length || target < 0)
            {
                return false;
            }
            int next = index + 1;
            int value = index + 1;
            return (Model.Tiles[index] is TileState.Open && CanMakeSum(target - value, next)) || CanMakeSum(target, next);
        }

        private void SetState(PlayerState state)
        {
            if (_state == state)
                return;
            _state = state;
            StatePublisher.Publish(_state);
        }
    }
}
