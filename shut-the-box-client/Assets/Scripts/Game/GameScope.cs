using Match;
using Network;
using Player;
using VContainer;
using VContainer.Unity;

namespace Game
{
    public class GameScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            MatchModel match = Parent.Container.Resolve<IMatchService>().Model;
            string playerId = Parent.Container.Resolve<INetworkService>().PlayerId;

            builder.RegisterInstance(match);

            foreach (PlayerModel player in match.Players)
            {
                RegistrationBuilder registrationBuilder = !playerId.Equals(player.PlayerId)
                    ? builder.Register<PlayerPresenter>(Lifetime.Singleton)
                    : builder.Register<LocalPlayerPresenter>(Lifetime.Singleton);
                registrationBuilder.WithParameter(player).AsImplementedInterfaces();
            }
        }
    }
}
