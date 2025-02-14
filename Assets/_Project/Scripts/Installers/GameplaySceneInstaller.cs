using _Project.Scripts.Launcher;
using _Project.Scripts.Launcher._Project.Scripts;
using _Project.Scripts.Planet;
using _Project.Scripts.Services;
using _Project.Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using ResourceProvider = _Project.Scripts.Services.ResourceProvider;

namespace _Project.Scripts.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [SerializeField] private GameObject _planetPrefab;
        [SerializeField] private GameObject _ballPrefab;
        [SerializeField] private Transform _launchPoint;

        public override void InstallBindings()
        {
            Container.Bind<PlanetModel>().AsSingle();
            Container.Bind<ResourceProvider>().AsSingle();
            Container.Bind<PlanetGenerationService>().AsSingle();
            Container.Bind<PlanetController>().AsSingle();
            Container.Bind<SceneManager>().AsSingle();
            
            var planet = Container.InstantiatePrefab(_planetPrefab);
            var view = planet.GetComponent<PlanetView>();
            Container.Bind<PlanetView>().FromInstance(view).AsSingle();
            
            Container.Bind<SceneController>().FromComponentInHierarchy().AsSingle();
            Container.Bind<BallLauncher>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInstance(_ballPrefab).WhenInjectedInto<BallLauncher>();
            Container.BindInstance(_launchPoint).WhenInjectedInto<BallLauncher>();
            
            Container.BindFactory<Ball, Ball.Factory>().FromComponentInNewPrefab(_ballPrefab);
            
            Container.Bind<GameplayInput>().AsSingle();
            
            Container.InjectGameObject(planet);
            
            var controller = Container.Resolve<PlanetController>();
            controller.Initialize();
        }
    }
}