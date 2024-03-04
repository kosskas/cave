using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

	// TODO
	// [x] Rozwiązać problem z MeshColliderem
	// [x] Rozwiązać problem z Colliderem gracza
	// [ ] Dodać ile razy była kolizja bool[] hits;
	// [x] Dodać linie
	// [x] Dodac tekst
	// [ ] Wymuś prostopadłość do płaszczyzny
	// [ ] Sparapetryzować line itd..

public class ObjectProjecter : MonoBehaviour {	
	/// <summary>
	/// Referencja na strukturę Obiekt3D
	/// </summary>
	Object3D OBJECT3D;
	/// <summary>
	/// Oznaczenia wierzchołków
	/// </summary>
	Dictionary<string, Vector3> labeledVertices;
	/// <summary>
	/// Lista krawędzi
	/// </summary>
	List<Tuple<string, string>> edges = new List<Tuple<string, string>>();
	/// <summary>
	/// projs[k,k+1,...,k+nOfProjDirs-1] dotyczą rzutów na różne płaszczyzny tego samego wierzchołka, przez co mają taką samą nazwę
	/// projs[k, C+k, 2C+k,...] dotyczą rzutów różnych wierzchołków na tą samą płaszczyznę dla C->(0,nOfProjDirs-1)
	/// przykład: 	0: A1, 1: A2, 2: A3,
	/// 			3: B1, 4: B2, 5: B3,
	/// 			6: C1, 7: C2, 8: C3
	/// , czyli punkty na rzutni "1" są pod indeksami 0,3,6 
	/// </summary>
	VertexProjection[] projs;

	/// <summary>
	/// Lista krawędzi wyświetlanych na rzutniach
	/// </summary>
	List<EdgeProjection> edgesprojs = new List<EdgeProjection>();
	/// <summary>
	/// Kierunki promieni, prostopadłe
	/// </summary>
	Vector3[] rayDirections = {Vector3.right*10, Vector3.down*10, Vector3.forward*10}; //ray w kierunku: X, -Y i Z +rzekome_własne_kierunki
	/// <summary>
	/// liczba rzutni
	/// </summary>
	int nOfProjDirs=3; //liczba rzutni jak ww.
	/// <summary>
	/// Pokazywanie promieni rzutowania
	/// </summary>
	public bool showlines = true;
	/// <summary>
	/// Inicjuje mechanizm rzutowania
	/// </summary>
	/// <param name="obj">Referencja na strukturę Object3D</param>
	/// <param name="labeledVertices">Oznaczenia wierzchołków</param>
	/// <param name="faces"></param>
	public void InitProjecter(Object3D obj, Dictionary<string, Vector3> labeledVertices, List<Tuple<string, string>> edges){
		this.OBJECT3D = obj;
		this.labeledVertices = labeledVertices;
		this.edges = edges;
		CreateHitPoints();
		CreateEgdesProj();
	}
	
	/// <summary>
	/// Aktualizuje rzutowanie
	/// </summary>
	void Update () {
		if(OBJECT3D != null){
			GenerateRays();
			DrawEgdesProjection();
		}
	}
	/// <summary>
	/// Rzutuje wierzchołki w kierunku płaszczyzn
	/// </summary>
	void GenerateRays()
    {
		this.OBJECT3D.GetComponent<MeshCollider>().enabled = false; //wyłączenie collidera bo raye go nie lubią i się z nim zderzają
		for(int k = 0; k < nOfProjDirs; k++){
			int i = k; //przeczytaj opis projs[] jak nie wiesz
			foreach (var pair in labeledVertices)
			{
				Vector3 vertex = transform.TransformPoint(pair.Value); //transformacja wierzchołka z pliku tak aby zgadzał się z aktualną pozycją bryły (ze wzgl. jej na obrót) 
				Ray ray = new Ray(vertex, rayDirections[k]);
				Debug.DrawRay(vertex, rayDirections[k]);
				ResolveProjection(ray,i);
				i+=nOfProjDirs;
			}			
		}
		this.OBJECT3D.GetComponent<MeshCollider>().enabled = true; //włączenie collidera żeby móc obracać obiektem
    }
	/// <summary>
	/// Rozwiązuje projekcję rzutu
	/// </summary>
	/// <param name="ray">Promień</param>
	/// <param name="idx">Indeks wierzchołka</param>
    void ResolveProjection(Ray ray, int idx)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
			DrawProjection(projs[idx], ray, hit);			
		}
    }
	/// <summary>
	/// Rysuje wierzchołek na płaszczyźnie
	/// </summary>
	/// <param name="proj">Inforamcje o wierzchołku</param>
	/// <param name="ray">Promień</param>
	/// <param name="hit">Metadana o zderzeniu</param>
	private void DrawProjection(VertexProjection proj, Ray ray, RaycastHit hit){
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
	/// <summary>
	/// Tworzy rzuty wierzchołków
	/// </summary>
	private void CreateHitPoints()
    {
		int length = nOfProjDirs * labeledVertices.ToArray().Length; //l.wierzch x liczba rzutni
		string[] names = labeledVertices.Keys.ToArray();
		projs = new VertexProjection[length];
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
            textMesh.text = names[i/nOfProjDirs]; //ta sama nazwa ale inny wymiar
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
			
			projs[i] = new VertexProjection(marker, label, lineRenderer, names[i/nOfProjDirs]);

		}
    }
	/// <summary>
	/// Rysuje krawiędzie miedzy wierzchołkami na rzutniach
	/// </summary>
	private void DrawEgdesProjection(){
		foreach (var edgeproj in edgesprojs){
			DrawEgdeLine(edgeproj);
		}
	}
	/// <summary>
	/// Tworzy listę linii które będą wyświetlane jako krawędzie na odpowiednich rzutniach
	/// </summary>
	private void CreateEgdesProj(){
		for(int k = 0; k < nOfProjDirs; k++){
			for(int i = k; i < projs.Length; i+=nOfProjDirs){		
				for(int j = i; j < projs.Length; j+=nOfProjDirs){
					if(OBJECT3D.AreNeighbours(projs[i].name, projs[j].name)){
						GameObject line = new GameObject("EgdeLine");
						LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
						lineRenderer.positionCount = 2;
						lineRenderer.material = new Material(Shader.Find("Standard")); // Ustawienie defaultowego materiału
						lineRenderer.startWidth = 0.05f;
						lineRenderer.endWidth = 0.05f;
						lineRenderer.material.color = Color.black;
						line.transform.SetParent(gameObject.transform);

						///dodaj do listy rzutowanych krawędzi
						edgesprojs.Add(new EdgeProjection(lineRenderer,projs[i].marker, projs[j].marker));
					}
				}	
			}		
		}
	}
	/// <summary>
	/// Rysuje krawędź na rzutni
	/// </summary>
	/// <param name="egdeproj">Informacje o rzutowanej krawędzi</param>
	private void DrawEgdeLine(EdgeProjection egdeproj){
		egdeproj.lineRenderer.SetPosition(0, egdeproj.start.transform.position);
		egdeproj.lineRenderer.SetPosition(1, egdeproj.end.transform.position);
	}
}
