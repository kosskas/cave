using UnityEngine;

public class VertexLabels : MonoBehaviour
{
    public float labelOffset = 0.1f; // Odległość etykiety nad wierzchołkiem
    public float sideOffset = 0.1f; // Dodatkowa odległość w bok

    void Start()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();

        if (meshFilter != null)
        {
            Mesh mesh = meshFilter.mesh;
            Vector3[] vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                // Tworzenie obiektu tekstu nad wierzchołkiem
                GameObject label = new GameObject("VertexLabel" + i);
                TextMesh textMesh = label.AddComponent<TextMesh>();
                textMesh.text = ((char)('A' + i)).ToString();
                textMesh.anchor = TextAnchor.MiddleCenter;
                textMesh.alignment = TextAlignment.Center;
                textMesh.characterSize = 0.2f; // Dostosuj rozmiar litery

                // Ustawianie pozycji tekstu nad wierzchołkiem z uwzględnieniem odległości w bok
                label.transform.position = transform.TransformPoint(vertices[i]) + Vector3.up * labelOffset + Vector3.right * sideOffset;
                label.transform.rotation = Quaternion.identity;
                label.transform.parent = transform;
            }
        }
        else
        {
            Debug.LogError("Brakuje komponentu MeshFilter!");
        }
    }
}
