using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTest : MonoBehaviour {

	Vector3 cubeCenter = new Vector3(2, 2, 2);

	Dictionary<string, Vector3> verticies = new Dictionary<string, Vector3>()
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

	Dictionary<string, Tuple<string, string>> edges = new Dictionary<string, Tuple<string, string>>()
	{
		{ "a", new Tuple<string, string>("A", "C") },
		{ "b", new Tuple<string, string>("C", "D") },
		{ "c", new Tuple<string, string>("D", "B") },
		{ "d", new Tuple<string, string>("B", "A") },

		{ "e", new Tuple<string, string>("A", "E") },
		{ "f", new Tuple<string, string>("C", "G") },
		{ "g", new Tuple<string, string>("D", "H") },
		{ "h", new Tuple<string, string>("B", "F") },

		{ "i", new Tuple<string, string>("E", "G") },
		{ "j", new Tuple<string, string>("G", "H") },
		{ "k", new Tuple<string, string>("H", "F") },
		{ "l", new Tuple<string, string>("F", "E") }
	};


	List<GameObject> realEdges = new List<GameObject>();
	List<GameObject> realVerticies = new List<GameObject>();


	GameObject edgeObjects = null;
	GameObject vertexObjects = null;


	// Use this for initialization
	void Start ()
	{
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
		Color color = Color.white;
        color.a = 0.5f;
        meshRenderer.material.color = color;

		gameObject.transform.position = cubeCenter;

		edgeObjects = new GameObject("Edges");
		edgeObjects.transform.SetParent(gameObject.transform);
		foreach (var edge in edges)
		{
			GameObject realEdge = new GameObject("Edge " + edge.Key);
			realEdge.transform.SetParent(edgeObjects.transform);

			LineSegment lineSegment = realEdge.AddComponent<LineSegment>();
			lineSegment.SetBaseCoordinates(cubeCenter, verticies[edge.Value.Item1], verticies[edge.Value.Item2]);
			lineSegment.SetStyle(Color.black, 0.05f);
			lineSegment.SetLabel(edge.Key, 0.05f, Color.white);
			
			realEdges.Add(realEdge);
		}

		vertexObjects = new GameObject("Verticies");
		vertexObjects.transform.SetParent(gameObject.transform);
		foreach (var vertex in verticies)
		{
			GameObject realVertex = new GameObject("Vertex " + vertex.Key);
			realVertex.transform.SetParent(vertexObjects.transform);

			Point point = realVertex.AddComponent<Point>();
			point.SetBaseCoordinates(cubeCenter, vertex.Value);
			point.SetStyle(Color.black, 0.1f);
			point.SetLabel(vertex.Key, 0.1f, Color.white);

			realVerticies.Add(realVertex);
		}
	}
	
	// Update is called once per frame
	void Update () {}
}