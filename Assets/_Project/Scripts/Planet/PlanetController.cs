using _Project.Scripts.Services;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Planet
{
    public class PlanetController
    {
        private readonly PlanetModel _planetModel;
        private readonly PlanetView _planetView;
        private readonly PlanetGenerationService _planetGenerationService;

        [Inject]
        public PlanetController(PlanetModel planetModel, PlanetView planetView,
            PlanetGenerationService planetGenerationService)
        {
            _planetModel = planetModel;
            _planetView = planetView;
            _planetGenerationService = planetGenerationService;
        }

        public void Initialize()
        {
            _planetModel.SetPlanetData(_planetGenerationService.GeneratePlanet(_planetView.transform));
            _planetView.Initialize(_planetModel);
        }

        public void RemovePatch(GameObject gameObject, Material material)
        {
            _planetModel.RemovePatch(gameObject, material);
        }
    }
}