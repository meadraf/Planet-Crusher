using UnityEngine;
using Zenject;

namespace _Project.Scripts.UI
{
    public class MainMenuUiNavigator : MonoBehaviour
    {
        private SceneController _sceneController;

        [Inject]
        public void Construct(SceneController sceneController)
        {
            _sceneController = sceneController;
        }
        
        public void OnPlayButtonClicked()
        {
           _sceneController.LoadGameplay();
        }
    }
}
