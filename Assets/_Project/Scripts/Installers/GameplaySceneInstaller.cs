using _Project.Scripts;
using _Project.Scripts.Services;
using UnityEngine;
using Zenject;
using ResourceProvider = _Project.Scripts.Services.ResourceProvider;

namespace _Project.Scripts.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [SerializeField] private GameObject _planetPrefab;

        public override void InstallBindings()
        {
            Container.Bind<PlanetModel>().AsSingle();
            Container.Bind<ResourceProvider>().AsSingle();
            Container.Bind<PlanetGenerationService>().AsSingle();
            Container.Bind<PlanetController>().AsSingle();
            
            var planet = Container.InstantiatePrefab(_planetPrefab);
            var view = planet.GetComponent<PlanetView>();
            Container.Bind<PlanetView>().FromInstance(view).AsSingle();
            
            Container.InjectGameObject(planet);
            
            var controller = Container.Resolve<PlanetController>();
            controller.Initialize();
        }
    }
}