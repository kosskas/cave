using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

	// TODO
	// [x] Rozwiązać problem z MeshColliderem
	// [x] Rozwiązać problem z Colliderem gracza
	// [x] Dodać ile razy była kolizja bool[] hits;
	// [x] Dodać linie
	// [x] Dodac tekst
	// [ ] Wymuś prostopadłość do płaszczyzny
	// [+-] Sparapetryzować line itd..

/// <summary>
/// Zarządza projekcją obiektu na płaszczyzny 
/// </summary>
public class ObjectProjecter : MonoBehaviour {	
	/// <summary>
	/// Referencja na strukturę Obiekt3D
	/// </summary>
	Object3D OBJECT3D;
	/// <summary>
	/// Dane dot. wyświetlania rzutów
	/// </summary>
	ProjectionInfo projectionInfo;
	/// <summary>
	/// Oznaczenia wierzchołków
	/// </summary>
	Dictionary<string, Vector3> labeledVertices;
	/// <summary>
	/// Lista krawędzi
	/// </summary>
	///List<Tuple<string, string>> edges = new List<Tuple<string, string>>();
	List<EdgeInfo> edges = null;
	/// <summary>
	/// Tablica określająca rzut wierzchołka na rzutnie.
	/// projs[k,k+1,...,k+nOfProjDirs-1] dotyczą rzutów na różne płaszczyzny tego samego wierzchołka, przez co mają taką samą nazwę
	/// projs[k, C+k, 2C+k,...] dotyczą rzutów różnych wierzchołków na tą samą płaszczyznę dla C->(0,nOfProjDirs-1)
	/// przykład: 	0: A1, 1: A2, 2: A3,
	/// 			3: B1, 4: B2, 5: B3,
	/// 			6: C1, 7: C2, 8: C3
	/// , czyli punkty na rzutni "1" są pod indeksami 0,3,6. Wszystkie rzuty punktu "A" są pod indeksami 0,1,2.
	/// </summary>
	VertexProjection[] projs;
	/// <summary>
	/// Lista krawędzi wyświetlanych na rzutniach
	/// </summary>
	List<EdgeProjection> edgesprojs = new List<EdgeProjection>();
	/// <summary>
	/// Kierunki promieni, prostopadłe
	/// </summary>
	Vector3[] rayDirections = null;
	//Vector3[] rayDirections = {Vector3.right*10, Vector3.down*10, Vector3.forward*10}; //ray w kierunku: X, -Y i Z +rzekome_własne_kierunki
	/// <summary>
	/// liczba rzutni
	/// </summary>
	int nOfProjDirs; //liczba rzutni jak ww.
	/// <summary>
	/// Pokazywanie promieni rzutowania
	/// </summary>
	bool showlines = false;
	/// <summary>
	/// Inicjuje mechanizm rzutowania
	/// </summary>
	/// <param name="obj">Referencja na strukturę Object3D</param>
	/// <param name="labeledVertices">Oznaczenia wierzchołków</param>
	/// <param name="faces"></param>
	public void InitProjecter(Object3D obj,ProjectionInfo projectionInfo, Dictionary<string, Vector3> labeledVertices, List<EdgeInfo> edges){
		this.OBJECT3D = obj;
		this.labeledVertices = labeledVertices;
		this.edges = edges;
		this.projectionInfo = projectionInfo;

		CreateRayDirections();
		CreateHitPoints();
		CreateEgdesProj();
	}
	
	/// <summary>
	/// Aktualizuje rzutowanie
	/// </summary>
	void Update () {
		if(OBJECT3D != null){
			GenerateRays();
			CastPoints();
			//DrawEgdesProjection();
		foreach(var edge in edges){
			Debug.Log(edge.label+ " "+edge.endPoints);
		}
		}
	}

	/// <summary>
	/// Tworzy na podstawie pozycji ścian, wektor rzutujący w kierunku prostopadłum do płaszczyzn
	/// </summary>
	void CreateRayDirections(){
		GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
		int i=0;
		nOfProjDirs = walls.Length;
		rayDirections = new Vector3[nOfProjDirs];
		foreach (GameObject wall in walls)
        {
			Transform tr = wall.transform;
		   	Vector3 pos = tr.position;
			Vector3 normal = tr.TransformVector(Vector3.right); //patrz w kierunku X
			rayDirections[i++] = normal*-10f; // minus bo w przeciwnym kierunku niż patzry ściana, czyli patrzymy na ścianę

        }
	}
	/// <summary>
	/// Odświerza rzuty w kierunku prostopadłum do płaszczyzn
	/// </summary>
	public void RefreshRayDirection(){
		GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
		int i=0;
		foreach (GameObject wall in walls)
        {
			Transform tr = wall.transform;
		   	Vector3 pos = tr.position;
			Vector3 normal = tr.TransformVector(Vector3.right); //patrz w kierunku X
			rayDirections[i++] = normal*-10f; // minus bo w przeciwnym kierunku niż patzry ściana, czyli patrzymy na ścianę

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
				//Vector3 vertex = transform.TransformPoint(pair.Value); //transformacja wierzchołka z pliku tak aby zgadzał się z aktualną pozycją bryły (ze wzgl. jej na obrót) 
				Vector3 vertex = pair.Value;
				Ray ray = new Ray(vertex, rayDirections[k]);
				//Debug.DrawRay(vertex, rayDirections[k]);
				RaycastHit hit;
        		if (Physics.Raycast(ray, out hit))
        		{
					DrawProjection(projs[i], ray, hit);			
				}
				i+=nOfProjDirs;
			}			
		}
		this.OBJECT3D.GetComponent<MeshCollider>().enabled = true; //włączenie collidera żeby móc obracać obiektem		
    }

	/// <summary>
	/// Sprawdza kolizję do zakrywania wierzchołków
	/// </summary>
	private void CastPoints(){
		///sprawdzenie kolizji do zakrywania wierzch
		for(int k = 0; k < nOfProjDirs; k++){
			int i = k; //przeczytaj opis projs[] jak nie wiesz
			foreach (var pair in labeledVertices)
			{
				///offset musi być bo inaczej nie działa
				Vector3 vertex = transform.TransformPoint(pair.Value*1.0001f);//* 1.005f; //dodaj lekki offset tak żeby nie było jakiś dziwnych kolizji
				///

				Ray ray = new Ray(vertex, rayDirections[k]);
				RaycastHit hit;
				//Debug.DrawRay(vertex, rayDirections[k]);
				if (Physics.Raycast(ray, out hit))
        		{
					//jeżeli ray z pktu przeleciał przez Obiekt to pkt jest zakryty
					if (hit.collider.CompareTag("Solid")) 
					{
						//oznacz wg jakiej rzutni pkt jest zakryty
						//projs[i].collids[k] = true;
            		}
					else
					{
						//projs[i].collids[k] = false;
					}		
				}
				i+=nOfProjDirs;
			}			
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
		proj.line.SetEnable(showlines);
		if(showlines){
			proj.line.SetCoordinates(ray.origin,hit.point);
		}
		//znacznik
		proj.vertex.SetCoordinates(hit.point);
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
        var VertexProjections = new GameObject("VertexProjections");
        VertexProjections.transform.SetParent(gameObject.transform);

        //for (int i = 0; i < projs.Length; i++){
		int i =0;
		foreach(var edge in edges){
            GameObject obj = new GameObject("VertexProjection " + i);
            obj.transform.SetParent(VertexProjections.transform);
            GameObject Point = new GameObject("Point " + i);
            Point.transform.SetParent(obj.transform);
			GameObject Line = new GameObject("Line " + i);
            Line.transform.SetParent(obj.transform);

            //znacznik
            Point vertexObject = Point.AddComponent<Point>();
            vertexObject.SetStyle(Color.black, 0.08f);
            //vertexObject.SetCoordinates(null);
            vertexObject.SetLabel(names[i/nOfProjDirs] + new String('\'', i%nOfProjDirs + 1), 0.08f, Color.white);
            
            ///linia
            LineSegment lineseg = Line.AddComponent<LineSegment>();
            lineseg.SetStyle(Color.black, 0.02f);
            lineseg.SetLabel("", 0.02f, Color.white);
            Vector3 vertex3D = labeledVertices[names[i / nOfProjDirs]];
            projs[i] = new VertexProjection(ref vertex3D, ref vertexObject, ref lineseg, names[i / nOfProjDirs]);
			i++;

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
		// for(int k = 0; k < nOfProjDirs; k++){
		// 	for(int i = k; i < projs.Length; i+=nOfProjDirs){		
		// 		for(int j = i; j < projs.Length; j+=nOfProjDirs){
		// 			if(OBJECT3D.AreNeighbours(projs[i].name, projs[j].name)){
		// 				GameObject line = new GameObject("EgdeLine");
		// 				LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
		// 				lineRenderer.positionCount = 2;
		// 				lineRenderer.material = new Material(Shader.Find("Transparent/Diffuse")); // Ustawienie defaultowego materiału
		// 				lineRenderer.startWidth = projectionInfo.edgeLineWidth;
		// 				lineRenderer.endWidth = projectionInfo.edgeLineWidth;
		// 				lineRenderer.material.color = projectionInfo.edgeColor;
		// 				line.transform.SetParent(gameObject.transform);

		// 				///dodaj do listy rzutowanych krawędzi
		// 				edgesprojs.Add(new EdgeProjection(k, lineRenderer,projs[i], projs[j]));
		// 			}
		// 		}
		// 	}	
		// }		
	}
	/// <summary>
	/// Rysuje krawędź na rzutni
	/// </summary>
	/// <param name="egdeproj">Informacje o rzutowanej krawędzi</param>
	private void DrawEgdeLine(EdgeProjection egdeproj){
		//pobierz wsp. markerów wierzchołków na rzutni
		// Vector3 point1 = egdeproj.start.marker.transform.position;
		// Vector3 point2 = egdeproj.end.marker.transform.position;
		// ///jeżeli na tej samej rzutni (egdeproj.nOfProj) jeden z tych pktów jest zakryty to oznacz krawędz jako zakrytą
		// if(egdeproj.start.collids[egdeproj.nOfProj] || egdeproj.end.collids[egdeproj.nOfProj]){
		// 	Color color = projectionInfo.edgeColor;
        // 	color.a = 0.2f;
		// 	egdeproj.lineRenderer.material.color = color;
		// }
		// else{
		// 	Color color = projectionInfo.edgeColor;
        // 	color.a = 1.0f;
		// 	egdeproj.lineRenderer.material.color = color;
		// }

		// egdeproj.lineRenderer.SetPosition(0, point1);
		// egdeproj.lineRenderer.SetPosition(1, point2);
	}
	/// <summary>
	/// Przełącza widoczność linii rzutujących
	/// </summary>
	public void SetShowingLines(){
		showlines = !showlines;
	}
	/// <summary>
	/// Zwraca liczbę rzutni na planszy
	/// </summary>
	/// <returns>liczba rzutni</returns>
	public int GetNOfProjections(){
		return nOfProjDirs;
	}
}
