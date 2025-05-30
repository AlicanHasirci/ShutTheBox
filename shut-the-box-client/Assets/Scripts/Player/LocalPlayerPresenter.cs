namespace Player
{
    using System;
    using MessagePipe;
    using Network;
    using R3;

    public interface ILocalPlayerPresenter : IPlayerPresenter
    {
        ReactiveProperty<bool> CanConfirm { get; }
        public void Select(Joker joker);
        public void Confirm();
        public void Roll();
        public void Done();
    }

    public class LocalPlayerPresenter : PlayerPresenter, ILocalPlayerPresenter
    {
        public ReactiveProperty<bool> CanConfirm { get; }

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

        public void Select(Joker joker)
        {
            Service.Select(joker);
        }

        public void Roll()
        {
            SetState(PlayerState.Idle);
            Service.Roll();
        }

        public override void TileToggle(int index)
        {
            if (Model.State is not PlayerState.Play)
                return;
            if (!Toggle(index))
                return;
            Service.Toggle(index);
            BoxPublisher.Publish(Model.Tiles);
            CanConfirm.Value = IsTurnComplete();
        }

        public void Confirm()
        {
            if (Model.State is not PlayerState.Play)
            {
                return;
            }
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
            SetState(PlayerState.Idle);
        }

        public void Done()
        {
            Service.Done();
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
    }
}
