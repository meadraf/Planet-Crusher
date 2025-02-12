using System.Collections.Generic;
using R3;
using UnityEngine;

namespace _Project.Scripts
{
    public class PlanetModel
    {
        private readonly ReactiveProperty<Dictionary<Vector3, GameObject>> _spheres = new();
        private Dictionary<Vector3, List<Vector3>> _neighborMap = new();
        private Dictionary<Vector3, Material> _sphereMaterials = new();

        public ReadOnlyReactiveProperty<Dictionary<Vector3, GameObject>> Spheres => _spheres;

        public void SetPlanetData(PlanetData planetData)
        {
            _spheres.Value = planetData.Spheres;
            _neighborMap = planetData.NeighborMap;
            _sphereMaterials = planetData.Materials;
        }

        public Dictionary<Vector3, GameObject> GetSpheres()
        {
            return _spheres.Value;
        }

        public List<GameObject> RemovePatch(Vector3 position, Material targetMaterial)
        {
            List<GameObject> removedSpheres = new();
            HashSet<Vector3> visited = new();
            Queue<Vector3> queue = new();

            if (!_spheres.Value.ContainsKey(position) || _sphereMaterials[position] != targetMaterial)
                return removedSpheres;

            queue.Enqueue(position);
            visited.Add(position);

            while (queue.Count > 0)
            {
                Vector3 current = queue.Dequeue();

                if (_spheres.Value.TryGetValue(current, out GameObject sphere))
                {
                    removedSpheres.Add(sphere);
                    _spheres.Value.Remove(current);
                }

                if (_neighborMap.TryGetValue(current, out List<Vector3> neighbors))
                {
                    foreach (Vector3 neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor) && _sphereMaterials[neighbor] == targetMaterial)
                        {
                            queue.Enqueue(neighbor);
                            visited.Add(neighbor);
                        }
                    }
                }
            }

            return removedSpheres;
        }
    }
}