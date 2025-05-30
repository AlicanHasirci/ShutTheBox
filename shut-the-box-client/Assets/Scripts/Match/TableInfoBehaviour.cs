namespace Match
{
    using System;
    using DG.Tweening;
    using TMPro;
    using UnityEngine;

    public class TableInfoBehaviour : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _tableText;

        private IDisposable _disposable;

        private void Awake()
        {
            _tableText.color = new Color(1, 1, 1, 0);
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);

            _disposable?.Dispose();
        }

        public void SetTableInfo(string text, float fade = 0)
        {
            DOTween.Kill(this);
            _tableText.text = text;
            Sequence sequence = DOTween.Sequence().Append(_tableText.DOFade(1, .25f));
            if (fade > 0)
            {
                sequence.Insert(fade, _tableText.DOFade(0, .25f)).SetId(this);
            }
        }

        public void HideTableInfo()
        {
            DOTween.Kill(this);
            _tableText.DOFade(0, .25f).SetId(this);
        }
    }
}
