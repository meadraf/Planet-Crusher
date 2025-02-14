using _Project.Scripts.UI;
using UnityEngine.SceneManagement;
using Zenject;

namespace _Project.Scripts.Installers
{
    public class MainMenuSceneInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<MainMenuUiNavigator>().AsSingle();
            Container.Bind<SceneController>().FromComponentInHierarchy().AsSingle();
        }
    }
}
