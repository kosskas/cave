using UnityEngine;

public class VertexLabels : MonoBehaviour
{
    public float labelOffset = 0.1f;
    public float sideOffset = 0.1f;
    Vector3[] vertices;
    GameObject[] labels;

    void Start()
    {
        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();

        if (meshFilter != null)
        {
            vertices = meshFilter.mesh.vertices;
            labels = new GameObject[vertices.Length / 3];

            for (int i = 0; i < vertices.Length / 3; i++)
            {
                GameObject label = new GameObject("VertexLabel" + i);
                TextMesh textMesh = label.AddComponent<TextMesh>();
                textMesh.text = ((char)('A' + i)).ToString();
                textMesh.characterSize = 0.1f;
                textMesh.color = Color.black;
                labels[i] = label;
            }
        }
        else
        {
            Debug.LogError("MeshFilter component not found!");
        }
    }

    void Update()
    {
        for (int i = 0; i < labels.Length; i++)
        {
            Vector3 localOffset = Vector3.up * labelOffset + Vector3.right * sideOffset;
            Vector3 worldPosition = transform.TransformPoint(vertices[i]) + transform.rotation * localOffset;
            labels[i].transform.position = worldPosition;

            // Use the object's rotation directly
            labels[i].transform.rotation = transform.rotation;

            //labels[i].transform.parent = transform;
        }
    }

}

