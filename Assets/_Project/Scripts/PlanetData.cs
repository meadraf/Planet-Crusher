using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts
{
    public class PlanetData
    {
        public Dictionary<Vector3, GameObject> Spheres { get; }
        public Dictionary<Vector3, List<Vector3>> NeighborMap { get; }
        public Dictionary<Vector3, Material> Materials { get; }

        public PlanetData(Dictionary<Vector3, GameObject> spheres, 
            Dictionary<Vector3, List<Vector3>> neighborMap,
            Dictionary<Vector3, Material> materials)
        {
            Spheres = spheres;
            NeighborMap = neighborMap;
            Materials = materials;
        }
    }
}