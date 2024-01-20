using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

/// <summary>
/// Klasa Object3D zarządza wyświetlanym obiektem. Po załadowaniu bryły z pliku, ładuje pozostałe klasy potrzebne do działania aplikacji.
/// </summary>
public class Object3D : MonoBehaviour {

	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triangles = new List<int>();
	private Dictionary<string, Vector3> labeledVertices = new Dictionary<string, Vector3>();
	//private List<List<string>> faces = new List<List<string>>();
	private MeshFilter meshFilter;


	void Update(){
				// Iteruj przez każdy wierzchołek
		/*		
		foreach (Vector3 vertex in vertices)
        {
            // Ray w kierunku X
            Ray rayX = new Ray(vertex, Vector3.right*10);
            Debug.DrawRay(vertex, Vector3.right*10);

            // Ray w kierunku Y
            Ray rayY = new Ray(vertex, Vector3.up*10);
            Debug.DrawRay(vertex, Vector3.up*10);

            // Ray w kierunku Z
            Ray rayZ = new Ray(vertex, Vector3.forward*10);
            Debug.DrawRay(vertex, Vector3.forward*10);
        }
		*/
	}
	/// <summary>
	/// Inicjalizuje obiekt, dodaje skrypty.
	/// </summary>
	/// <param name="vertices">Lista wierzchołków</param>
	/// <param name="triangles">Lista trójkątów</param>
	/// <param name="labeledVertices">Oznaczenia wierzchołków</param>
	public void InitObject(List<Vector3> vertices, List<int> triangles, Dictionary<string, Vector3> labeledVertices){
		this.vertices = vertices;
		this.triangles = triangles;
		this.labeledVertices = labeledVertices;
		CreateMesh();
		AddVertexLabels();
		AddCamera();
		//TODO
		//Dodanie oznaczeń i wyświetlanie krawędzi

		GenerateRays();
		//TODO
		//Dodanie projekcji rzutów
	}
	void GenerateRays()
    {
        foreach (Vector3 vertex in vertices)
        {
            // Ray w kierunku X
            Ray rayX = new Ray(vertex, Vector3.right * 10);
            DrawRay(rayX);

            // Ray w kierunku Y
            Ray rayY = new Ray(vertex, Vector3.up * 10);
            DrawRay(rayY);

            // Ray w kierunku Z
            Ray rayZ = new Ray(vertex, Vector3.forward * 10);
            DrawRay(rayZ);
        }
    }
    void DrawRay(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            // Rysuj linię reprezentującą Ray
            GameObject line = new GameObject("RayLine");
            LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, ray.origin);
            lineRenderer.SetPosition(1, hit.point);

            // Dodaj punkt w miejscu przecięcia
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = hit.point;
            point.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            Destroy(point.GetComponent<Collider>()); // Usuń collider, aby uniknąć kolizji z kolejnymi rayami
        }
    }
	/// <summary>
	/// Tworzy siatkę dla nowo powstałego obiektu
	/// </summary>
    private void CreateMesh() {
        // Create a new mesh
        Mesh mesh = new Mesh();
        
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();

        // Recalculate normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Add a MeshFilter component to the GameObject
        meshFilter = gameObject.AddComponent<MeshFilter>();

        // Add a MeshRenderer component to the GameObject
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Set the material for the MeshRenderer (you can create your own material or use an existing one)
        meshRenderer.material = new Material(Shader.Find("Standard"));

        // Assign the generated mesh to the MeshFilter
        meshFilter.mesh = mesh;

		//Dodanie collidera potrzebnego do obracania
		MeshCollider meshColl = gameObject.AddComponent<MeshCollider>();
    }
	/// <summary>
	/// Dodaje oznaczenia przy wyświetlaniu wierzchołków
	/// </summary>
    private void AddVertexLabels() {
        
		VertexLabels vl = gameObject.AddComponent<VertexLabels>();
		vl.InitLabels(meshFilter, labeledVertices);	
    }
	/// <summary>
	/// Dodaje kamerę potrzebną do obracania
	/// </summary>
	private void AddCamera(){
		
		GameObject camObject = GameObject.Find("CameraObject");
		CameraScript camScript = camObject.GetComponent<CameraScript>();
		GameObject staticCam = camScript.cam2;
		ObjectRotator rotator = gameObject.AddComponent<ObjectRotator>();
		rotator.cam = staticCam.GetComponent<Camera>();
	}

}
