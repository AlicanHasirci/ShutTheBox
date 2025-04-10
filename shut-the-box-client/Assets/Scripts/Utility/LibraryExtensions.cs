using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using Revel.SceneManagement;
using Revel.UI;

namespace Utility
{
    public static class LibraryExtensions
    {
        public static Observable<Unit> Observe(
            this RevelButton button,
            CancellationToken ct = default
        )
        {
            return Observable.FromEvent(
                a => button.OnClick.AddListener(() => a()),
                a => button.OnClick.RemoveListener(() => a()),
                ct
            );
        }

        public static async UniTask ChangeTopScene(
            this ISceneController sceneController,
            string sceneName
        )
        {
            var topScene = string.Empty;
            using var enumerator = sceneController.Scenes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                topScene = enumerator.Current;
            }

            await sceneController.UnloadSceneAsync(topScene);
            await sceneController.LoadSceneAsync(sceneName);
        }
    }
}
