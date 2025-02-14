using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Planet
{
    public class PlanetData
    {
        public List<GameObject> Spheres { get; }
        public List<Dictionary<Material, HashSet<GameObject>>> ColorPatches { get; }

        public PlanetData(List<GameObject> spheres,
            List<Dictionary<Material, HashSet<GameObject>>> colorPatches)
        {
            Spheres = spheres;
            ColorPatches = colorPatches;
        }
    }
}