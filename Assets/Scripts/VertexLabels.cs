using UnityEngine;

public class VertexLabels : MonoBehaviour
{
    public float labelOffset = 0.1f;
    public float sideOffset = 0.1f;
    Vector3[] vertices;
    GameObject[] labels;
    public Camera cam;

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
        //Parametry obrotu dziecka MainObject czyli właściwego obiektu
        //pod 0 musi być właściwy obiekt
        Quaternion rotation = transform.GetChild(0).rotation;
        
        for (int i = 0; i < labels.Length; i++)
        {
            Vector3 rotatedVertex = rotation * vertices[i];
            Vector3 worldPosition = transform.TransformPoint(rotatedVertex) + Vector3.up * labelOffset;
            labels[i].transform.position = worldPosition;
            labels[i].transform.rotation = rotation;
        }

        //TODO
        //literki mają patrzec na kamere
        //labels[i].transform.LookAt(cam.transform);
    }

}

