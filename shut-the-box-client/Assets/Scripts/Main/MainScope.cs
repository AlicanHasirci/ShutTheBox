using Revel.UI.Popup;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Main
{
    public class MainScope : LifetimeScope
    {
        [SerializeField]
        private PopupManager _popupManager;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<MainSceneController>();
            builder.RegisterInstance<IPopupManager>(_popupManager);
        }
    }
}
