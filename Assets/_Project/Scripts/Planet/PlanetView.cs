using ObservableCollections;
using R3;
using UnityEngine;

namespace _Project.Scripts.Planet
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

            _planetModel.Spheres.ObserveRemove().Subscribe(s => RemoveSphere(s.Value));
        }

        private void Update()
        {
            _transform.Rotate(Vector3.down * (_rotationSpeed * Time.deltaTime));
        }

        private static void RemoveSphere(GameObject sphere)
        {
            Destroy(sphere);
        }
    }
}