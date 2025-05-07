// using System;
// using Nakama;
// using R3;
// using ILogger = Revel.Diagnostics.ILogger;
// using DisposableBag = MessagePipe.DisposableBag;
//
// namespace Network
// {
//     public class NetworkMessageFactory : IDisposable
//     {
//         private readonly ILogger _logger;
//         private readonly NetworkService _networkService;
//         private readonly IDisposable _disposable;
//         
//          
//
//         public NetworkMessageFactory(ILogger logger, NetworkService networkService)
//         {
//             _logger = logger;
//             _networkService = networkService;
//             
//             _disposable = DisposableBag.Create(
//                 Observable
//                     .FromEvent(
//                         // ReSharper disable once RedundantLambdaParameterType
//                         (Action<IMatchState> a) => networkService.Socket.ReceivedMatchState += a,
//                         // ReSharper disable once RedundantLambdaParameterType
//                         (Action<IMatchState> a) => networkService.Socket.ReceivedMatchState -= a
//                     )
//                     .Subscribe(ReceivedMatchState)
//             );
//         }
//
//         private void ReceivedMatchState(IMatchState state)
//         {
//             OpCode opCode = (OpCode)state.OpCode;
//             _logger.Debug($"Received match state: {opCode}");
//         }
//
//         public void Dispose()
//         {
//             _disposable?.Dispose();
//         }
//     }
// }