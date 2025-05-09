using Cysharp.Threading.Tasks;
using Revel.UI;
using Revel.UI.Popup;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Popups
{
    public class InfoPopup : PopupBehaviour<InfoPopup.Payload>
    {
        public struct Payload
        {
            public readonly string Header;
            public readonly string Button;
            public readonly UnityAction OnClick;

            public Payload(string header, string button, UnityAction onClick)
            {
                Header = header;
                Button = button;
                OnClick = onClick;
            }
        }

        [SerializeField]
        private TMP_Text _header;

        [SerializeField]
        private TMP_Text _button;
        
        [SerializeField]
        private TMP_Text _description;

        [SerializeField]
        private RevelButton _primary;

        public override UniTask OnShow(Payload parameter)
        {
            _header.text = parameter.Header;
            _button.text = parameter.Button;
            _primary.OnClick.AddListener(parameter.OnClick);
            _primary.OnClick.AddListener(Hide);

            return base.OnShow(parameter);
        }

        public override UniTask OnHide()
        {
            _header.text = string.Empty;
            _button.text = string.Empty;
            _primary.OnClick.RemoveAllListeners();
            return base.OnHide();
        }
    }
}
