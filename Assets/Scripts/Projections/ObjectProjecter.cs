using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
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
	List<EdgeInfo> edges = null;
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
	/// Liczba rzutni
	/// </summary>
	int nOfProjDirs; //liczba rzutni jak ww.
	/// <summary>
	/// Pokazywanie promieni rzutowania
	/// </summary>
	bool showlines = false;

	/// <summary>
	/// Pilnuje prostopadłości rzutów
	/// </summary>
	bool perpendicularity = false;
	/// <summary>
	/// Opisuje zbiór rzutowanych wierzchołków na danej płaszczyźnie
	/// </summary>
	Dictionary<int, Dictionary<string, VertexProjection>> verticesOnWalls = new Dictionary<int, Dictionary<string, VertexProjection>>();
	/// <summary>
	/// Opisuje zbiór rzutowanych krawędzi na danej płaszczyźnie
	/// </summary>
	Dictionary<int, Dictionary<int, EdgeProjection>> egdesOnWalls = new Dictionary<int, Dictionary<int, EdgeProjection>>();

	/// <summary>
	/// Inicjuje mechanizm rzutowania
	/// </summary>
	/// <param name="obj">Referencja na strukturę Object3D</param>
	/// <param name="projectionInfo">Metadane dotyczące wyświetlania rzutów</param>
	/// <param name="rotatedVertices">Oznaczenia wierzchołków</param>
	/// <param name="edges">Lista krawędzi</param>
	public void InitProjecter(Object3D obj,ProjectionInfo projectionInfo, Dictionary<string, Vector3> rotatedVertices, List<EdgeInfo> edges){
		this.OBJECT3D = obj;
		this.rotatedVertices = rotatedVertices;
		this.edges = edges;
		this.projectionInfo = projectionInfo;
		if(OBJECT3D != null && rotatedVertices != null && edges != null){
			CreateRayDirections();
			CreateHitPoints();
		}
	}	
	/// <summary>
	/// Aktualizuje rzutowanie
	/// </summary>
	void Update () {
		if(OBJECT3D != null && rotatedVertices != null && edges != null){
			ProjectObject();
			//CastPointWithCollision
			if(perpendicularity){
				RefreshRayDirection();
			}
		}
	}
	/// <summary>
	/// Tworzy na podstawie pozycji ścian, wektor rzutujący w kierunku prostopadłum do płaszczyzn
	/// </summary>
	void CreateRayDirections(){
		GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
		nOfProjDirs = walls.Length;
		rayDirections = new Vector3[nOfProjDirs];
		RefreshRayDirection();
	}
	/// <summary>
	/// Odświerza rzuty w kierunku prostopadłum do płaszczyzn
	/// </summary>
	public void RefreshRayDirection(){
		GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
		int i=0;
		const float LengthOfTheRay = 10f;
		foreach (GameObject wall in walls)
        {
			Transform tr = wall.transform;
		   	Vector3 pos = tr.position;
			Vector3 normal = tr.TransformVector(Vector3.right); //patrz w kierunku X, czewona strzałka UNITY
			rayDirections[i++] = (-1f) * normal*LengthOfTheRay; // minus bo w przeciwnym kierunku niż patzry ściana, czyli patrzymy na ścianę
        }
	}
	/// <summary>
	/// Rzutuje punkty i krawędzie bryły na płaszczyzny
	/// </summary>
	void ProjectObject(){
		this.OBJECT3D.GetComponent<MeshCollider>().enabled = false; //wyłączenie collidera bo raye go nie lubią i się z nim zderzają
		foreach (var edge in edgesprojs)
		{
				CastRay(edge.start, edge.nOfProj);
				CastRay(edge.end, edge.nOfProj);
				DrawEgdeLine(edge);
		}			
		this.OBJECT3D.GetComponent<MeshCollider>().enabled = true; //włączenie collidera żeby móc obracać obiektem	
    }
	/// <summary>
	/// Wyznacza rzut punktu na zadaną płaszczyznę
	/// </summary>
	/// <param name="vproj">Rzut punktu</param>
	/// <param name="direction">Numer płaszczyzny</param>
	private void CastRay(VertexProjection vproj, int direction){
		Vector3 vertex = rotatedVertices[vproj.vertexName];
		Ray ray = new Ray(vertex, rayDirections[direction]);
		Debug.DrawRay(vertex, rayDirections[direction]);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit))
		{
			DrawVertexProjection(vproj, ray, hit);			
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
		///katalog organizujący rzuty wierzchołków
        var VertexProjections = new GameObject("VertexProjections");
        VertexProjections.transform.SetParent(gameObject.transform);
		///katalog organizujący rzuty krawędzi
        var EdgeProjections = new GameObject("EdgeProjections");
        EdgeProjections.transform.SetParent(gameObject.transform);
		
		for(int k = 0; k < nOfProjDirs; k++){
			//iterujemy po krawędziach, wierzchołki się powtórzą więc żeby kilku tych samych rzutów jednego wierzchołka nie było to słownik
			Dictionary<string, VertexProjection> vertexOnThisPlane = new Dictionary<string, VertexProjection>();
			//słownik krawędzi na danej rzutni
			Dictionary<int, EdgeProjection> egdeOnThisPlane = new Dictionary<int, EdgeProjection>();
			int i=0; //numer krawędzi, potrzebny do Dictionary
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
				EdgeProjection edgeProj = CreateEgdeProjection(EdgeProjections, p1, p2, k, edge.label);
				edgesprojs.Add(edgeProj);

				egdeOnThisPlane[i++] = edgeProj;
			}
			verticesOnWalls[k] = vertexOnThisPlane;
			egdesOnWalls[k] = egdeOnThisPlane;
		}
    }
	/// <summary>
	/// Rysuje krawędź na rzutni
	/// </summary>
	/// <param name="egdeproj">Informacje o rzutowanej krawędzi</param>
	private void DrawEgdeLine(EdgeProjection egdeproj){
		//pobierz wsp. wierzchołków na rzutni
		Vector3 point1 = egdeproj.start.vertex.GetCoordinates();
		Vector3 point2 = egdeproj.end.vertex.GetCoordinates();

		//egdeproj.lineRenderer.SetPosition(0, point1);
		//egdeproj.lineRenderer.SetPosition(1, point2);
		egdeproj.line.SetCoordinates(point1, point2);
	}
	/// <summary>
	/// Tworzy rzut punktu na płaszczyznę
	/// </summary>
	/// <param name="VertexProjectionsDir">Katalog organizujący rzuty wierzchołków</param>
	/// <param name="name">Nazwa rzutowanego wierzchołka</param>
	/// <param name="nOfProj">Numer rzutni</param>
	/// <returns>Informacje o rzucie punktu na daną płaszczyznę</returns>
	private VertexProjection CreateVertexProjection(GameObject VertexProjectionsDir, string name, int nOfProj){
		GameObject obj = new GameObject("VertexProjection P("+nOfProj+") " + name);
		obj.transform.SetParent(VertexProjectionsDir.transform);
		GameObject Point = new GameObject("Point P("+nOfProj+") " + name);
		Point.transform.SetParent(obj.transform);
		GameObject Line = new GameObject("Line P("+nOfProj+") " + name);
		Line.transform.SetParent(obj.transform);

		//znacznik
		Point vertexObject = Point.AddComponent<Point>();
		vertexObject.SetStyle(projectionInfo.pointColor, projectionInfo.pointSize);
		vertexObject.SetLabel(name + new String('\'', nOfProj + 1), projectionInfo.pointLabelSize, projectionInfo.pointLabelColor);
		
		///linia rzutująca
		LineSegment lineseg = Line.AddComponent<LineSegment>();
		lineseg.SetStyle(projectionInfo.projectionLineColor, projectionInfo.projectionLineWidth);
		lineseg.SetLabel("", projectionInfo.projectionLabelSize, projectionInfo.projectionLabelColor);

		return new VertexProjection(ref vertexObject, ref lineseg, name);
	}
	/// <summary>
	/// Tworzy rzut krawędzi na płaszczyznę
	/// </summary>
	/// <param name="EdgeProjectionsDir">Katalog organizujący rzuty krawędzi</param>
	/// <param name="p1">Pierwszy zrzutowany punkt krawędzi</param>
	/// <param name="p2">Drugi zrzutowany punkt krawędzi</param>
	/// <param name="nOfProj">Numer rzutni</param>
	/// <param name="label">Etykieta krawędzi</param>
	/// <returns>Informacje o rzucie krawędzi na daną płaszczyznę</returns>
	private EdgeProjection CreateEgdeProjection(GameObject EdgeProjectionsDir, VertexProjection p1, VertexProjection p2, int nOfProj, string label){
		GameObject edge = new GameObject("EgdeLine P("+nOfProj +") " + p1.vertexName+p2.vertexName);
		edge.transform.SetParent(EdgeProjectionsDir.transform);
		LineSegment drawEdge = edge.AddComponent<LineSegment>();
		drawEdge.SetStyle(projectionInfo.edgeLineColor, projectionInfo.edgeLineWidth);
		drawEdge.SetLabel(label, projectionInfo.edgeLabelSize, projectionInfo.edgeLabelColor);	
		return new EdgeProjection(nOfProj, drawEdge, p1, p2);		
	}
	/// <summary>
	/// Przełącza widoczność linii rzutujących
	/// </summary>
	public void SetShowingLines(){
		showlines = !showlines;
	}
	/// <summary>
	/// Przełącza pilnowanie prostopadłości rzutu
	/// </summary>
	public void watchPerpendicularity(){
		perpendicularity = !perpendicularity;
	}


}
