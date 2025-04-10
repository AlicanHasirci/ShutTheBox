namespace App
{
    using Network;
    using Revel.SceneManagement;
    using VContainer.Unity;

    public class AppController : IPostStartable
    {
        private readonly INetworkService _networkService;
        private readonly ISceneController _sceneController;

        public AppController(INetworkService networkService, ISceneController sceneController)
        {
            _networkService = networkService;
            _sceneController = sceneController;
        }

        public async void PostStart()
        {
            try
            {
                await _networkService.ConnectAsync();
            }
            finally
            {
                await _sceneController.LoadSceneAsync("MenuScene");
            }

        }
    }
}