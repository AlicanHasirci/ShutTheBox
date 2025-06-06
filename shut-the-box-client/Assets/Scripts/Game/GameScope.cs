using Player;
using VContainer;
using VContainer.Unity;

namespace Game
{
    using Match;
    using Network;
    using UnityEngine;

    public class GameScope : LifetimeScope
    {
        [SerializeField]
        private Camera _camera;

        protected override void Configure(IContainerBuilder builder)
        {
            string playerId = Parent.Container.Resolve<INetworkService>().PlayerId;
            MatchModel matchModel = Parent.Container.Resolve<IMatchPresenter>().Model;

            builder.RegisterInstance(matchModel);
            builder.RegisterComponent(_camera);
            foreach (PlayerModel player in matchModel.Players)
            {
                RegistrationBuilder registrationBuilder = !playerId.Equals(player.PlayerId)
                    ? builder.Register<PlayerPresenter>(Lifetime.Singleton)
                    : builder.Register<LocalPlayerPresenter>(Lifetime.Singleton);
                registrationBuilder.WithParameter(player).AsImplementedInterfaces();
            }
        }
    }
}
