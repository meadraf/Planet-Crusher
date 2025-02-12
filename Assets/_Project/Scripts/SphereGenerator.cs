using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;
using ResourceProvider = _Project.Scripts.Services.ResourceProvider;

public class SphereGenerator : MonoBehaviour
{
    [SerializeField] private GameObject _spherePrefab;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private float _smallSphereRadius = 0.2f;
    [SerializeField] private int _subdivisions = 2;
    [SerializeField] private Material[] _materials;
    [SerializeField] private int _numberOfPatches = 3; // Number of color patches
    [SerializeField] private float _rotationSpeed = 6f;
    [SerializeField] private float _neighborDistance = 0.5f;

    private ResourceProvider _resourceProvider;
    private Transform _transform;
    private Dictionary<long, int> _middlePointCache;
    private List<Vector3> _vertices;
    private List<int> _triangles;
    private Dictionary<Vector3, Material> _vertexMaterials;
    private Dictionary<Vector3, bool> _visitedVertices;

    private Dictionary<Vector3, GameObject> sphereObjects = new Dictionary<Vector3, GameObject>();
    private Dictionary<Vector3, List<Vector3>> neighborMap = new Dictionary<Vector3, List<Vector3>>();

    private void Start()
    {
        _resourceProvider = new ResourceProvider();
        _materials = _resourceProvider.LoadSphereMaterials();
        foreach (var material in _materials)
        {
            Debug.Log(material.name);
        }
        GenerateIcosphere();
        _transform = GetComponent<Transform>();
    }

    private void Update()
    {
        _transform.Rotate(Vector3.up * (Time.deltaTime * _rotationSpeed));
        if (Input.GetKey(KeyCode.Space))
        {
            Debug.Log($"Key pressed at {Time.time}");
            var pos = new Vector3(-2.1f, 3.40f, 0f);
            RemovePatch(pos, _materials[1]);
            Debug.Log($"{pos.x}, {pos.y}, {pos.z}, material = {_materials[1].name}");
        }
    }

    private void GenerateIcosphere()
    {
        _vertices = new List<Vector3>();
        _triangles = new List<int>();
        _middlePointCache = new Dictionary<long, int>();
        _vertexMaterials = new Dictionary<Vector3, Material>();

        // Create initial icosahedron
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

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

        // Add faces
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

        // Subdivide
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

        // Generate color patches using center points
        GenerateColorPatches();

        // Create spheres
        var uniqueVertices = new HashSet<Vector3>();
        foreach (var vertex in _vertices)
        {
            if (!uniqueVertices.Add(vertex)) continue;
            var position = vertex * _radius;
            CreateSphere(position, _vertexMaterials[vertex]);
        }

        BuildNeighborMap();
        foreach (var kvp in neighborMap)
        {
            Debug.Log($"Sphere at {kvp.Key} has {kvp.Value.Count} neighbors");
        }
    }

    private void GenerateColorPatches()
    {
        // Generate random center points for each patch
        List<Vector3> centers = new List<Vector3>();
        for (int i = 0; i < _numberOfPatches; i++)
        {
            // Generate random spherical coordinates
            float phi = Random.Range(0f, Mathf.PI);
            float theta = Random.Range(0f, Mathf.PI * 2f);

            // Convert to Cartesian coordinates
            Vector3 center = new Vector3(
                Mathf.Sin(phi) * Mathf.Cos(theta),
                Mathf.Sin(phi) * Mathf.Sin(theta),
                Mathf.Cos(phi)
            ).normalized;

            centers.Add(center);
        }

        // Assign colors based on closest center point
        foreach (Vector3 vertex in _vertices)
        {
            float minDistance = float.MaxValue;
            int closestCenterIndex = 0;

            for (int i = 0; i < centers.Count; i++)
            {
                // Use angular distance instead of Euclidean distance
                float distance = Vector3.Angle(vertex, centers[i]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCenterIndex = i;
                }
            }

            // Assign material based on closest center
            _vertexMaterials[vertex] = _materials[closestCenterIndex % _materials.Length];
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
        long key = ((long) Mathf.Min(p1, p2) << 32) + Mathf.Max(p1, p2);

        if (_middlePointCache.TryGetValue(key, out var ret))
            return ret;

        Vector3 middle = (_vertices[p1] + _vertices[p2]) * 0.5f;
        int i = AddVertex(middle.normalized);
        _middlePointCache.Add(key, i);
        return i;
    }

    void CreateSphere(Vector3 position, Material material)
    {
        GameObject sphere = Instantiate(_spherePrefab, position, Quaternion.identity);
        sphere.transform.parent = transform;
        sphere.transform.localScale = Vector3.one * _smallSphereRadius * 2;

        // Add collider if not already present
        if (!sphere.GetComponent<SphereCollider>())
        {
            sphere.AddComponent<SphereCollider>();
        }

        // Add rigidbody to make it static but detectable in collisions
        Rigidbody rb = sphere.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        sphere.GetComponent<Renderer>().material = material;

        // Store reference to the sphere
        sphereObjects[position] = sphere;
        _vertexMaterials[position] = material;
    }

    void BuildNeighborMap()
    {
        neighborMap.Clear();

        // Convert dictionary keys to list for easier access
        List<Vector3> positions = new List<Vector3>(sphereObjects.Keys);
    
        // Create a list to store all distances
        List<float> distances = new List<float>();

        foreach (Vector3 pos1 in positions)
        {
            List<(Vector3, float)> nearestNeighbors = new List<(Vector3, float)>();

            foreach (Vector3 pos2 in positions)
            {
                if (pos1 == pos2) continue;

                float distance = Vector3.Distance(pos1, pos2);
                nearestNeighbors.Add((pos2, distance));
                distances.Add(distance);
            }

            // Sort by distance and take the 6 closest neighbors
            nearestNeighbors.Sort((a, b) => a.Item2.CompareTo(b.Item2));
            List<Vector3> neighbors = nearestNeighbors.Take(6).Select(n => n.Item1).ToList();

            // Store in the map
            neighborMap[pos1] = neighbors;
        }

        // Determine an optimal _neighborDistance (average of the 6th nearest distance)
        if (distances.Count >= 6)
        {
            distances.Sort();
            _neighborDistance = distances[5]; // Use the 6th closest distance as a baseline
        }

        Debug.Log($"Neighbor map built. Optimal _neighborDistance: {_neighborDistance}");
    }


    void RemovePatch(Vector3 hitPosition, Material hitMaterial)
    {
        if (sphereObjects.Count == 0) return;

        // Find closest sphere
        Vector3 closestPosition = Vector3.zero;
        float minDistance = float.MaxValue;
        foreach (var pos in sphereObjects.Keys)
        {
            float distance = Vector3.Distance(pos, hitPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPosition = pos;
            }
        }

        // If no sphere is close enough, return
        if (minDistance > _neighborDistance * 2 || !_vertexMaterials.ContainsKey(closestPosition))
        {
            Debug.Log("No valid sphere found near the hit position.");
            return;
        }

        // Ensure the material matches
        if (!_vertexMaterials.TryGetValue(closestPosition, out var material) || material.name != hitMaterial.name)
        {
            Debug.Log("Material mismatch or missing sphere.");
            return;
        }

        Debug.Log($"Starting patch removal at {closestPosition} with material {hitMaterial.name}");

        // Flood fill to find the patch
        HashSet<Vector3> patchToRemove = new HashSet<Vector3>();
        Queue<Vector3> positionsToCheck = new Queue<Vector3>();
        positionsToCheck.Enqueue(closestPosition);
        patchToRemove.Add(closestPosition);

        while (positionsToCheck.Count > 0)
        {
            Vector3 currentPos = positionsToCheck.Dequeue();

            if (!neighborMap.TryGetValue(currentPos, out var neighbors))
            {
                Debug.Log($"No neighbors found for {currentPos}");
                continue;
            }

            foreach (Vector3 neighborPos in neighbors)
            {
                if (patchToRemove.Contains(neighborPos)) continue;

                if (_vertexMaterials.TryGetValue(neighborPos, out var neighborMaterial) &&
                    neighborMaterial.name == hitMaterial.name)
                {
                    patchToRemove.Add(neighborPos);
                    positionsToCheck.Enqueue(neighborPos);
                }
            }
        }

        // Remove all spheres in the patch
        foreach (Vector3 pos in patchToRemove)
        {
            if (sphereObjects.TryGetValue(pos, out var sphere))
            {
                Destroy(sphere);
                sphereObjects.Remove(pos);
            }

            _vertexMaterials.Remove(pos);
        }

        Debug.Log($"Removed {patchToRemove.Count} spheres.");
    }
}