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

    public class AppScope : LifetimeScope
    {
        public NetworkSettings networkSettings;
        public DebugServices debugServices;
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
            
            if (debugServices.enabled)
            {
                builder.RegisterInstance(debugServices).AsImplementedInterfaces();
                builder.RegisterBuildCallback(resolver => resolver.Inject(debugServices));
            }
            else
            {
                builder
                    .Register<NetworkService>(Lifetime.Singleton)
                    .AsSelf()
                    .AsImplementedInterfaces()
                    .WithParameter(typeof(INetworkSettings), networkSettings);
                builder.Register<NetworkMatchService>(Lifetime.Singleton).AsImplementedInterfaces();
                builder.Register<NetworkPlayerService>(Lifetime.Singleton).AsImplementedInterfaces();
            }
        }
    }
}
