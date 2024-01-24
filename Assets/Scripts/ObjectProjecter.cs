using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

	// TODO
	// [x] Rozwiązać problem z MeshColliderem
	// [ ] Rozwiązać problem z Colliderem gracza
	// [ ] Dodać ile razy była kolizja bool[] hits;
	// [x] Dodać linie
	// [x] Dodac tekst
	// [ ] Wymuś prostopadłość do płaszczyzny

public class ObjectProjecter : MonoBehaviour {	
	Object3D OBJECT3D;
	Dictionary<string, Vector3> labeledVertices;
	/// <summary>
	/// projs[k,k+1,...,k+nOfProjDirs-1] dotyczą rzutów na różne płaszczyzny tego samego wierzchołka, przez co mają taką samą nazwę
	/// projs[k, C*k, 2C*k,...] dotyczą rzutów różnych wierzchołków na tą samą płaszczyznę dla C->(0,nOfProjDirs-1)
	/// </summary>
	ProjectionInfo[] projs;
	Vector3[] rayDirections = {Vector3.right*10, Vector3.down*10, Vector3.forward*10}; //ray w kierunku: X, -Y i Z +rzekome_własne_kierunki
	int nOfProjDirs=3; //liczba rzutni jak ww.
	bool showlines = true;
	public void InitVertexProjecter(Object3D obj ,Dictionary<string, Vector3> labeledVertices){
		this.OBJECT3D = obj;
		this.labeledVertices = labeledVertices;
		CreateHitPoints();
	}
	
	void Update () {
		if(OBJECT3D != null){
			GenerateRays();
			//RysujRzutyNaŚcianach() TODO
		}
	}
	void GenerateRays()
    {
		this.OBJECT3D.GetComponent<MeshCollider>().enabled = false; //wyłączenie collidera bo raye go nie lubią i się z nim zderzają
		for(int k = 0; k < nOfProjDirs; k++){
			int i = k; //przeczytaj opis projs[] jak nie wiesz
			foreach (var pair in labeledVertices)
			{
				Vector3 vertex = transform.TransformPoint(pair.Value); //magic
				Ray ray = new Ray(vertex, rayDirections[k]);
				Debug.DrawRay(vertex, rayDirections[k]);
				ResolveProjection(ray,i);
				i+=nOfProjDirs;
			}			
		}
		this.OBJECT3D.GetComponent<MeshCollider>().enabled = true; //włączenie collidera żeby móc obracać obiektem
    }
    void ResolveProjection(Ray ray, int idx)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
			DrawProjection(projs[idx], ray, hit);			
		}
    }
	
	private void DrawProjection(ProjectionInfo proj, Ray ray, RaycastHit hit){
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
		int length = nOfProjDirs * labeledVertices.ToArray().Length;
		string[] names = labeledVertices.Keys.ToArray();
		projs = new ProjectionInfo[length];
		/*info
		points[k,k+1,...,k+nOfProjDirs-1] dotyczą różnych rzutów tego samego wierzchołka, przez co mają taką samą nazwę
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
            textMesh.text = names[i/nOfProjDirs]; //
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
			
			projs[i] = new ProjectionInfo(marker, label, lineRenderer);

		}
    }
}
