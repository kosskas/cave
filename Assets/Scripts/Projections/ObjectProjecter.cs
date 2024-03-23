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
	Dictionary<string, Vector3> rotatedVertices;
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
	/// <param name="rotatedVertices">Oznaczenia wierzchołków</param>
	/// <param name="faces"></param>
	public void InitProjecter(Object3D obj,ProjectionInfo projectionInfo, Dictionary<string, Vector3> rotatedVertices, List<EdgeInfo> edges){
		this.OBJECT3D = obj;
		this.rotatedVertices = rotatedVertices;
		this.edges = edges;
		this.projectionInfo = projectionInfo;

		CreateRayDirections();
		CreateHitPoints();
	}
	
	/// <summary>
	/// Aktualizuje rzutowanie
	/// </summary>
	void Update () {
		if(OBJECT3D != null){
			GenerateRays();
			CastPoints();
			DrawEgdesProjection();
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

		foreach (var edge in edgesprojs)
		{
				CastRay(edge.start, edge.nOfProj);
				CastRay(edge.end, edge.nOfProj);
		}			
		this.OBJECT3D.GetComponent<MeshCollider>().enabled = true; //włączenie collidera żeby móc obracać obiektem	
    }

	private void CastRay(VertexProjection vproj, int direction){
			Vector3 vertex = rotatedVertices[vproj.vertexName];
			//Vector3 vertex = vproj.vertex3D;
			Ray ray = new Ray(vertex, rayDirections[direction]);
			Debug.DrawRay(vertex, rayDirections[direction]);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				DrawVertexProjection(vproj, ray, hit);			
			}
	}
	/// <summary>
	/// Sprawdza kolizję do zakrywania wierzchołków
	/// </summary>
	private void CastPoints(){
		///sprawdzenie kolizji do zakrywania wierzch
		for(int k = 0; k < nOfProjDirs; k++){
			int i = k; //przeczytaj opis projs[] jak nie wiesz
			foreach (var pair in rotatedVertices)
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
	private void DrawVertexProjection(VertexProjection proj, Ray ray, RaycastHit hit){
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
       // int length = nOfProjDirs * labeledVertices.ToArray().Length; //l.wierzch x liczba rzutni
        //string[] names = labeledVertices.Keys.ToArray();
        //projs = new VertexProjection[length];
        /*info
        points[k,k+1,...,k+nOfProjDirs-1] dotyczą różnych rzutów tego samego wierzchołka, przez co mają taką samą nazwę
        */
        var VertexProjections = new GameObject("VertexProjections");
        VertexProjections.transform.SetParent(gameObject.transform);

        var EdgeProjections = new GameObject("EdgeProjections");
        EdgeProjections.transform.SetParent(gameObject.transform);
        //for (int i = 0; i < projs.Length; i++){
		
		for(int k = 0; k < nOfProjDirs; k++){
			Dictionary<string, VertexProjection> vertexOnThisPlane = new Dictionary<string, VertexProjection>();
			foreach(var edge in edges){
				VertexProjection p1 = null;
				VertexProjection p2 = null;
				if(vertexOnThisPlane.ContainsKey(edge.endPoints.Item1)){
					p1 = vertexOnThisPlane[edge.endPoints.Item1];
				}
				else{
					p1 = CreateVertexProjection(VertexProjections, edge.endPoints.Item1, k);
					vertexOnThisPlane[edge.endPoints.Item1] = p1;
				}
				if(vertexOnThisPlane.ContainsKey(edge.endPoints.Item2)){
					p2= vertexOnThisPlane[edge.endPoints.Item2];
				}
				else{
					p2 = CreateVertexProjection(VertexProjections, edge.endPoints.Item2, k);
					vertexOnThisPlane[edge.endPoints.Item2] = p2;
				}
				EdgeProjection edgeProj = CreateEgdeProjection(EdgeProjections, p1, p2, k);
				edgesprojs.Add(edgeProj);
			}
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

	private VertexProjection CreateVertexProjection(GameObject VertexProjectionsDir, string name, int nOfProj){
		GameObject obj = new GameObject("VertexProjection P("+nOfProj+") " + name);
		obj.transform.SetParent(VertexProjectionsDir.transform);
		GameObject Point = new GameObject("Point P("+nOfProj+") " + name);
		Point.transform.SetParent(obj.transform);
		GameObject Line = new GameObject("Line P("+nOfProj+") " + name);
		Line.transform.SetParent(obj.transform);

		//znacznik 1
		Point vertexObject = Point.AddComponent<Point>();
		vertexObject.SetStyle(Color.black, 0.08f);
		//vertexObject.SetCoordinates(null);
		vertexObject.SetLabel(name + new String('\'', nOfProj + 1), 0.08f, Color.white);
		
		///linia1
		LineSegment lineseg = Line.AddComponent<LineSegment>();
		lineseg.SetStyle(Color.black, 0.02f);
		lineseg.SetLabel("", 0.02f, Color.white);
		Vector3 vertex3D = rotatedVertices[name];

		return new VertexProjection(ref vertex3D, ref vertexObject, ref lineseg, name);
	}
	/// <summary>
	/// Tworzy listę linii które będą wyświetlane jako krawędzie na odpowiednich rzutniach
	/// </summary>
	private EdgeProjection CreateEgdeProjection(GameObject EdgeProjectionsDir, VertexProjection p1, VertexProjection p2, int nOfProj){
		GameObject line = new GameObject("EgdeLine P("+nOfProj +") " + p1.vertexName+p2.vertexName);
		line.transform.SetParent(EdgeProjectionsDir.transform);
		LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
		lineRenderer.positionCount = 2;
		lineRenderer.material = new Material(Shader.Find("Transparent/Diffuse")); // Ustawienie defaultowego materiału
		lineRenderer.startWidth = projectionInfo.edgeLineWidth;
		lineRenderer.endWidth = projectionInfo.edgeLineWidth;
		lineRenderer.material.color = projectionInfo.edgeColor;
		
		return new EdgeProjection(nOfProj, lineRenderer, p1, p2);

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
		Vector3 point1 = egdeproj.start.vertex.GetCoordinates();
		Vector3 point2 = egdeproj.end.vertex.GetCoordinates();

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

		egdeproj.lineRenderer.SetPosition(0, point1);
		egdeproj.lineRenderer.SetPosition(1, point2);
	}
	/// <summary>
	/// Przełącza widoczność linii rzutujących
	/// </summary>
	public void SetShowingLines(){
		showlines = !showlines;
	}
}
