using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Globalization;


// Define tuple-like class
public class LabeledVertex
{
    public string Label { get; set; }
    public Vector3 Vertex { get; set; }

    public LabeledVertex(string label, Vector3 vertex)
    {
        Label = label;
        Vertex = vertex;
    }

	public Vector3 GetVertex() {
		return Vertex;
	}
}


public class SolidImporter : MonoBehaviour {

	private const string pathToFolderWithSolids = "./Assets/Figures3D";
	private const string solidFileExt = "*.wobj";
	
	private string[] solidFiles;
	private int currentSolidFileIndex;

	List<LabeledVertex> vertices = new List<LabeledVertex>();
	List<int> triangles = new List<int>();

	GameObject customSolid;
	

	// Use this for initialization
	void Start () {
		solidFiles = Directory.GetFiles(pathToFolderWithSolids, solidFileExt);
		currentSolidFileIndex = 0;

		ReadSolid();
		CreateSolid();
		LogStatus();
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown("p"))
		{
			DeleteSolid();
			ClearSolid();
			PickNextSolid();
			ReadSolid();
			CreateSolid();
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

		vertices.Add(new LabeledVertex(label, new Vector3(x, y, z)));
	}

	// ! O(n)
	private void ReadFace(string line) {
		string[] faceData = line.Trim().Split(',');

		// face is a sequence of vertices indexed from 0 to n-1
		// used algorithm to cover face with minimal number of trangles without intersected areas
		// - ith triangle - | - list of vertex indexes -
		// 			1		|	0 1 2
		//			2		|	0 2 3
		//			3		|	0 3 4
		//			...		|	...
		//			n-2		|	0 n-2 n-1
		for (int ithTriange = 1; ithTriange <= faceData.Length - 2; ithTriange++)
		{
			triangles.Add(CastLabelToIndex(faceData[0]));
			triangles.Add(CastLabelToIndex(faceData[ithTriange]));
			triangles.Add(CastLabelToIndex(faceData[ithTriange + 1]));

			// triangles.Add(CastLabelToIndex(faceData[0]));
			// triangles.Add(CastLabelToIndex(faceData[ithTriange + 1]));
			// triangles.Add(CastLabelToIndex(faceData[ithTriange]));
		}
	}
	
	// ! O(n)
	private int CastLabelToIndex(string label) {
		int ithVertex = 0;

		for (; ithVertex < vertices.Count; ithVertex++) {
			if (vertices[ithVertex].Label == label) break;
		}

		return ithVertex;
	}

	private void CreateSolid() {

		// Create a new mesh
        Mesh mesh = new Mesh();
        
		mesh.vertices = vertices.ConvertAll(v => v.Vertex).ToArray();
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
	}

	private void DeleteSolid() {
		Destroy(customSolid);
	}
}
