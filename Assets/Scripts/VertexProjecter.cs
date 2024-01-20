using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexProjecter : MonoBehaviour {

	Dictionary<string, Vector3> labeledVertices;
	public void InitVertexProjecter(Dictionary<string, Vector3> labeledVertices){
		this.labeledVertices = labeledVertices;
		//GenerateRays();
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		GenerateRays();
	}
	void GenerateRays()
    {
        foreach (var pair in labeledVertices)
        {

			Vector3 vertex = transform.TransformPoint(pair.Value); //magic
 			// Ray w kierunku X
            Ray rayX = new Ray(vertex, Vector3.right*10);
            Debug.DrawRay(vertex, Vector3.right*10);
			//DrawRay(rayX);

            // Ray w kierunku Y
            Ray rayY = new Ray(vertex, Vector3.down*10);
            Debug.DrawRay(vertex, Vector3.down*10);
			//DrawRay(rayY);

            // Ray w kierunku Z
            Ray rayZ = new Ray(vertex, Vector3.forward*10);
            Debug.DrawRay(vertex, Vector3.forward*10);
			//DrawRay(rayZ);
        }
    }
    void DrawRay(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
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
            // Dodaj punkt w miejscu przecięcia
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            point.transform.position = hit.point;
            point.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            Destroy(point.GetComponent<Collider>()); // Usuń collider, aby uniknąć kolizji z kolejnymi rayami
			point.transform.SetParent(gameObject.transform);
        }
    }
}
