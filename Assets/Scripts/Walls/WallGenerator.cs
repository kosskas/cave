using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using JetBrains.Annotations;
using UnityEngine;

public class WallGenerator : MonoBehaviour {

	// Use this for initialization
    public List<KeyValuePair<string, Vector3>> points = new List<KeyValuePair<string, Vector3>>();
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

    //private void TestPunktow()
    //{
    //    Point point1 = new Point();
    //    Point point2 = new Point();
    //    Point point3 = new Point();
    //    Point point4 = new Point();
    //    Point point5 = new Point();
    //    point1.SetCoordinates(new Vector3(1f,2f,3f));
    //    point2.SetCoordinates(new Vector3(4f,5f,6f));
    //    point3.SetCoordinates(new Vector3(7f, 0f, 5f));
    //    point4.SetCoordinates(new Vector3(3f, 1f, 4f));
    //    point5.SetCoordinates(new Vector3(2f, 4f, 2f));
    //    List<Point> points = new List<Point>
    //    {
    //        point1,
    //        point2,
    //        point3,
    //        point4,
    //        point5
    //    };

    //    CheckIfPointsAreOnTheSamePlane(points);


    //}

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

        // 6. Przypisz mesh do MeshFilter
        meshFilter.mesh = mesh;
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
}
