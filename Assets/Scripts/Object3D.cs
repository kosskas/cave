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
	/// Wierzchołki opisane przez etykietę i bazowe współrzędne, czyli określone w pliku .wobj
	/// </summary>
	public Dictionary<string, Vector3> baseVertices;

	/// <summary>
	/// Wierzchołki opisane przez etykietę i współrzędne, uwzględnijące aktualne przemieszczenie i obrót bryły 3D
	/// </summary>
	public Dictionary<string, Vector3> rotatedVertices = new Dictionary<string, Vector3>();

	/// <summary>
	/// Określa współrzędne środka ciężkości bryły, które służą jako punkt odniesienia np. do symulacji rotacji bryły 3D
	/// </summary>
	public Vector3 midPoint = new Vector3(0.0f, 1.0f, 0.0f);

	/// <summary>
	/// Krawędzie bryły określone przez etykietę i etykiety wierzchołków, któe łączy
	/// </summary>
	public List<EdgeInfo> edges;

	/// <summary>
	/// Kolekcja zawiera kolekcje, z których każda zawiera etykiety wierzchołów tworzących ścianę bryły 3D
	/// </summary>
	public List<List<string>> faces;

	/// <summary>
	/// Kolekcja indeksów wierzchołków z kolekcji <param name="faces">faces"</param>, gdzie każda kolejna trójka definiuje trójkąt dla mesha bryły 3D
	/// </summary>
	private List<int> triangles;

	/// <summary>
	/// Siatka obiektu
	/// </summary>
	private MeshFilter meshFilter;

	/// <summary>
	/// Referencja na obiekt gracza
	/// </summary>
	public GameObject player = null;


	public Dictionary<string, Point> vertexObjects = new Dictionary<string, Point>();

	public List<LineSegment> edgeObjects = new List<LineSegment>();

	public GameObject edgesFolder = null;
	public GameObject verticesFolder = null;
	

	
	public void InitObject( Dictionary<string, Vector3> baseVertices, List<EdgeInfo> edges, List<List<string>> faces, List<int> triangles )
	{
		this.player = GameObject.Find("FPSPlayer");

		if (this.player == null)
		{
			Debug.Log("Object3D nie znalazl instancji FPSPlayer");
			return;
		}

		this.baseVertices = baseVertices;
		this.edges = edges;
		this.faces = faces;
		this.triangles = triangles;

		gameObject.transform.position = midPoint;

		this.tag = "Solid";

		CreateMesh();

		AddCamera();

		InitRotatedVertices();

		InitVertexObjects();

		InitEdgeObjects();

		AddRays();

		// foreach(var edge in edges){
		// 	Debug.Log(edge.endPoints.Item1+" " +edge.endPoints.Item2);
		// }
	}

	void Update()
	{
		// Aktualizacja współrzędnych wierzchołków pod wpływem obrotu (rotacji) bryły
		UpdateRotatedVertices();
		
		///edgeObjects jest NULL???? albo rotatedVertices?
		// if(edgeObjects == null){
		// 	Debug.LogWarning("edgeObjects");
		// }
		// if(rotatedVertices == null){
		// 	Debug.LogWarning("rotatedVertices");
		// }

		// Aktualizacja krawędzi
		for (int i = 0; i < edgeObjects.Count; i++)
		{
			edgeObjects[i].SetCoordinates(rotatedVertices[edges[i].endPoints.Item1], rotatedVertices[edges[i].endPoints.Item2]);
		}

	}



	/// <summary>
    /// Tworzy siatkę dla nowo powstałego obiektu
    /// </summary>
    private void CreateMesh()
	{
        // Create a new mesh
        Mesh mesh = new Mesh();
        
		mesh.vertices = ConvertFacesCollectionToVertexArray(); 
		mesh.triangles = this.triangles.ToArray();

        // Recalculate normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        // Add a MeshFilter component to the GameObject
        this.meshFilter = gameObject.AddComponent<MeshFilter>();

        // Add a MeshRenderer component to the GameObject
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();

        // Set the material for the MeshRenderer (you can create your own material or use an existing one)
        meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		Color color = Color.white;
        color.a = 0.5f;

        // Ustawienie koloru na biały
        meshRenderer.material.color = color;

        // Assign the generated mesh to the MeshFilter
        this.meshFilter.mesh = mesh;

		//Dodanie collidera potrzebnego do obracania
		MeshCollider meshColl = gameObject.AddComponent<MeshCollider>();
    }

	private Vector3[] ConvertFacesCollectionToVertexArray()
	{
		var vertices = new List<Vector3>();

		faces.ForEach( face => face.ForEach( label => vertices.Add(baseVertices[label])));

		return vertices.ToArray();
	}

	/// <summary>
	/// Dodaje kamerę potrzebną do obracania
	/// </summary>
	private void AddCamera()
	{	
		GameObject camObject = GameObject.Find("CameraObject");
		CameraScript camScript = camObject.GetComponent<CameraScript>();
		GameObject staticCam = camScript.cam2;
		ObjectRotator rotator = gameObject.AddComponent<ObjectRotator>();
		rotator.cam = staticCam.GetComponent<Camera>();
	}

	private void InitRotatedVertices()
	{
		foreach (var vertexLabel in baseVertices.Keys.ToList())
		{
			//rotatedVertices[vertexLabel] = midPoint + baseVertices[vertexLabel];
			rotatedVertices[vertexLabel] = transform.TransformPoint(baseVertices[vertexLabel]);
		}
	}

	private void UpdateRotatedVertices()
	{
		foreach (var vertexLabel in rotatedVertices.Keys.ToList())
		{
			//rotatedVertices[vertexLabel] = midPoint + (transform.rotation * baseVertices[vertexLabel]);
			rotatedVertices[vertexLabel] = transform.TransformPoint(baseVertices[vertexLabel]);
		}
	}

	private void InitVertexObjects()
	{
		verticesFolder = new GameObject("Verticies");
		verticesFolder.transform.SetParent(gameObject.transform);

		foreach (var vertexLabel in rotatedVertices.Keys)
		{
			GameObject obj = new GameObject("Vertex " + vertexLabel);
			obj.transform.SetParent(verticesFolder.transform);

			Point vertexObject = obj.AddComponent<Point>();
			vertexObject.SetStyle(Color.black, 0.04f);
			vertexObject.SetCoordinates(rotatedVertices[vertexLabel]);
			vertexObject.SetLabel(vertexLabel, 0.04f, Color.white);

			vertexObjects[vertexLabel] = vertexObject;
		}
	}

	private void InitEdgeObjects()
	{
		edgesFolder = new GameObject("Edges");
		edgesFolder.transform.SetParent(gameObject.transform);

		foreach (var edge in edges)
		{
			GameObject obj = new GameObject("Edge " + edge.label);
			obj.transform.SetParent(edgesFolder.transform);

			LineSegment edgeObject = obj.AddComponent<LineSegment>();
			edgeObject.SetStyle(Color.black, 0.01f);
			edgeObject.SetCoordinates(rotatedVertices[edge.endPoints.Item1], rotatedVertices[edge.endPoints.Item2]);
			edgeObject.SetLabel(edge.label, 0.01f, Color.white);

			edgeObjects.Add(edgeObject);
		}
	}
	/// <summary>
	/// Inicjuje mechanizm rzutowania
	/// </summary>
	private void AddRays()
    {
		ProjectionInfo projectionInfo = new ProjectionInfo(
    		Color.black, Color.white, 0.02f, 0.04f,    // Parametry punktu
    		Color.black, Color.white, 0.01f, 0.01f,    // Parametry krawędzi
    		Color.gray, Color.white, 0.01f, 0.01f,     // Parametry linii rzutującej
    		false                                     // Określenie czy linie rzutowania powinny być wyświetlane
		);

        ObjectProjecter op = gameObject.AddComponent<ObjectProjecter>();
		op.InitProjecter(this, projectionInfo, rotatedVertices, edges);
    }
}



/*** * * * OLD VERSION OF FILE * * *

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
			AddSolidEdges();
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
        meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		Color color = Color.white;
        color.a = 0.5f;

        // Ustawienie koloru na biały
        meshRenderer.material.color = color;

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
		ProjectionInfo projectionInfo = new ProjectionInfo(); //bazowo parametry czytane z pliku

        ObjectProjecter op = gameObject.AddComponent<ObjectProjecter>();
		op.InitProjecter(this, projectionInfo, labeledVertices, edges);
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


	private void AddSolidEdges()
	{
		SolidEdges solidEdges = gameObject.AddComponent<SolidEdges>();
		solidEdges.InitEdges(this, edges, labeledVertices);	
	}


	/// <summary>
	/// Sprawdza czy wierzchołki sąsiadują ze sobą
	/// </summary>
	/// <param name="vert1Name">Nazwa pierwszego wierzchołka</param>
	/// <param name="vert2Name">Nazwa drugiego wierzchołka</param>
	/// <returns>True jeśli sąsiadują, False jeśli nie</returns>
	public bool AreNeighbours(string vert1Name, string vert2Name){
		foreach (var edge in edges){
			if(edge.Item1 == vert1Name && edge.Item2 == vert2Name){
				return true;
			}
		}
		return false;
	}

}

***/