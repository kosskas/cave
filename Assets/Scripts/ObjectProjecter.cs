using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ObjectProjecter : MonoBehaviour {

	// TODO
	// [ ] Rozwiązać problem z MeshColliderem
	// [ ] Rozwiązać problem z Colliderem gracza
	// [ ] Dodać ile razy była kolizja
	// [ ] Dodać linie
	// [x] Dodac tekst
	
	Dictionary<string, Vector3> labeledVertices;
	Projection[] projs;
	Object3D OBJECT3D;

	bool showlines = true;
	public void InitVertexProjecter(Object3D obj ,Dictionary<string, Vector3> labeledVertices){
		this.OBJECT3D = obj;
		this.labeledVertices = labeledVertices;
		CreateHitPoints();
	}
	
	// Update is called once per frame
	void Update () {
		if(OBJECT3D != null){
			GenerateRays();
			//RysujRzutyNaŚcianach() TODO
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
            Debug.DrawRay(vertex, Vector3.right*10);
			ResolveProjection(rayX,i);

            // Ray w kierunku -Y
            Ray rayY = new Ray(vertex, Vector3.down*10);
            Debug.DrawRay(vertex, Vector3.down*10);
			ResolveProjection(rayY,i+1);

            // Ray w kierunku Z
            Ray rayZ = new Ray(vertex, Vector3.forward*10);
            Debug.DrawRay(vertex, Vector3.forward*10);
			ResolveProjection(rayZ,i+2);
			i+=3;
        }

    }
    void ResolveProjection(Ray ray, int idx)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) && hit.collider.tag != "Solid")
        {
			DrawProjection(projs[idx], ray, hit);			
        }
    }
	
	private void DrawProjection(Projection proj, Ray ray, RaycastHit hit){
		//rysuwanie lini wychodzącej z wierzchołka do punktu kolizji
		if(showlines){
			proj.lineRenderer.SetPosition(0, ray.origin);
			proj.lineRenderer.SetPosition(1, hit.point);
		}

		//znacznik
		proj.marker.transform.position = hit.point;

		//tekst skierowany do gracza
		proj.label.transform.position = hit.point;
		Vector3 playerPos = OBJECT3D.player.transform.position;
		Vector3 directionToPlayer = ( playerPos+ 2*Vector3.up - proj.label.transform.position).normalized;
		proj.label.transform.rotation = Quaternion.LookRotation(-directionToPlayer);
	}
	private void CreateHitPoints()
    {
		//3 bo 3 reje, MOZE BYĆ WIECEJ!!!
		int length = 3 * labeledVertices.ToArray().Length;
		string[] names = labeledVertices.Keys.ToArray();
		projs = new Projection[length];
		/*info
		points[k,k+1,k+2] dotyczą trzech różnych rzutów tego samego wierzchołka, przez co mają taką samą nazwę
		*/
        for (int i = 0; i < projs.Length; i++){
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

			
			///linia
			GameObject line = new GameObject("RayLine");
			LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
			lineRenderer.positionCount = 2;
            lineRenderer.material = new Material(Shader.Find("Standard")); // Ustawienie defaultowego materiału
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
			line.transform.SetParent(gameObject.transform);
			
			projs[i] = new Projection(marker, label, lineRenderer, false);

		}
    }
}
