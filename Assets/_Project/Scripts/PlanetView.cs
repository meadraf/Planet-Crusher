using UnityEngine;

namespace _Project.Scripts
{
    public class PlanetView : MonoBehaviour
    {
        [SerializeField] private GameObject _planetPrefab;
        [SerializeField] private float _rotationSpeed = 6f;

        private PlanetModel _planetModel;
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        public void Initialize(PlanetModel planetModel)
        {
            _planetModel = planetModel;

            // Subscribe to updates from PlanetModel
        }

        private void Update()
        {
            _transform.Rotate(Vector3.down * (_rotationSpeed * Time.deltaTime));
        }

        private void RemoveSphere(Vector3 position)
        {
            if (_planetModel.Spheres.CurrentValue.TryGetValue(position, out var sphere))
            {
                Destroy(sphere);
            }
        }
    }
}