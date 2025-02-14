using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Planet;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Services
{
    public class PlanetGenerationService
    {
        private GameObject _spherePrefab;
        private float _radius = 4f;
        private float _smallSphereRadius = 0.6f;
        private int _subdivisions = 2;
        private Material[] _materials;
        private int _numberOfPatches = 4;
        private float _neighborDistance;

        private readonly Dictionary<long, int> _middlePointCache = new();
        private readonly List<Vector3> _vertices = new();
        private readonly Dictionary<Vector3, Material> _vertexMaterials = new();
        private readonly Dictionary<Vector3, GameObject> _sphereObjects = new();
        private readonly List<Dictionary<Material, HashSet<GameObject>>> _colorPatches = new();
        private List<int> _triangles = new();

        [Inject]
        public PlanetGenerationService(ResourceProvider resourceProvider)
        {
            var resourceProvider1 = resourceProvider;
            _materials = resourceProvider1.LoadSphereMaterials();
            _spherePrefab = resourceProvider1.LoadSpherePrefab();
        }

        public PlanetData GeneratePlanet(Transform parentTransform)
        {
            GenerateIcosphere();
            CreateSpheres(parentTransform);

            return new PlanetData(_sphereObjects.Select(v => v.Value).ToList(), _colorPatches);
        }

        private void GenerateIcosphere()
        {
            var t = (1f + Mathf.Sqrt(5f)) / 2f;

            AddVertex(new Vector3(-1, t, 0).normalized);
            AddVertex(new Vector3(1, t, 0).normalized);
            AddVertex(new Vector3(-1, -t, 0).normalized);
            AddVertex(new Vector3(1, -t, 0).normalized);
            AddVertex(new Vector3(0, -1, t).normalized);
            AddVertex(new Vector3(0, 1, t).normalized);
            AddVertex(new Vector3(0, -1, -t).normalized);
            AddVertex(new Vector3(0, 1, -t).normalized);
            AddVertex(new Vector3(t, 0, -1).normalized);
            AddVertex(new Vector3(t, 0, 1).normalized);
            AddVertex(new Vector3(-t, 0, -1).normalized);
            AddVertex(new Vector3(-t, 0, 1).normalized);

            AddFace(0, 11, 5);
            AddFace(0, 5, 1);
            AddFace(0, 1, 7);
            AddFace(0, 7, 10);
            AddFace(0, 10, 11);
            AddFace(1, 5, 9);
            AddFace(5, 11, 4);
            AddFace(11, 10, 2);
            AddFace(10, 7, 6);
            AddFace(7, 1, 8);
            AddFace(3, 9, 4);
            AddFace(3, 4, 2);
            AddFace(3, 2, 6);
            AddFace(3, 6, 8);
            AddFace(3, 8, 9);
            AddFace(4, 9, 5);
            AddFace(2, 4, 11);
            AddFace(6, 2, 10);
            AddFace(8, 6, 7);
            AddFace(9, 8, 1);

            Subdivide();
        }

        private void Subdivide()
        {
            for (var i = 0; i < _subdivisions; i++)
            {
                var newTriangles = new List<int>();
                for (var tri = 0; tri < _triangles.Count; tri += 3)
                {
                    var v1 = GetMiddlePoint(_triangles[tri], _triangles[tri + 1]);
                    var v2 = GetMiddlePoint(_triangles[tri + 1], _triangles[tri + 2]);
                    var v3 = GetMiddlePoint(_triangles[tri + 2], _triangles[tri]);

                    newTriangles.AddRange(new int[]
                    {
                        _triangles[tri], v1, v3,
                        _triangles[tri + 1], v2, v1,
                        _triangles[tri + 2], v3, v2,
                        v1, v2, v3
                    });
                }

                _triangles = newTriangles;
            }
        }

        private void GenerateColorPatches()
        {
            var centers = new List<Vector3>();
            for (var i = 0; i < _numberOfPatches; i++)
            {
                centers.Add(Random.onUnitSphere);
                _colorPatches.Add(new Dictionary<Material, HashSet<GameObject>>());
            }

            foreach (var vertex in _vertices)
            {
                var minDistance = float.MaxValue;
                var closestCenterIndex = 0;

                for (var i = 0; i < centers.Count; i++)
                {
                    var distance = Vector3.Angle(vertex, centers[i]);
                    if (!(distance < minDistance)) continue;
                    minDistance = distance;
                    closestCenterIndex = i;
                }

                _vertexMaterials[vertex] = _materials[closestCenterIndex % _materials.Length];

                if (_colorPatches[closestCenterIndex].TryGetValue(_materials[closestCenterIndex % _materials.Length],
                        out var gameObjects) == false)
                {
                    _colorPatches[closestCenterIndex].Add(_materials[closestCenterIndex % _materials.Length],
                        new HashSet<GameObject> {_sphereObjects[vertex]});
                }
                else
                {
                    gameObjects.Add(_sphereObjects[vertex]);
                }
            }
        }

        private int AddVertex(Vector3 vertex)
        {
            _vertices.Add(vertex);
            return _vertices.Count - 1;
        }

        private void AddFace(int v1, int v2, int v3)
        {
            _triangles.Add(v1);
            _triangles.Add(v2);
            _triangles.Add(v3);
        }

        private int GetMiddlePoint(int p1, int p2)
        {
            var key = ((long) Mathf.Min(p1, p2) << 32) + Mathf.Max(p1, p2);
            if (_middlePointCache.TryGetValue(key, out var ret)) return ret;

            var middle = (_vertices[p1] + _vertices[p2]) * 0.5f;
            var i = AddVertex(middle.normalized);
            _middlePointCache[key] = i;
            return i;
        }

        private void CreateSpheres(Transform parentTransform)
        {
            foreach (var vertex in _vertices)
            {
                var position = vertex * _radius;
                var sphere = Object.Instantiate(_spherePrefab, position, Quaternion.identity, parentTransform);
                _sphereObjects[vertex] = sphere;
                sphere.transform.localScale = Vector3.one * (_smallSphereRadius * 2);
            }

            GenerateColorPatches();

            foreach (var vector in _sphereObjects.Keys)
            {
                _sphereObjects[vector].GetComponent<Renderer>().material = _vertexMaterials[vector];
            }
        }
    }
}