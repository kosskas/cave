using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngineInternal;

/// <summary>
/// Klasa Object3D zarządza wyświetlanym obiektem. Po załadowaniu bryły z pliku, ładuje pozostałe klasy potrzebne do działania aplikacji.
/// </summary>
public class Object3D : MonoBehaviour {

	/// <summary>
	/// Referencja na obiekt gracza
	/// </summary>
	public GameObject player;
	/// <summary>
	/// Lista wierchołków
	/// </summary>
	private List<Vector3> vertices = new List<Vector3>();
	/// <summary>
	/// Lista trójkątów bloku
	/// </summary>
	private List<int> triangles = new List<int>();
	/// <summary>
	/// Współrzędne wierch. i odpowiadające im nazwy
	/// </summary>
	private Dictionary<string, Vector3> labeledVertices = new Dictionary<string, Vector3>();
	/// <summary>
	/// Lista ścian
	/// </summary>
	private List<List<string>> faces = new List<List<string>>();
	/// <summary>
	/// Siatka obiektu
	/// </summary>
	private MeshFilter meshFilter;


	/// <summary>
	/// Lista krawędzi
	/// </summary>
	List<Tuple<string, string>> edges = new List<Tuple<string, string>>();

	/// <summary>
	/// Inicjalizuje obiekt, dodaje skrypty.
	/// </summary>
	/// <param name="vertices">Lista wierzchołków</param>
	/// <param name="triangles">Lista trójkątów</param>
	/// <param name="labeledVertices">Oznaczenia wierzchołków</param>
	public void InitObject(List<Vector3> vertices, List<int> triangles, List<List<string>> faces, Dictionary<string, Vector3> labeledVertices){
		player = GameObject.Find("FPSPlayer");
		if(player!= null){
			this.vertices = vertices;
			this.triangles = triangles;
			this.labeledVertices = labeledVertices;
			this.faces = faces;
			this.tag = "Solid";
			CreateMesh();
			AddVertexLabels();
			AddCamera();
			//TODO
			//Dodanie oznaczeń i wyświetlanie krawędzi
					
			//TODO
			//Dodanie projekcji rzutów
			GetEgdesList();
			AddRays();
		}
		else{
			Debug.Log("Object3D nie znalazl instancji FPSPlayer");
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
		vl.InitLabels(this,meshFilter, labeledVertices);	
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
	/// <summary>
	/// Inicjuje mechanizm rzutowania
	/// </summary>
	private void AddRays()
    {
        ObjectProjecter op = gameObject.AddComponent<ObjectProjecter>();
		op.InitProjecter(this, labeledVertices, edges);
    }
	/// <summary>
	/// Tworzy listę krawędzi
	/// </summary>
	private void GetEgdesList(){
		foreach (var face in faces)
        {
            for (int i = 0; i < face.Count; i++)
            {
                string vertex1 = face[i];
                string vertex2 = face[(i + 1) % face.Count]; // Pobierz następny wierzchołek w cyklu
                edges.Add(new Tuple<string, string>(vertex1, vertex2));
            }
        }

        // Wyświetlenie listy krawędzi
        foreach (var edge in edges)
        {
            Debug.Log(edge.Item1 + " - " + edge.Item2);
        }
	}


}