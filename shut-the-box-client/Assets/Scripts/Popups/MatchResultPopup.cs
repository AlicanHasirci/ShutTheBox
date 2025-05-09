namespace Popups
{
    using Cysharp.Threading.Tasks;
    using Match;
    using Revel.UI.Popup;
    using TMPro;
    using UnityEngine;

    public class MatchResultPopup : PopupBehaviour<MatchResult>
    {
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _player;
        [SerializeField] private TMP_Text _opponent;

        public override UniTask OnShow(MatchResult payload)
        {
            _title.text = payload.Type.ToString();
            _player.text = $"Player: {payload.Player}";
            _opponent.text = $"Opponent: {payload.Opponent}";
            return base.OnShow(payload);
        }
    }
}