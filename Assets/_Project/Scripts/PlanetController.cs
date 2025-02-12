using _Project.Scripts.Services;
using UnityEngine;
using Zenject;

namespace _Project.Scripts
{
    public class PlanetController : IInitializable, ITickable
    {
        private readonly PlanetModel _planetModel;
        private readonly PlanetView _planetView;
        private readonly PlanetGenerationService _planetGenerationService;
    
        private Transform _planetTransform;
        private const float RotationSpeed = 6f;

        [Inject]
        public PlanetController(PlanetModel planetModel, PlanetView planetView, PlanetGenerationService planetGenerationService)
        {
            _planetModel = planetModel;
            _planetView = planetView;
            _planetGenerationService = planetGenerationService;
        }

        public void Initialize()
        {
            // Generate the planet using the service
            _planetModel.SetPlanetData(_planetGenerationService.GeneratePlanet(_planetView.transform));

            // Apply generated data to the view
            _planetView.Initialize(_planetModel);

            _planetTransform = _planetView.transform;
        }

        public void Tick()
        {
            RotatePlanet();
        }

        private void RotatePlanet()
        {
            if (_planetTransform != null)
            {
                _planetTransform.Rotate(Vector3.up * (Time.deltaTime * RotationSpeed));
            }
        }

        public void RemovePatch(Vector3 position, Material material)
        {
            var removedSpheres = _planetModel.RemovePatch(position, material);
            
        }
    }
}