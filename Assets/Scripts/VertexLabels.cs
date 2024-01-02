using System.Collections.Generic;
using UnityEngine;

/*
 * Obiekt tej klasy powinien rysować litery dopiero gdy SolidImporter załaduje kształt
 * solidImporter wywołuje InitLabels
 * VertexLabels vl = FindObjectOfType<VertexLabels>();
*/
public class VertexLabels : MonoBehaviour
{
    public float labelOffset = 0.1f;
    public float sideOffset = 0.1f;
    public Font font;
    Vector3[] vertices;
    GameObject[] labels = null;
    GameObject player;
    void Start()
    {
        player = GameObject.Find("FPSPlayer");
        if (player == null)
        {
            Debug.LogError("Brak obiektu FPSPlayer potrzebnego do działania VertexLabels.cs");
        }
        InitLabels(null); //Do wywalenia
    }

    void Update()
    {
        if (Input.GetKey("v"))
        {
            InitLabels(null);
        }
        if (transform.childCount > 0)
        {
            //Parametry obrotu dziecka MainObject czyli właściwego obiektu
            //pod 0 musi być właściwy obiekt
            Quaternion rotation = transform.GetChild(0).rotation;

            for (int i = 0; i < labels.Length; i++)
            {
                Vector3 rotatedVertex = rotation * vertices[i];
                Vector3 worldPosition = transform.TransformPoint(rotatedVertex) + Vector3.up * labelOffset;
                labels[i].transform.position = worldPosition;
                Vector3 directionToPlayer = (player.transform.position - labels[i].transform.position).normalized;

                //literki są twarzą do postaci gracza
                labels[i].transform.rotation = Quaternion.LookRotation(-directionToPlayer);
            }
        }
    }

    public void InitLabels(string[] labelsnames)
    {
        ClearLabels();

        MeshFilter meshFilter = GetComponentInChildren<MeshFilter>();

        if (meshFilter != null)
        {
            //vertices = meshFilter.mesh.vertices;
            vertices = GetUniqueVertices(meshFilter.mesh.vertices);
            Debug.Log(vertices.Length);
            labels = new GameObject[vertices.Length];

            if (labelsnames == null)
                labelsnames = GetBaseLabels();

            for (int i = 0; i < labels.Length; i++)
            {
                GameObject label = new GameObject("VertexLabel" + i);
                TextMesh textMesh = label.AddComponent<TextMesh>();
                textMesh.text = labelsnames[i];
                textMesh.characterSize = 0.1f;
                textMesh.color = Color.black;
                textMesh.font = font;
                labels[i] = label;
            }
        }
        else
        {
            Debug.LogError("Brak MeshFilter potrzebnego do działania VertexLabels.cs");
        }
    }
    string[] GetBaseLabels()
    {
        int MAXLABELSNUMBER = 50; //max liczba wierzchołków do nazwania
        //teoretyczna max liczba dla tego algorytmu to 26+26*26 = 702 wierzchołków
        
        string[] labelsnames = new string[MAXLABELSNUMBER];
        for (int i = 0; i < labelsnames.Length; i++)
        {
            labelsnames[i] = ((char)('A' + i)).ToString();
        }
        //jeśli jest więcej
        for (int i = 26; i < labelsnames.Length; i++)
        {
            char first = (char)('A' + (i - 26 / 26));
            char second = (char)('A' + (i - 26 % 26));
            labelsnames[i] = $"{first}{second}";
        }
        return labelsnames;
    }

    void ClearLabels()
    {
        //jeśli był jakiś obiekt wcześniej to usuń dane o nim
        if (labels != null)
        {
            for (int i = 0; i < labels.Length; i++)
                Destroy(labels[i]);
            labels = null;
        }
    
    }

    Vector3[] GetUniqueVertices(Vector3[] meshVertices)
    {
        HashSet<Vector3> set = new HashSet<Vector3>();
        for (int i = 0; i < meshVertices.Length; i++)
        {
            set.Add(meshVertices[i]);
        }
        Vector3[] uniquevertices = new Vector3[set.Count];
        set.CopyTo(uniquevertices);
        return uniquevertices;
    }
}

