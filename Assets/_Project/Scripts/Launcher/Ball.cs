using _Project.Scripts.Planet;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Launcher
{
    namespace _Project.Scripts
    {
        public class Ball : MonoBehaviour
        {
            [SerializeField] private float _lifeTime = 5f;
            
            private Material _ballMaterial;
            private PlanetController _planetController;

            public class Factory : PlaceholderFactory<Ball>
            {
            }

            [Inject]
            public void Construct(PlanetController planetController)
            {
                _planetController = planetController;
            }

            public void DelayedDestroy()
            {
                Destroy(gameObject, _lifeTime);
            }
            public void SetMaterial(Material material)
            {
                _ballMaterial = material;
                GetComponent<Renderer>().material = material;
            }

            private void OnCollisionEnter(Collision collision)
            {
                if (!collision.gameObject.CompareTag("Planet")) return;
                
                var collidedRenderer = collision.gameObject.GetComponent<Renderer>();
                if (collidedRenderer != null && collidedRenderer.material.color == _ballMaterial.color)
                {
                    _planetController.RemovePatch(collision.gameObject, _ballMaterial);
                   
                }
                Destroy(gameObject);
            }
        }
    }
}