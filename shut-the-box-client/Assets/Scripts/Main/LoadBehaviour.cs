using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using Revel.SceneManagement;
using Revel.UI.Util;
using UnityEngine;
using VContainer;

namespace Main
{
    public class LoadBehaviour : MonoBehaviour
    {
        [Inject]
        public ISceneController SceneController;

        [SerializeField]
        private CanvasGroup _canvasGroup;
        private IDisposable _disposable;
        private bool _visible = true;

        private void Awake()
        {
            _disposable = SceneController.ChangeInProgress.Subscribe(Change);
        }

        private void OnDestroy()
        {
            _disposable?.Dispose();
        }

        private void Change(bool changeInProgress)
        {
            if (changeInProgress)
                Show().Forget();
            else
                Hide().Forget();
        }

        private async UniTaskVoid Show()
        {
            if (_visible)
                return;
            _visible = true;
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            await _canvasGroup.DOFade(1, .25f).ToUniTask();
        }

        private async UniTaskVoid Hide()
        {
            if (!_visible)
                return;
            _canvasGroup.alpha = 1;
            await _canvasGroup.DOFade(0, .25f).ToUniTask();
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            _visible = false;
        }
    }
}
