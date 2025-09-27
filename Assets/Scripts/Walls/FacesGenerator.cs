using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Assets.Scripts;
using JetBrains.Annotations;
using UnityEngine;

public class FacesGenerator : MonoBehaviour {

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

                const float eps = 0.01f;
                if (!(Mathf.Abs(det - 0f) < eps))
                {
                    areOnSamePlane = false;
                    break;
                }
            }

        }

        return areOnSamePlane;
    }



    public void CreateFace(List<KeyValuePair<string, Vector3>> points)
    {
        
        GameObject wallObject = new GameObject("Face");
        wallObject.transform.SetParent(this.transform);

        MeshFilter meshFilter = wallObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = wallObject.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));

        Mesh mesh = new Mesh();

        
        List<Vector3> vertices = new List<Vector3>();
        foreach (var point in points)
        {
            vertices.Add(point.Value);
        }

        mesh.vertices = vertices.ToArray();

        List<int> triangles = new List<int>();

        
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);       
            triangles.Add(i);       
            triangles.Add(i + 1);   
        }

       
        for (int i = 1; i < vertices.Count - 1; i++)
        {
            triangles.Add(0);       
            triangles.Add(i + 1);   
            triangles.Add(i);       
        }

        
        mesh.triangles = triangles.ToArray();

        
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        
        meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
        Color color = Color.white;
        color.a = SOLID_WALL_TRANSPARENCY;

        
        meshRenderer.material.color = color;

        meshFilter.mesh = mesh;

        List<KeyValuePair<string, Vector3>> copied_points = points
            .Select(point => new KeyValuePair<string, Vector3>(point.Key, point.Value))
            .ToList();


        faceInfoList.Add(new FaceInfo(copied_points,wallObject));
    }

    public void GenerateFace(List<KeyValuePair<string, Vector3>> _points)
    {
        if (_points.Count() < 3)
        {
            Debug.Log("Conajmniej 3 punkty muszą zostać wybrane, by ściana mogła powstać.");
            
        }
        else if (_points.Count() == 3 || CheckIfPointsAreOnTheSamePlane(_points))
        {
            //Debug.Log(CheckIfPointsAreOnTheSamePlane(points));

            CreateFace(_points);
        }

        points = new List<KeyValuePair<string, Vector3>>();
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

    public static Dictionary<GameObject, List<string>> GetFaces()
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
    public static void RemoveFacesFromPoint(string pointLabel)
    {
        ///////UWAGA NIE WIADOMO CZY DZIAŁA------------------------
        List<FaceInfo> faceInfos = FacesGenerator.GetFaceInfosFromPointLabel(pointLabel);
        Debug.Log("faceinfos:" + FacesGenerator.faceInfoList.Count);
        foreach (var faceInfo in faceInfos)
        {
            Destroy(faceInfo.FaceObject.gameObject);
        }
        FacesGenerator.faceInfoList.RemoveAll(faceInfo => faceInfos.Contains(faceInfo));
        Debug.Log("faceinfos:" + FacesGenerator.faceInfoList.Count);
        /////////----------------------------------------------------
    }

    public void Clear()
    {
        foreach (var faceInfo in faceInfoList)
        {
            if (faceInfo.FaceObject != null)
            {
                Destroy(faceInfo.FaceObject);
            }
        }
    }
}
