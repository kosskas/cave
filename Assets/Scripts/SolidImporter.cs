using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Globalization;


public class SolidImporter : MonoBehaviour {

	private const string pathToFolderWithSolids = "./Assets/Figures3D";
	private const string solidFileExt = "*.wobj";
	
	private string[] solidFiles;
	private int currentSolidFileIndex;

	private Dictionary<string, Vector3> labeledVertices = new Dictionary<string, Vector3>();
	private List<List<string>> faces = new List<List<string>>();

	private List<Vector3> vertices = new List<Vector3>();
	private List<int> triangles = new List<int>();

	private GameObject customSolid;
	private GameObject mainObj;
	

	// Use this for initialization
	void Start () {
		mainObj = GameObject.Find("MainObject");
		solidFiles = Directory.GetFiles(pathToFolderWithSolids, solidFileExt);
		currentSolidFileIndex = 0;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown("p"))
		{
			DeleteMainObjChild();
			DeleteSolid();
			ClearSolid();
			PickNextSolid();
			ReadSolid();
			SetUpVertices();
			SetUpTriangles();
			CreateSolid();
			SetAsMainObjChild();
			LogStatus();

			// Debug.Log("vertices");
			// foreach (var vertex in vertices)
			// {
			// 	Debug.Log($"Label: {vertex.Label}, Vertex: {vertex.Vertex}");
			// }
			// Debug.Log("faces");
			// for (int i = 0; i < triangles.Count; i += 3)
			// {
			// 	Debug.Log($"{triangles[i]} {triangles[i + 1]} {triangles[i + 2]}");
			// }
		}

	}

	private void LogStatus() {
		if (solidFiles.Length > 0)
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
			Debug.LogError("No solids found!");
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
					else if (part == 1) ReadVertex(line);
					else if (part == 2) ReadFace(line);
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

		labeledVertices[label] = new Vector3(x, y, z);
	}

	private void ReadFace(string line) {
		string[] faceData = line.Trim().Split(',');

		List<string> face = new List<string>();
		Array.ForEach(faceData, label => face.Add(label));

		faces.Add(face);
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

	private void CreateSolid() {

		// Create labeling object
		//VertexLabels vl = FindObjectOfType<VertexLabels>();

		// Create a new mesh
        Mesh mesh = new Mesh();
        
		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();

        // Recalculate normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

		// Create a new GameObject to hold the custom solid
        customSolid = new GameObject("CustomSolid");

        // Add a MeshFilter component to the GameObject
        MeshFilter meshFilter = customSolid.AddComponent<MeshFilter>();

        // Add a MeshRenderer component to the GameObject
        MeshRenderer meshRenderer = customSolid.AddComponent<MeshRenderer>();

        // Set the material for the MeshRenderer (you can create your own material or use an existing one)
        meshRenderer.material = new Material(Shader.Find("Standard"));

        // Assign the generated mesh to the MeshFilter
        meshFilter.mesh = mesh;

		// Set labels
		//string[] x = vertices.ConvertAll(v => v.Label).ToArray();
		//Debug.Log(x.Length);
		//vl.InitLabels(vertices.ConvertAll(v => v.Label).ToArray());
	}

	private void DeleteSolid() {
		Destroy(customSolid);
	}

	private void DeleteMainObjChild() {
		if (mainObj.transform.childCount > 0) {
			Destroy(mainObj.transform.GetChild(0).gameObject);
		}
	}

	private void SetAsMainObjChild() {
		customSolid.transform.SetParent(mainObj.transform);
	}
}
