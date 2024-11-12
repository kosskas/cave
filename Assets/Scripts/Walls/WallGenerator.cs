using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using UnityEngine;

public class WallGenerator : MonoBehaviour {

    private const float SOLID_WALL_TRANSPARENCY = 0.3f;
    // Use this for initialization
    public List<KeyValuePair<string, Vector3>> points = new List<KeyValuePair<string, Vector3>>();
    public static List<FaceInfo> faceInfoList = new List<FaceInfo>();
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //if (Input.GetKeyDown("m"))
        //{
        //    TestPunktow();
        //    
        //}
    }

    private bool CheckIfPointsAreOnTheSamePlane(List<KeyValuePair<string, Vector3>> points)
    {
        bool areOnSamePlane = true;
        if (points.Count() < 3)
        {
            return false;
        }
        else
        {
            Vector3 cords1 = points[0].Value;
            Vector3 cords2 = points[1].Value;
            Vector3 cords3 = points[2].Value;

            for (int i = 3; i < points.Count(); i++)
            {
                Vector3 cords4 = points[i].Value;

                float a11a22a33 = (cords4.x - cords1.x) * (cords4.y - cords2.y) * (cords4.z - cords3.z);
                float a12a23a31 = (cords4.y - cords1.y) * (cords4.z - cords2.z) * (cords4.x - cords3.x);
                float a13a21a32 = (cords4.z - cords1.z) * (cords4.x - cords2.x) * (cords4.y - cords3.y);
                float a13a22a31 = (cords4.z - cords1.z) * (cords4.y - cords2.y) * (cords4.x - cords3.x);
                float a11a23a32 = (cords4.x - cords1.x) * (cords4.z - cords2.z) * (cords4.y - cords3.y);
                float a12a21a33 = (cords4.y - cords1.y) * (cords4.x - cords2.x) * (cords4.z - cords3.z);
                float posPart = a11a22a33 + a12a23a31 + a13a21a32;
                float negPart = a13a22a31 + a11a23a32 + a12a21a33;
                float det = posPart - negPart;
                Debug.Log("Wyznacznik macierzy: "+ det + '\n');

                if (!Mathf.Approximately(det, 0f))
                {
                    areOnSamePlane = false;
                    break;
                }
            }

        }

        return areOnSamePlane;
    }



    public void CreateWall(List<KeyValuePair<string, Vector3>> points)
    {
        // 1. Tworzymy nowy obiekt dla ściany
        GameObject wallObject = new GameObject("Face");
        wallObject.transform.SetParent(this.transform);

        // Dodaj komponenty MeshFilter i MeshRenderer
        MeshFilter meshFilter = wallObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wallObject.AddComponent<MeshRenderer>();

        // Ustawienie materiału dla ściany (upewnij się, że materiał jest przypisany w Unity)
        meshRenderer.material = new Material(Shader.Find("Standard"));

        // Stwórz nowy mesh
        Mesh mesh = new Mesh();

        // 2. Przekształć punkty do listy Vector3
        List<Vector3> vertices = new List<Vector3>();
        foreach (var point in points)
        {
            vertices.Add(point.Value);
        }

        // 3. Przypisz wierzchołki do mesh
        mesh.vertices = vertices.ToArray();

        // 4. Tworzenie trójkątów dla obu stron
        List<int> triangles = new List<int>();

        // Tworzymy trójkąty w standardowej kolejności
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);       // pierwszy punkt jako środek
            triangles.Add(i);       // aktualny punkt
            triangles.Add(i + 1);   // następny punkt
        }

        // Tworzymy trójkąty w odwrotnej kolejności dla drugiej strony
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);       // pierwszy punkt jako środek
            triangles.Add(i + 1);   // następny punkt
            triangles.Add(i);       // aktualny punkt
        }

        // Przypisz trójkąty do mesh
        mesh.triangles = triangles.ToArray();

        // 5. Oblicz normalne, aby uzyskać prawidłowe oświetlenie
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Set the material for the MeshRenderer (you can create your own material or use an existing one)
        meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
        Color color = Color.white;
        color.a = SOLID_WALL_TRANSPARENCY;

        // Ustawienie koloru na biały
        meshRenderer.material.color = color;

       

        // 6. Przypisz mesh do MeshFilter
        meshFilter.mesh = mesh;

        List<KeyValuePair<string, Vector3>> copied_points = points
            .Select(point => new KeyValuePair<string, Vector3>(point.Key, point.Value))
            .ToList();


        faceInfoList.Add(new FaceInfo(copied_points,wallObject));
    }

    public void GenerateWall(List<KeyValuePair<string, Vector3>> points)
    {
        if (points.Count() < 3)
        {
            Debug.Log("Conajmniej 3 punkty muszą zostać wybrane, by ściana mogła powstać.");
            
        }
        else if (points.Count() == 3)
        {
            //Debug.Log(CheckIfPointsAreOnTheSamePlane(points));
            CreateWall(points);
        }
        else
        {
            if (CheckIfPointsAreOnTheSamePlane(points))
            {
                //Debug.Log(CheckIfPointsAreOnTheSamePlane(points));
                CreateWall(points);
            }
        }

        points.Clear();
        PointsList.listTextComponent.text = "";
    }

    public static List<FaceInfo> GetFaceInfosFromPointLabel(string pointLabel)
    {
        List<FaceInfo> faceInfosFromPoint = new List<FaceInfo>();

        foreach (var faceInfo in faceInfoList)
        {
            if (faceInfo.Points.Any(point => point.Key == pointLabel))
            {
                faceInfosFromPoint.Add(faceInfo);
            }
        }

        return faceInfosFromPoint;
    }

    public Dictionary<GameObject, List<string>> GetFaces()
    {
        
        Dictionary<GameObject, List<string>> ret = new Dictionary<GameObject, List<string>>();
        foreach (FaceInfo faceInfo in faceInfoList)
        {
            List<string> pointsLabels = new List<string>();
            foreach (var faceInfoPoint in faceInfo.Points)
            {
                pointsLabels.Add(faceInfoPoint.Key);
            }
            ret.Add(faceInfo.FaceObject, pointsLabels);
        }
        return ret;
    }

    
}
