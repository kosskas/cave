using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Linq;


public struct EdgeInfo {
	public string label;
	public Tuple<string, string> endPoints;

	public EdgeInfo(string label, Tuple<string, string> endPoints)
	{
		this.label = label;
		this.endPoints = endPoints;
	}
}


public class DefaultSolid {
	public static Dictionary<string, Vector3> Verticies = new Dictionary<string, Vector3>()
	{
		{ "A", new Vector3(-1, -1, -1) },
		{ "B", new Vector3(-1, -1, 1) },
		{ "C", new Vector3(-1, 1, -1) },
		{ "D", new Vector3(-1, 1, 1) },

		{ "E", new Vector3(1, -1, -1) },
		{ "F", new Vector3(1, -1, 1) },
		{ "G", new Vector3(1, 1, -1) },
		{ "H", new Vector3(1, 1, 1) }
	};

	public static List<EdgeInfo> Edges = new List<EdgeInfo>()
	{
		new EdgeInfo("a", new Tuple<string, string>("A", "C")),
		new EdgeInfo("b", new Tuple<string, string>("C", "D")),
		new EdgeInfo("c", new Tuple<string, string>("D", "B")),
		new EdgeInfo("d", new Tuple<string, string>("B", "A")),

		new EdgeInfo("e", new Tuple<string, string>("A", "E")),
		new EdgeInfo("f", new Tuple<string, string>("C", "G")),
		new EdgeInfo("g", new Tuple<string, string>("D", "H")),
		new EdgeInfo("h", new Tuple<string, string>("B", "F")),

		new EdgeInfo("i", new Tuple<string, string>("E", "G")),
		new EdgeInfo("j", new Tuple<string, string>("G", "H")),
		new EdgeInfo("k", new Tuple<string, string>("H", "F")),
		new EdgeInfo("l", new Tuple<string, string>("F", "E"))
	};

	public static List<List<string>> Faces = new List<List<string>>()
		{
			new List<string>(){ "A", "C", "G", "E" },

			new List<string>(){ "C", "D", "H", "G" },

			new List<string>(){ "E", "G", "H", "F" },

			new List<string>(){ "E", "F", "B", "A" },

			new List<string>(){ "F", "H", "D", "B" },

			new List<string>(){ "B", "D", "C", "A" }
		};

	public static List<int> Triangles = new List<int>()
	{
		0, 1, 2,
		0, 2, 3,

		4, 5, 6,
		4, 6, 7,

		8, 9, 10,
		8, 10, 11,

		12, 13, 14,
		12, 14, 15,

		16, 17, 18,
		16, 18, 19,

		20, 21, 22,
		20, 22, 23
	};
}


/// <summary>
/// Klasa SolidImporter służy do parsowania i ładowania brył zapisanych w formacie wobj to aplikacji.
/// </summary>
///
public class SolidImporter : MonoBehaviour {

	private const string pathToFolderWithSolids = "./Figures3D";

	private bool isSolidFolderValid = false;
	private const string solidFileExt = "*.wobj";
	
	private const float SIZER = 0.3f; 
	private string[] solidFiles = null;
	private int currentSolidFileIndex;

	private Dictionary<string, Vector3> labeledVertices = new Dictionary<string, Vector3>();
	private List<List<string>> faces = new List<List<string>>();

	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triangles = new List<int>();
	private List<EdgeInfo> edges = new List<EdgeInfo>();

	private GameObject customSolid;
	private GameObject mainObj;

	// Use this for initialization
	void Start () {
		mainObj = GameObject.Find("MainObject");

		try {
			solidFiles = Directory.GetFiles(pathToFolderWithSolids, solidFileExt);
			Debug.Log(solidFiles);
		}
		catch (System.Exception) {
			solidFiles = null;
			Debug.LogError("[CAVE] It seems that folder " + Application.dataPath + pathToFolderWithSolids + " does not exist.");
		}
		
		currentSolidFileIndex = 0;

		isSolidFolderValid = (solidFiles == null || solidFiles.Length == 0) ? false : true;
	}

	/// <summary>
	/// Szuka następną bryłę do załadowania w folderze i ładuje ją. W trakcie działania parsuje plik w formacie wobj, środkuje bryłę względem 0,0,0. Ładuje Object3D -centralną, klasę, która podłącza resztę komponentów
	/// </summary>
	public void ImportSolid () {
		DeleteMainObjChild();
		DeleteSolid();
		ClearSolid();

		if (isSolidFolderValid) {
			PickNextSolid();
			ReadSolid();
			CentralizePosition();
			SetUpVertices();
			SetUpTriangles();
		}

		CreateSolidObject();

		LogStatus();
	}

    private void LogStatus() {
		if (solidFiles != null && solidFiles.Length > 0)
		{
			StringBuilder infoString = new StringBuilder();
			infoString.Append(String.Format(" Current solid: {0}", Path.GetFileName(GetCurrentSolid())));
			infoString.Append(String.Format("\n | All solids:"));
			foreach (string solidFile in solidFiles)
			{
				infoString.Append(String.Format(" {0}", Path.GetFileName(solidFile)));
			}
			Debug.Log(infoString.ToString());
		}
		else
		{
			Debug.LogWarning("[CAVE] No solids found! Default solid has loaded.");
		}
	}

	private void PickNextSolid() {
		currentSolidFileIndex = (currentSolidFileIndex + 1) % solidFiles.Length;
	}

	private string GetCurrentSolid() {
		return solidFiles[currentSolidFileIndex];
	}

	private void ClearSolid() {
		labeledVertices.Clear();
		faces.Clear();
		edges.Clear();
		vertices.Clear();
		triangles.Clear();
	}

	private void ReadSolid() {
		if (File.Exists(GetCurrentSolid()))
        {
            using (StreamReader reader = new StreamReader(GetCurrentSolid()))
            {
				int part = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
					if (line.Contains("###")) part++;
					else if (String.IsNullOrEmpty(line)) continue;
					else if (part == 1) { ReadVertex(line); }
					else if (part == 2) { ReadFace(line); ReadEdges(line); }
					else if (part > 2) break;
                }
            }
        }
        else
        {
			Debug.LogError(Path.GetFileName(GetCurrentSolid()) + " not found!");
        }
	}

	private void ReadVertex(string line) {
		string[] vertexData = line.Trim().Split(' ');

		string label = vertexData[0];
		float x = float.Parse(vertexData[1], CultureInfo.InvariantCulture);
		float y = float.Parse(vertexData[2], CultureInfo.InvariantCulture);
		float z = float.Parse(vertexData[3], CultureInfo.InvariantCulture);

		labeledVertices[label] = new Vector3(x, y, z) * SIZER;
	}

	private void ReadFace(string line) {
		string[] faceData = line.Trim().Split(',');

		List<string> face = new List<string>();
		Array.ForEach(faceData, label => face.Add(label));

		faces.Add(face);
	}

	private void ReadEdges(string line) {
		string[] vLabelsOfFace = line.Trim().Split(',');

		for (int i = 0; i < vLabelsOfFace.Length; i++)
		{
			int v1 = i % vLabelsOfFace.Length;
			int v2 = (i+1) % vLabelsOfFace.Length;

			if (edges.Exists(edge => 
					(edge.endPoints.Item1 == vLabelsOfFace[v1] && edge.endPoints.Item2 == vLabelsOfFace[v2]) ||
					(edge.endPoints.Item2 == vLabelsOfFace[v1] && edge.endPoints.Item1 == vLabelsOfFace[v2]) 
			))
			{
				continue;
			}

			edges.Add(
				new EdgeInfo("", new Tuple<string, string>(vLabelsOfFace[v1], vLabelsOfFace[v2]))
			);
		}
	}

	// O(2n)
	private void CentralizePosition() {
		Vector3 centerPoint = new Vector3(0, 0, 0);
		int n = labeledVertices.Count;

		foreach(Vector3 vertex in labeledVertices.Values)
		{
			centerPoint += vertex;
		}

		centerPoint /= n;

		labeledVertices = labeledVertices.ToDictionary(entry => entry.Key, entry => entry.Value - centerPoint);
	}

	/*  SetUpVertices()
	* each face is given its own exclusive set of vertices
	*/// O(n)
	private void SetUpVertices() {
		faces.ForEach( face => face.ForEach( label => vertices.Add(labeledVertices[label])));
	}

	/*  SetUpTriangles() 
	* face is a sequence of n vertices indexed from 0 to n-1
	* used algorithm to cover face with minimal number of trangles without intersected areas:
	* - i-th triangle - | - indexes -
	* 			1		|	0 1 2
	*			2		|	0 2 3
	*			3		|	0 3 4
	*			...		|	...
	*			n-2		|	0 n-2 n-1
	*/// O(n)
	private void SetUpTriangles() {
		int startVertexIndex = 0;

		faces.ForEach( face => {
			const int firstVertexOffset = 0;
			int secondVertexOffset = 1;
			int thirdVertexOffset = 2;

			for (int ithTriangle = 1; ithTriangle <= face.Count - 2; ithTriangle++)
			{
				triangles.AddRange(new List<int>{
					startVertexIndex + firstVertexOffset,
					startVertexIndex + secondVertexOffset,
					startVertexIndex + thirdVertexOffset
				});

				secondVertexOffset++;
				thirdVertexOffset++;
			}

			startVertexIndex += face.Count;
		});
	}

	private void CreateSolidObject(){
		// Create a new GameObject to hold the custom solid
        customSolid = new GameObject("CustomSolid");
		//Set parent as MainObject
		customSolid.transform.SetParent(mainObj.transform);
		//zeruje bo było (0,2,0) nie wiem czemu
		transform.position = Vector3.zero;
        customSolid.transform.position = Vector3.zero;

		//Dodanie Object3D - centralnej klasy która podepnie resztę komponentów
		Object3D object3D = customSolid.AddComponent<Object3D>();
		
		if (isSolidFolderValid) {
			object3D.InitObject(labeledVertices, edges, faces, triangles);
		}
		else {
			object3D.InitObject(DefaultSolid.Verticies, DefaultSolid.Edges, DefaultSolid.Faces, DefaultSolid.Triangles);
		}
	}
	private void DeleteSolid() {
		Destroy(customSolid);
	}

	private void DeleteMainObjChild() {
		if (mainObj.transform.childCount > 0) {
			Destroy(mainObj.transform.GetChild(0).gameObject);
		}
	}
}
