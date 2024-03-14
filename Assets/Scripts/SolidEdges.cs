using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SolidEdges : MonoBehaviour {

	/// <summary>
    /// Referencja na Obiekt3D
    /// </summary>
    Object3D OBJECT3D;

	/// <summary>
	/// Oznaczenia wierzchołków
	/// </summary>
	Dictionary<string, Vector3> labeledVertices;
	List<Tuple<string, string>> edges;

	/// <summary>
	/// Krawędzie objektu 3D
	/// nazewnictwo: SolidEdge_<etykieta_wierzchołka_początkowego>_<etykieta_wierzchołka_końcowego>
	/// </summary>
	List<GameObject> solidEdges = null;


	public void InitEdges(
		Object3D obj,
		List<Tuple<string, string>> edges,
		Dictionary<string, Vector3> labeledVertices
		)
    {
        this.OBJECT3D = obj;
		this.labeledVertices = labeledVertices;
		this.edges = edges;
		this.solidEdges = new List<GameObject>();
    
		foreach (var edge in edges)
		{
			GameObject solidEdge = new GameObject("SolidEdge_" + edge.Item1 + "__" + edge.Item2);
			LineRenderer lineRenderer = solidEdge.AddComponent<LineRenderer>();

			lineRenderer.positionCount = 2;
            lineRenderer.material = new Material(Shader.Find("Standard"));
            lineRenderer.startWidth = 0.02f;
            lineRenderer.endWidth = 0.02f;
			lineRenderer.numCapVertices = 10;
			lineRenderer.shadowCastingMode = ShadowCastingMode.Off;

			lineRenderer.SetPosition(0, transform.TransformPoint(labeledVertices[edge.Item1]));
			lineRenderer.SetPosition(1, transform.TransformPoint(labeledVertices[edge.Item2]));

			//Debug.Log("SolidEdge_" + edge.Item1 + "_" + edge.Item2);
			solidEdges.Add(solidEdge);
			solidEdge.transform.SetParent(gameObject.transform);
		}

    }
	

	void Update ()
	{
		if (OBJECT3D != null && solidEdges != null)
        {
            //Quaternion rotation = transform.rotation;
            //NOTE: Rezygnacja z używania Rotation bo labele są dziećmi CustomSolid
			int i = 0;
			foreach (var solidEdge in solidEdges)
			{
				LineRenderer lineRenderer = solidEdge.GetComponent<LineRenderer>();

				lineRenderer.SetPosition(0, transform.TransformPoint(labeledVertices[edges[i].Item1]));
				lineRenderer.SetPosition(1, transform.TransformPoint(labeledVertices[edges[i].Item2]));

				i++;
			}
        }
	}

	
}
