using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace _Project.Scripts.Services
{
    public class ResourceProvider
    {
        public Material[] LoadSphereMaterials()
        {
            var materials = Resources.LoadAll<Material>("SpheresMaterials");
            if (materials.Length > 0)
            {
                return materials;
            }
            
            Debug.LogError("No materials found in the catalog.");

            return null;
        }

        public GameObject LoadSpherePrefab()
        {
            var spherePrefab = Resources.Load<GameObject>("Prefabs/Sphere");

            if (spherePrefab != null)
            {
                return spherePrefab;
            }
            
            Debug.LogError("Error loading Sphere prefab");
            
            return null;
        }
    }
}
