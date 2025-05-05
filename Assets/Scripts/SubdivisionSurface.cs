#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SubdivisionSurface: MonoBehaviour {
  private const float RotationSpeed = 0.1f;

  private MeshFilter? _meshFilter;

  private float Surface(float theta, float phi) {
    return 1;
  }

  private Vector3 AdjustVertex(Vector3 vertex) {
    float theta = Mathf.Atan2(vertex.y, vertex.z);
    float phi = Mathf.Atan2(vertex.y, vertex.x);

    return Surface(theta, phi) * vertex.normalized;
  }

  private void AdjustVertices() {
    if (_meshFilter == null) {
      return;
    }

    Mesh mesh = _meshFilter.mesh;
    Vector3[] vertices = mesh.vertices;

    for (int i = 0; i < vertices.Length; i++) {
      vertices[i] = AdjustVertex(vertices[i]);
    }

    mesh.vertices = vertices;
    mesh.RecalculateNormals();
  }

  private Vector3 SubdivideEdge(Vector3 a, Vector3 b) {
    Vector3 ab = (a + b) / 2;

    return AdjustVertex(ab);
  }

  private void Subdivide() {
    if (_meshFilter == null) {
      return;
    }

    Mesh mesh = _meshFilter.mesh;
    int[] triangles = mesh.triangles;
    Vector3[] vertices = mesh.vertices;

    Dictionary<(int, int), int> edgeToVertex = new();
    int[] newTriangles = new int[4 * triangles.Length];
    List<Vector3> newVertices = vertices.ToList();

    int a, b, c, ab, bc, ca;
    for (int i = 0; i < triangles.Length / 3; i++) {
      a = triangles[3 * i];
      b = triangles[3 * i + 1];
      c = triangles[3 * i + 2];

      if (!edgeToVertex.TryGetValue((a, b), out ab)) {
        newVertices.Add(SubdivideEdge(vertices[a], vertices[b]));
        ab = newVertices.Count() - 1;
        edgeToVertex[(a, b)] = ab;
      }
      if (!edgeToVertex.TryGetValue((b, c), out bc)) {
        newVertices.Add(SubdivideEdge(vertices[b], vertices[c]));
        bc = newVertices.Count() - 1;
        edgeToVertex[(b, c)] = bc;
      }
      if (!edgeToVertex.TryGetValue((c, a), out ca)) {
        newVertices.Add(SubdivideEdge(vertices[c], vertices[a]));
        ca = newVertices.Count() - 1;
        edgeToVertex[(c, a)] = ca;
      }

      newTriangles[4 * 3 * i] = a;
      newTriangles[4 * 3 * i + 1] = ab;
      newTriangles[4 * 3 * i + 2] = ca;
      newTriangles[4 * 3 * i + 3] = ca;
      newTriangles[4 * 3 * i + 4] = ab;
      newTriangles[4 * 3 * i + 5] = bc;
      newTriangles[4 * 3 * i + 6] = c;
      newTriangles[4 * 3 * i + 7] = ca;
      newTriangles[4 * 3 * i + 8] = bc;
      newTriangles[4 * 3 * i + 9] = bc;
      newTriangles[4 * 3 * i + 10] = ab;
      newTriangles[4 * 3 * i + 11] = b;
    }

    mesh.vertices = newVertices.ToArray();
    mesh.triangles = newTriangles;
    mesh.RecalculateNormals();
  }

  private void Awake() {
    _meshFilter = GetComponent<MeshFilter>();

    Mesh mesh = _meshFilter.mesh;
    Vector3[] vertices = mesh.vertices;
    int[] triangles = mesh.triangles;

    mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    AdjustVertices();
  }

  private void Update() {
    transform.Rotate(0, RotationSpeed, 0);

    if (Input.GetKeyDown(KeyCode.Space)) {
      Subdivide();
    }
  }
}
