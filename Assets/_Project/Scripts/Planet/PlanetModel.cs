using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ObservableCollections;

namespace _Project.Scripts.Planet
{
    public class PlanetModel
    {
        public IObservableCollection<GameObject> Spheres => _spheres;
        public List<Material> UniqueMaterials { get; private set; } = new();
        
        private readonly ObservableList<GameObject> _spheres = new();
        private List<Dictionary<Material, HashSet<GameObject>>> _colorPatches = new();

        public void SetPlanetData(PlanetData planetData)
        {
            _spheres.Clear();
            _spheres.AddRange(planetData.Spheres);
            _colorPatches = planetData.ColorPatches;
            
            UpdateUniqueMaterials();
        }

        private void UpdateUniqueMaterials()
        {
            UniqueMaterials.Clear();
            foreach (var material in _colorPatches.SelectMany(dictionary => dictionary.Keys.Distinct()))
            {
                UniqueMaterials.Add(material);
            }
        }

        public void RemovePatch(GameObject sphere, Material hitMaterial)
        {
            Dictionary<Material, HashSet<GameObject>> patchToRemove = new();
            foreach (var patch in _colorPatches.Where(d=> d.ContainsKey(hitMaterial)))
            {
                if (!patch.TryGetValue(hitMaterial, out var gameObjects)) continue;
                if (!gameObjects.Contains(sphere)) continue;
                foreach (var gameObject in gameObjects)
                {
                    _spheres.Remove(gameObject);
                }

                patchToRemove = patch;
                break;
            }

            _colorPatches.Remove(patchToRemove);
            UpdateUniqueMaterials();
        }
    }
}