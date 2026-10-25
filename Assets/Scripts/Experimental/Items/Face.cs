using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Experimental.Items
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class Face : MonoBehaviour, IRaycastable, IColorable
    {
        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private MeshCollider _meshCollider;
        private Material _material;

        private Color _color;

        private MeshBuilder _mb;

        private MeshBuilder Mb
        {
            get
            {
                if (_mb == null)
                    _mb = (MeshBuilder)FindObjectOfType(typeof(MeshBuilder));

                return _mb;
            }
        }


        // Properties

        public List<KeyValuePair<string, Vector3>> Points { get; private set; }


        // IColorable

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;

                if (_material != null)
                    _material.color = _color;
            }
        }


        // IRaycastable

        public void OnHoverAction(Action<GameObject> action)
        {
            action(gameObject);
        }

        public void OnHoverEnter()
        {
            var color = ReconstructionInfo.FOCUSED;
            color.a = ReconstructionInfo.SOLID_FACE_TRANSPARENCY_FOCUSED;

            if (_material != null)
                _material.color = color;
        }

        public void OnHoverExit()
        {
            if (_material != null)
                _material.color = _color;
        }


        // Unity staff 

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();

            _mesh = new Mesh();
            _mesh.name = "Face Mesh";
            _meshFilter.sharedMesh = _mesh;

            var shader = Resources.Load<Shader>("Shaders/UnlitTransparentColor");
            if (shader == null)
            {
                Debug.LogError("Failed to load UnlitTransparentColor shader.", this);
                return;
            }

            _material = new Material(shader);
            _meshRenderer.sharedMaterial = _material;

            _meshCollider.convex = false;

            var playerCollider = FindObjectOfType<PlayerController>().GetComponent<CharacterController>();
            Physics.IgnoreCollision(playerCollider, _meshCollider);

            var color = ReconstructionInfo.FACE_COLOR;
            color.a = ReconstructionInfo.SOLID_FACE_TRANSPARENCY;
            Color = color;
        }

        private void OnDestroy()
        {
            Mb?.RemoveFace(gameObject);

            if (_meshCollider != null)
                _meshCollider.sharedMesh = null;

            if (_meshFilter != null)
                _meshFilter.sharedMesh = null;

            if (_meshRenderer != null)
                _meshRenderer.sharedMaterial = null;

            if (_mesh != null)
                Destroy(_mesh);

            if (_material != null)
                Destroy(_material);
        }


        // Public

        public bool CreateFace(
            List<KeyValuePair<string, Vector3>> points
            )
        {
            if (points == null || !CheckIfPointsAreOnTheSamePlane(points))
            {
                Debug.LogWarning("Invalid face vertices.", this);
                ClearFace();
                return false;
            }

            // Convert world coordinates to local coordinates
            var vertices = new List<Vector3>(points.Count);

            foreach (var point in points)
                vertices.Add(transform.InverseTransformPoint(point.Value));

            // Triangulate the polygon (including concave polygons)
            var frontTriangles = Triangulate(vertices);

            if (frontTriangles == null)
            {
                Debug.LogWarning("Face triangulation failed.", this);
                ClearFace();
                return false;
            }

            // Duplicate vertices to create front and back faces
            var meshVertices = new List<Vector3>(vertices.Count * 2);
            meshVertices.AddRange(vertices);
            meshVertices.AddRange(vertices);

            var triangles = new List<int>(frontTriangles.Count * 2);
            int offset = vertices.Count;

            for (int i = 0; i < frontTriangles.Count; i += 3)
            {
                int a = frontTriangles[i];
                int b = frontTriangles[i + 1];
                int c = frontTriangles[i + 2];

                // Front face
                triangles.Add(a);
                triangles.Add(b);
                triangles.Add(c);

                // Back face (reversed winding)
                triangles.Add(a + offset);
                triangles.Add(c + offset);
                triangles.Add(b + offset);
            }

            // Update the existing mesh
            _meshCollider.sharedMesh = null;

            _mesh.Clear();
            _mesh.SetVertices(meshVertices);
            _mesh.SetTriangles(triangles, 0);
            _mesh.RecalculateNormals();
            _mesh.RecalculateBounds();

            _meshCollider.sharedMesh = _mesh;

            Points = new List<KeyValuePair<string, Vector3>>(points);

            return true;
        }

        public void SetEnabled(
            bool enabled
            )
        {
            if (_meshRenderer == null || _meshCollider == null) return;
            if (_meshRenderer.enabled == enabled) return;
            
            _meshRenderer.enabled = enabled;
            _meshCollider.enabled = enabled;
        }


        // Private

        private void ClearFace()
        {
            _meshCollider.sharedMesh = null;
            _mesh.Clear();
            Points = null;
        }

        private static List<int> Triangulate(
            List<Vector3> vertices
            )
        {
            const float eps = 1e-6f;
            int count = vertices.Count;

            if (count < 3)
                return null;

            // Calculate the polygon normal using Newell's method
            var normal = Vector3.zero;

            for (int i = 0; i < count; i++)
            {
                var a = vertices[i];
                var b = vertices[(i + 1) % count];

                normal.x += (a.y - b.y) * (a.z + b.z);
                normal.y += (a.z - b.z) * (a.x + b.x);
                normal.z += (a.x - b.x) * (a.y + b.y);
            }

            if (normal.sqrMagnitude < eps * eps)
                return null;

            normal.Normalize();

            // Build a 2D coordinate system on the polygon plane
            var reference = Mathf.Abs(normal.y) < 0.9f
                ? Vector3.up
                : Vector3.right;

            var u = Vector3.Cross(reference, normal).normalized;
            var v = Vector3.Cross(normal, u);

            var projected = new List<Vector2>(count);
            var origin = vertices[0];

            for (int i = 0; i < count; i++)
            {
                var delta = vertices[i] - origin;

                projected.Add(new Vector2(
                    Vector3.Dot(delta, u),
                    Vector3.Dot(delta, v)
                ));
            }

            var remaining = new List<int>(count);

            for (int i = 0; i < count; i++)
                remaining.Add(i);

            // Remove duplicate and collinear vertices
            bool changed;

            do
            {
                changed = false;

                if (remaining.Count <= 3)
                    break;

                for (int i = 0; i < remaining.Count; i++)
                {
                    int n = remaining.Count;

                    var a = projected[remaining[(i + n - 1) % n]];
                    var b = projected[remaining[i]];
                    var c = projected[remaining[(i + 1) % n]];

                    bool duplicate =
                        (b - a).sqrMagnitude < eps * eps ||
                        (c - b).sqrMagnitude < eps * eps;

                    bool collinear =
                        Mathf.Abs(Cross2D(a, b, c)) < eps &&
                        Vector2.Dot(b - a, c - b) >= 0f;

                    if (duplicate || collinear)
                    {
                        remaining.RemoveAt(i);
                        changed = true;
                        break;
                    }
                }
            }
            while (changed);

            if (remaining.Count < 3)
                return null;

            // Determine polygon winding
            float area = 0f;

            for (int i = 0; i < remaining.Count; i++)
            {
                var a = projected[remaining[i]];
                var b = projected[remaining[(i + 1) % remaining.Count]];

                area += a.x * b.y - b.x * a.y;
            }

            if (Mathf.Abs(area) < eps)
                return null;

            float winding = area > 0f ? 1f : -1f;

            var triangles = new List<int>((remaining.Count - 2) * 3);

            // Ear clipping
            while (remaining.Count > 3)
            {
                bool earFound = false;

                for (int i = 0; i < remaining.Count; i++)
                {
                    int n = remaining.Count;

                    int prev = remaining[(i + n - 1) % n];
                    int curr = remaining[i];
                    int next = remaining[(i + 1) % n];

                    var a = projected[prev];
                    var b = projected[curr];
                    var c = projected[next];

                    // An ear must be convex
                    if (Cross2D(a, b, c) * winding <= eps)
                        continue;

                    bool containsPoint = false;

                    // Check whether another vertex lies inside the ear
                    foreach (int index in remaining)
                    {
                        if (index == prev || index == curr || index == next)
                            continue;

                        if (PointInTriangle(
                            projected[index], a, b, c, winding))
                        {
                            containsPoint = true;
                            break;
                        }
                    }

                    if (containsPoint)
                        continue;

                    triangles.Add(prev);
                    triangles.Add(curr);
                    triangles.Add(next);

                    remaining.RemoveAt(i);
                    earFound = true;
                    break;
                }

                if (!earFound)
                    return null;
            }

            // Add the final triangle
            triangles.Add(remaining[0]);
            triangles.Add(remaining[1]);
            triangles.Add(remaining[2]);

            return triangles;
        }

        private static float Cross2D(
            Vector2 a,
            Vector2 b,
            Vector2 c
            )
        {
            return (b.x - a.x) * (c.y - a.y)
                 - (b.y - a.y) * (c.x - a.x);
        }

        private static bool PointInTriangle(
            Vector2 p,
            Vector2 a,
            Vector2 b,
            Vector2 c,
            float winding
            )
        {
            const float eps = 1e-6f;

            return Cross2D(a, b, p) * winding >= -eps &&
                   Cross2D(b, c, p) * winding >= -eps &&
                   Cross2D(c, a, p) * winding >= -eps;
        }

        private static bool CheckIfPointsAreOnTheSamePlane(
            List<KeyValuePair<string, Vector3>> points
            )
        {
            const float tolerance = 0.01f;
            const float epsSqr = 1e-12f;

            if (points == null || points.Count < 3)
                return false;

            var origin = points[0].Value;
            var normal = Vector3.zero;

            // Find three non-collinear points
            for (int i = 1; i < points.Count - 1; i++)
            {
                for (int j = i + 1; j < points.Count; j++)
                {
                    normal = Vector3.Cross(
                        points[i].Value - origin,
                        points[j].Value - origin
                    );

                    if (normal.sqrMagnitude >= epsSqr)
                        break;
                }

                if (normal.sqrMagnitude >= epsSqr)
                    break;
            }

            if (normal.sqrMagnitude < epsSqr)
                return false;

            normal.Normalize();

            // Check the distance of every point from the plane
            foreach (var point in points)
            {
                float distance = Mathf.Abs(Vector3.Dot(
                    point.Value - origin,
                    normal
                ));

                if (distance > tolerance)
                    return false;
            }

            return true;
        }
    }
}
