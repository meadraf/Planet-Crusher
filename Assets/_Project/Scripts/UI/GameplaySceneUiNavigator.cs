using UnityEngine;
using Zenject;

namespace _Project.Scripts.UI
{
    public class GameplaySceneUiNavigator : MonoBehaviour
    {
        private SceneController _sceneController;

        [Inject]
        public void Construct(SceneController sceneController)
        {
            _sceneController = sceneController;
        }
        
        public void OnBackButtonClicked()
        {
            _sceneController.LoadMainMenu();
        }
    }
}