using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VertexProjecter : MonoBehaviour {
	// TODO
	// [ ] Rozwiązać problem z MeshColliderem
	// [ ] Rozwiązać problem z Colliderem gracza
	// [ ] Dodać ile razy była kolizja
	// [ ] Dodać linie
	// [x] Dodac tekst
	
	Dictionary<string, Vector3> labeledVertices;
	/// <summary>
	/// Pierwszy to znacznik, drugi to tekst
	/// </summary>
	Tuple<GameObject,GameObject>[] points;
	Object3D OBJECT3D;
	//GameObject[] points;
	public void InitVertexProjecter(Object3D obj ,Dictionary<string, Vector3> labeledVertices){
		this.OBJECT3D = obj;
		this.labeledVertices = labeledVertices;
		CreateHitPoints();
	}
	
	// Update is called once per frame
	void Update () {
		if(OBJECT3D != null){
			GenerateRays();
		}
	}
	void GenerateRays()
    {
		/*info
		points[k,k+1,k+2] dotyczą trzech różnych rzutów tego samego wierzchołka, przez co mają taką samą nazwę
		*/
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
			//znacznik
            points[idx].Item1.transform.position = hit.point;

			//tekst skierowany do gracza
			points[idx].Item2.transform.position = hit.point;
			Vector3 playerPos = OBJECT3D.player.transform.position;
			Vector3 directionToPlayer = ( playerPos+ 2*Vector3.up - points[idx].Item2.transform.position).normalized;
            points[idx].Item2.transform.rotation = Quaternion.LookRotation(-directionToPlayer);
			
        }
    }
	private void CreateHitPoints()
    {
		//3 bo 3 reje
		int length = 3 * labeledVertices.ToArray().Length;
		string[] names = labeledVertices.Keys.ToArray();
		points = new Tuple<GameObject, GameObject>[length];
		/*info
		points[k,k+1,k+2] dotyczą trzech różnych rzutów tego samego wierzchołka, przez co mają taką samą nazwę
		*/
        for (int i = 0; i < points.Length; i++){
			//znacznik
			GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			marker.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
			Destroy(marker.GetComponent<Collider>());
			marker.transform.SetParent(gameObject.transform);

			//tekst
			GameObject label = new GameObject("VertexLabel" + i);
            TextMesh textMesh = label.AddComponent<TextMesh>();
            textMesh.text = names[i/3];
            textMesh.characterSize = 0.1f;
            textMesh.color = Color.black;
            textMesh.font = null;
			label.transform.SetParent(gameObject.transform);
			///
			points[i] = new Tuple<GameObject,GameObject>(marker, label);

		}
    }
}
