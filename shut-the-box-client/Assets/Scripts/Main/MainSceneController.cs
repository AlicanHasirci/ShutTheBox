using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using Network;
using Popups;
using R3;
using Revel.SceneManagement;
using Revel.UI.Popup;
using Utility;
using VContainer.Unity;
using DisposableBag = MessagePipe.DisposableBag;

namespace Main
{
    public class MainSceneController : IPostStartable, IDisposable
    {
        private readonly ISceneController _sceneController;
        private readonly INetworkService _networkService;
        private readonly IPopupManager _popupManager;
        private readonly IDisposable _disposable;

        public MainSceneController(
            ISubscriber<bool, InfoPopup.Payload> infoPopupSubscriber,
            ISceneController sceneController,
            INetworkService networkService,
            IPopupManager popupManager
        )
        {
            _sceneController = sceneController;
            _networkService = networkService;
            _popupManager = popupManager;

            _disposable = DisposableBag.Create(
                infoPopupSubscriber.Subscribe(true, ShowInfoPopup),
                infoPopupSubscriber.Subscribe(false, HideInfoPopup),
                _networkService.Connected.Where(c => !c).Subscribe(_ => OnDisconnect())
            );
        }

        public void PostStart() { }

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        private void OnDisconnect()
        {
            ShowInfoPopup(
                new InfoPopup.Payload(
                    "Disconnected",
                    "Reconnect",
                    () =>
                    {
                        _networkService.ConnectAsync().Forget();
                        _popupManager.Hide<InfoPopup>();
                    }
                )
            );
            _sceneController.ChangeTopScene("MenuScene").Forget();
        }

        private void ShowInfoPopup(InfoPopup.Payload payload)
        {
            _popupManager.Show<InfoPopup, InfoPopup.Payload>(payload);
        }

        private void HideInfoPopup(InfoPopup.Payload payload)
        {
            _popupManager.Hide<InfoPopup>();
        }
    }
}
