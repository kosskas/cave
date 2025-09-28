using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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


	private const float POINT_DIAMETER = 0.015f; 						// 0.009f
	private const float LINE_WEIGHT = 0.008f; 							// 0.005f

	private const float ON_WALL_POINT_DIAMETER = 0.5f * POINT_DIAMETER; // 0.009f
	private const float ON_WALL_LINE_WEIGHT = 0.5f * LINE_WEIGHT; 		// 0.005f

	private const float CONSTRUCTION_LINE_WEIGHT = 2.0f * 0.001f;
	private const float ADDITIONAL_CONSTRUCTION_LINE_WEIGHT = 2.0f * 0.001f; 
	private const float AXIS_WEIGHT = 0.002f; 


	private const float VERTEX_LABEL_SIZE = 0.04f;
	private const float EDGE_LABEL_SIZE = 0.01f;


	private Color LABEL_COLOR = Color.white;
	private Color POINT_COLOR = Color.black;
	private Color LINE_COLOR = Color.black;
	private Color CONSTRUCTION_LINE_COLOR = Color.blue;
	private Color ADDITIONAL_CONSTRUCTION_LINE_COLOR = Color.grey;

	private const float SOLID_WALL_TRANSPARENCY = 0.3f;

	private Dictionary<string, Point> vertexObjects = new Dictionary<string, Point>();

	private List<LineSegment> edgeObjects = new List<LineSegment>();

	private GameObject edgesFolder = null;
	private GameObject verticesFolder = null;
	

	/// <summary>
	/// Inicjuje wyświetlaną bryłę oraz resztę komponentów
	/// </summary>
	/// <param name="baseVertices">Wierzchołki opisane przez etykietę i bazowe współrzędne, czyli określone w pliku .wobj</param>
	/// <param name="edges">Krawędzie bryły określone przez etykietę i etykiety wierzchołków, któe łączy</param>
	/// <param name="faces">Kolekcja zawiera kolekcje, z których każda zawiera etykiety wierzchołów tworzących ścianę bryły 3D</param>
	/// <param name="triangles">Kolekcja indeksów wierzchołków z kolekcji faces, gdzie każda kolejna trójka definiuje trójkąt dla mesha bryły 3D</param>
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

		Vector3[] v = ConvertFacesCollectionToVertexArray();

		int ll = this.triangles.Count;
        int[] z = new int[ll * 2];

        this.triangles.ToArray().CopyTo(z, 0);

        List<int> lt = this.triangles;
		lt.Reverse();
		lt.ToArray().CopyTo(z, ll);



        mesh.vertices = v;
		mesh.triangles = z;

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
        color.a = SOLID_WALL_TRANSPARENCY;

        // Ustawienie koloru na biały
        meshRenderer.material.color = color;

        // Assign the generated mesh to the MeshFilter
        this.meshFilter.mesh = mesh;
    }

	private Vector3[] ConvertFacesCollectionToVertexArray()
	{
		var vertices = new List<Vector3>();

		faces.ForEach( face => face.ForEach( label => vertices.Add(baseVertices[label])));

        faces.ForEach(face => face.ForEach(label => vertices.Prepend(baseVertices[label])));

        return vertices.ToArray();
	}

	/// <summary>
	/// Dodaje kamerę potrzebną do obracania
	/// </summary>
	private void AddCamera()
	{	
		ObjectRotator rotator = gameObject.AddComponent<ObjectRotator>();
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
			vertexObject.SetStyle(POINT_COLOR, POINT_DIAMETER);
			vertexObject.SetCoordinates(rotatedVertices[vertexLabel]);
			vertexObject.SetLabel(vertexLabel, VERTEX_LABEL_SIZE, LABEL_COLOR);

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
			edgeObject.SetStyle(LINE_COLOR, LINE_WEIGHT);
			edgeObject.SetCoordinates(rotatedVertices[edge.endPoints.Item1], rotatedVertices[edge.endPoints.Item2]);
			edgeObject.SetLabel(edge.label, EDGE_LABEL_SIZE, LABEL_COLOR);

			edgeObjects.Add(edgeObject);
		}
	}
	/// <summary>
	/// Inicjuje mechanizm rzutowania
	/// </summary>
	private void AddRays()
    {
		ProjectionInfo projectionInfo = new ProjectionInfo(
    		POINT_COLOR, LABEL_COLOR, ON_WALL_POINT_DIAMETER, VERTEX_LABEL_SIZE,    									// Parametry punktu
    		LINE_COLOR, LABEL_COLOR, ON_WALL_LINE_WEIGHT, EDGE_LABEL_SIZE,    											// Parametry krawędzi
    		CONSTRUCTION_LINE_COLOR, LABEL_COLOR, CONSTRUCTION_LINE_WEIGHT, EDGE_LABEL_SIZE,     						// Parametry linii rzutującej
			ADDITIONAL_CONSTRUCTION_LINE_COLOR, LABEL_COLOR, ADDITIONAL_CONSTRUCTION_LINE_WEIGHT, EDGE_LABEL_SIZE,		// Parametry linii odnoszących
    		false                                     																	// Określenie czy linie rzutowania powinny być wyświetlane
		);

        ObjectProjecter op = gameObject.AddComponent<ObjectProjecter>();
		op.InitProjecter(this, projectionInfo, rotatedVertices, edges);
    }
}
