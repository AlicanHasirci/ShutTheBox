namespace Player.Jokers
{
    using System;
    using Revel.UI;
    using TMPro;
    using UnityEngine;

    public class JokerSelectionBehaviour : MonoBehaviour
    {
        [SerializeField]
        private RevelImage _jokerImage;

        [SerializeField]
        private TMP_Text _jokerName;

        [SerializeField]
        private TMP_Text _jokerDescription;

        private Action _onClick;

        public void Initialize(Network.Joker joker, Action onClick)
        {
            _jokerName.text = joker.ToString();
            _onClick = onClick;
        }

        public void OnClick()
        {
            _onClick?.Invoke();
        }
    }
}
