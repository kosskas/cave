using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VertexProjecter : MonoBehaviour {
	// TODO
	// Rozwiązać problem z MeshColliderem
	// Dodać ile razy była kolizja
	// Dodać linie
	
	Dictionary<string, Vector3> labeledVertices;
	GameObject[] points;
	public void InitVertexProjecter(Dictionary<string, Vector3> labeledVertices){
		this.labeledVertices = labeledVertices;
		CreateHitPoints();
	}
	
	// Update is called once per frame
	void Update () {
		GenerateRays();
	}
	void GenerateRays()
    {
		int i = 0;
        foreach (var pair in labeledVertices)
        {

			Vector3 vertex = transform.TransformPoint(pair.Value); //magic
 			// Ray w kierunku X
            Ray rayX = new Ray(vertex, Vector3.right*10);
            //Debug.DrawRay(vertex, Vector3.right*10);
			DrawRay(rayX,i);

            // Ray w kierunku -Y
            Ray rayY = new Ray(vertex, Vector3.down*10);
            //Debug.DrawRay(vertex, Vector3.down*10);
			DrawRay(rayY,i+1);

            // Ray w kierunku Z
            Ray rayZ = new Ray(vertex, Vector3.forward*10);
            //Debug.DrawRay(vertex, Vector3.forward*10);
			DrawRay(rayZ,i+2);
			i+=3;
        }
    }
    void DrawRay(Ray ray, int idx)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.collider.tag != "Solid")
        {
            // Rysuj linię reprezentującą Ray
			/*
		GameObject line = new GameObject("RayLine");
		LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
		lineRenderer.positionCount = 2;
		lineRenderer.SetPosition(0, ray.origin);
		lineRenderer.SetPosition(1, hit.point);

		line.transform.SetParent(gameObject.transform);
		*/           
            points[idx].transform.position = hit.point;
        }
    }
	private void CreateHitPoints()
    {
		points = new GameObject[3* labeledVertices.ToArray().Length];
        for (int i =0; i < points.Length; i++){
			points[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			points[i].transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			Destroy(points[i].GetComponent<Collider>());
			points[i].transform.SetParent(gameObject.transform);
		}
    }
}
