namespace Player.Jokers
{
    using Revel.UI;
    using TMPro;
    using UnityEngine;
    using UnityEngine.EventSystems;

    public class JokerPlayerBehaviour : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RevelImage _jokerImage;
        [SerializeField] private TMP_Text _jokerName;
        [SerializeField] private TMP_Text _jokerDescription;

        public void Initialize(JokerModel model)
        {
            _jokerImage.Sprite = model.Icon;
        }

        public void OnDrag(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            throw new System.NotImplementedException();
        }
    }
}