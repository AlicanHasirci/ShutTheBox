using Debug;
using MessagePipe;
using Network;
using Revel.Diagnostics;
using Revel.Native;
using Revel.SceneManagement;
using VContainer;
using VContainer.Unity;
using ILogger = Revel.Diagnostics.ILogger;

namespace App
{
    using Match;
    using Player.Jokers;

    public class AppScope : LifetimeScope
    {
        public NetworkSettings networkSettings;
        public DebugServices debugServices;
        public JokerDatabase jokerDatabase;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(c =>
                GlobalMessagePipe.SetProvider(c.AsServiceProvider())
            );

            builder.RegisterEntryPoint<AppController>();

            builder.Register<INative, DebugNative>(Lifetime.Singleton);
            builder.Register<ILogger, DebugLogger>(Lifetime.Singleton);
            builder.Register<ISceneController, SceneController>(Lifetime.Singleton);
            builder.Register<IMatchPresenter, MatchPresenter>(Lifetime.Singleton);
            builder.RegisterInstance<IJokerDatabase>(jokerDatabase);

            if (debugServices.enabled)
            {
                builder.RegisterInstance(debugServices.NetworkService).AsImplementedInterfaces();
                builder.RegisterBuildCallback(resolver =>
                    resolver.Inject(debugServices.NetworkService)
                );
                builder.RegisterInstance(debugServices.MatchService).AsImplementedInterfaces();
                builder.RegisterBuildCallback(resolver =>
                    resolver.Inject(debugServices.MatchService)
                );
                builder.RegisterInstance(debugServices.PlayerService).AsImplementedInterfaces();
                builder.RegisterBuildCallback(resolver =>
                    resolver.Inject(debugServices.PlayerService)
                );
            }
            else
            {
                builder
                    .Register<NetworkService>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithParameter(typeof(INetworkSettings), networkSettings);
                builder.Register<NetworkMatchService>(Lifetime.Singleton).AsImplementedInterfaces();
                builder
                    .Register<NetworkPlayerService>(Lifetime.Singleton)
                    .AsImplementedInterfaces();
            }
        }
    }
}
