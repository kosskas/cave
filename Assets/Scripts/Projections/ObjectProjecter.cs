﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
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
	/// Pokazywanie linii odnoszących
	/// </summary>
	bool showPerpenLines = false;

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
	/// Lista par ścian prostopadłych
	/// </summary>
	List<Tuple<int, int>> perpenWalls;
	/// <summary>
	/// Lista linii odnoszących
	/// </summary>
	List<Tuple<EdgeProjection,EdgeProjection>> referenceLines;
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
			ProjectObject();
			AddReferenceLines();
		}
	}	
	/// <summary>
	/// Aktualizuje rzutowanie
	/// </summary>
	void Update () {
		if(OBJECT3D != null && rotatedVertices != null && edges != null){
			ProjectObject();
			if(perpenWalls != null && referenceLines != null){
				ProjectReferenceLines();
			}			
			if(perpendicularity){
				RefreshRayDirection();
			}
		}
	}
	/// <summary>
	/// Przełącza widoczność linii rzutujących
	/// </summary>
	public void SetShowingProjectionLines(){
		showlines = !showlines;
	}
	/// <summary>
	/// Przełącza widoczność linii odnoszących
	/// </summary>
	public void SetShowingReferenceLines(){
		showPerpenLines = !showPerpenLines;
	}
	/// <summary>
	/// Przełącza pilnowanie prostopadłości rzutu
	/// </summary>
	public void watchPerpendicularity(){
		perpendicularity = !perpendicularity;
	}
	/// <summary>
	/// Odświerza rzuty w kierunku prostopadłum do płaszczyzn
	/// </summary>
	public void RefreshRayDirection(){
		GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
		int idx=0;
		const float LengthOfTheRay = 10f;
		foreach (GameObject wall in walls)
        {
			Transform tr = wall.transform;
		   	Vector3 pos = tr.position;
			Vector3 normal = tr.TransformVector(Vector3.right); //patrz w kierunku X, czewona strzałka UNITY
			rayDirections[idx++] = (-1f) * normal*LengthOfTheRay; // minus bo w przeciwnym kierunku niż patzry ściana, czyli patrzymy na ścianę
        }
	}
	/// <summary>
	/// Tworzy na podstawie pozycji ścian, wektor rzutujący w kierunku prostopadłum do płaszczyzn
	/// </summary>
	private void CreateRayDirections(){
		GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
		nOfProjDirs = walls.Length;
		rayDirections = new Vector3[nOfProjDirs];
		RefreshRayDirection();
		perpenWalls = FindPerpendicularWallsToGroundWall(walls);
	}
	/// <summary>
	/// Rzutuje punkty i krawędzie bryły na płaszczyzny
	/// </summary>
	private void ProjectObject(){
		this.OBJECT3D.GetComponent<MeshCollider>().enabled = false; //wyłączenie collidera bo raye go nie lubią i się z nim zderzają
		foreach (var edge in edgesprojs)
		{
				CastRay(edge.start, edge.nOfProj);
				CastRay(edge.end, edge.nOfProj);
				DrawEgdeLine(edge, true);
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
				EdgeProjection edgeProj = CreateEgdeProjection(EdgeProjections, p1, p2,edge.label,k);
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
	/// <param name="show">Ustawienie widoczności</param>
	private void DrawEgdeLine(EdgeProjection egdeproj, bool show){
		egdeproj.line.SetEnable(show);
		//pobierz wsp. wierzchołków na rzutni
		Vector3 point1 = egdeproj.start.vertex.GetCoordinates();
		Vector3 point2 = egdeproj.end.vertex.GetCoordinates();

		//egdeproj.lineRenderer.SetPosition(0, point1);
		//egdeproj.lineRenderer.SetPosition(1, point2);
		egdeproj.line.SetCoordinates(point1, point2);
	}
	/// <summary>
	/// Znajduje ściany podstopadłe do ściany leżącej na podłodze
	/// </summary>
	/// <param name="walls">Lista wszystkich ścian</param>
	/// <returns>Lista par ścian prostopadłych do siebie, null jeśli nie ma ściany leżązej na podłodze</returns>
	private List<Tuple<int, int>> FindPerpendicularWallsToGroundWall(GameObject[] walls){
		//Debug.Log(Vector3.up); jesłi tylko podłoga to znajdz == wall.transform.right
		List<Tuple<int, int>> tmp = new List<Tuple<int, int>>();
		//znajdz groundwall
		GameObject groundWall = null;
		int i =0;
		foreach (GameObject wall in walls){
			if(wall.transform.right == Vector3.up){
				groundWall = wall;
				break;
			}
			i++;
		}
		if(groundWall == null){
			Debug.Log("Nie znaleziono sciany na podlodze");
			return null;
		}
		Debug.Log(groundWall.name);
		int j =0;
		foreach (GameObject wall in walls)
        {
			float dot = Vector3.Dot(groundWall.transform.right, wall.transform.right);
			float eps = 1e-6F;
			if(dot < eps && dot > -eps){
				tmp.Add(new Tuple<int, int>(i, j));
				Debug.Log(groundWall.name+ "   "+wall.name);
			}
			j++;
		}
		// for(int i =0; i < walls.Length; i++){
		// 	for(int j =i; j < walls.Length; j++){
		// 		float dot = Vector3.Dot(walls[i].transform.right, walls[j].transform.right);
		// 		float eps = 1e-6F;
		// 		if(dot < eps && dot > -eps){
		// 			tmp.Add(new Tuple<int, int>(i, j));
		// 			Debug.Log(walls[i].name+ "   "+walls[j].name);
		// 		}
		// 	}
		// }
		return tmp;
	}
	/// <summary>
	/// Dodaje linie odnoszące
	/// </summary>
	private void AddReferenceLines(){
		var ReferenceLinesDir = new GameObject("ReferenceLines");
        ReferenceLinesDir.transform.SetParent(gameObject.transform);
		var crossPointsDir = new GameObject("crossPointsDir");
        crossPointsDir.transform.SetParent(gameObject.transform);
		referenceLines = new List<Tuple<EdgeProjection, EdgeProjection>>();
		foreach(var pair in perpenWalls){
			int wall1 = pair.Item1;
			int wall2 = pair.Item2;
			foreach(var vertice in rotatedVertices){
				VertexProjection v1 = verticesOnWalls[wall1][vertice.Key];
				VertexProjection v2= verticesOnWalls[wall2][vertice.Key];
				EdgeProjection refLine1 = CreateEgdeProjection(ReferenceLinesDir, CreateVertexProjection(crossPointsDir, "",wall1+10*wall2), v1, "",wall1+10*wall2);
				EdgeProjection refLine2 = CreateEgdeProjection(ReferenceLinesDir, CreateVertexProjection(crossPointsDir, "",wall1+10*wall2), v2, "",wall1+10*wall2);			
				referenceLines.Add(new Tuple<EdgeProjection, EdgeProjection>(refLine1, refLine2));
			}
		}
	}
	/// <summary>
	/// Rysuje linie odnoszące
	/// </summary>
	private void ProjectReferenceLines(){
		int i =0;
		foreach(var pair in perpenWalls){
			int wall1 = pair.Item1;
			int wall2 = pair.Item2;
			foreach(var vertice in rotatedVertices){
				VertexProjection v1 = verticesOnWalls[wall1][vertice.Key];
				VertexProjection v2= verticesOnWalls[wall2][vertice.Key];
				Vector3 p = vertice.Value;		
				Vector3 cross = FindCrossingPoint(p, v1.vertex.GetCoordinates(), v2.vertex.GetCoordinates());
				referenceLines[i].Item1.start.vertex.SetCoordinates(cross);
				referenceLines[i].Item2.start.vertex.SetCoordinates(cross);
				DrawEgdeLine(referenceLines[i].Item1, showPerpenLines);
				DrawEgdeLine(referenceLines[i].Item2, showPerpenLines);
				i++;
			}
		}
	}

    /// <summary>
	/// Znajduje punkt przecięcia prostopadłych ścian dla dwóch rzutów i punktu w 3D
	/// </summary>
	/// <param name="vec1">Punkt w 3D</param>
	/// <param name="vec2">Rzut wierzchołka na 1 ścianie</param>
	/// <param name="vec3">Rzut wierzchołka na 2 ścianie</param>
	/// <returns>Punkt przecięcia ścian</returns>
    private Vector3 FindCrossingPoint(Vector3 vec1, Vector3 vec2, Vector3 vec3)
    {		
		/*
		v1 = (A, b, c)
		v2 = (A, e, c)
		v3 = (A, b, f)
		ret = (A, e, f)
		*/
        Vector3 ret = Vector3.zero;
		const float eps = 0.0001f;
        for (int i = 0; i < 3; i++)
        {
			bool cmp_v12 = Mathf.Abs(vec1[i] - vec2[i]) < eps;
			bool cmp_v13 = Mathf.Abs(vec1[i] - vec3[i]) < eps;
			bool cmp_v23 = Mathf.Abs(vec2[i] - vec3[i]) < eps;
            if (cmp_v12 && cmp_v23)
            {
                ret[i] = vec1[i];
            }
            if (cmp_v12 && !cmp_v23)
            {
                ret[i] = vec3[i];
            }
			else if (cmp_v23 && !cmp_v13)
            {
                ret[i] = vec1[i];
            }
			else if (cmp_v13 && !cmp_v23)
            {
                ret[i] = vec2[i];
            }     
        }
        return ret;
    }
    /// <summary>
	/// Tworzy rzut punktu na płaszczyznę
	/// </summary>
	/// <param name="VertexProjectionsDir">Katalog organizujący rzuty wierzchołków</param>
	/// <param name="name">Nazwa rzutowanego wierzchołka</param>
	/// <param name="nOfProj">Numer rzutni</param>
	/// <returns>Rzut punktu na daną płaszczyznę</returns>
	private VertexProjection CreateVertexProjection(GameObject VertexProjectionsDir, string name, int nOfProj){
		GameObject obj = new GameObject(VertexProjectionsDir.name+" P("+nOfProj+") " + name);
		obj.transform.SetParent(VertexProjectionsDir.transform);
		GameObject Point = new GameObject("Point P("+nOfProj+") " + name);
		Point.transform.SetParent(obj.transform);
		GameObject Line = new GameObject("Line P("+nOfProj+") " + name);
		Line.transform.SetParent(obj.transform);

		//znacznik
		Point vertexObject = Point.AddComponent<Point>();
		vertexObject.SetStyle(projectionInfo.pointColor, projectionInfo.pointSize);
		vertexObject.SetLabel(name, projectionInfo.pointLabelSize, projectionInfo.pointLabelColor);
		
		///linia rzutująca
		LineSegment lineseg = Line.AddComponent<LineSegment>();
		lineseg.SetStyle(projectionInfo.projectionLineColor, projectionInfo.projectionLineWidth);
		lineseg.SetLabel("", projectionInfo.projectionLabelSize, projectionInfo.projectionLabelColor);

		return new VertexProjection(vertexObject, lineseg, name);
	}


    /// <summary>
	/// Tworzy rzut krawędzi na płaszczyznę
	/// </summary>
	/// <param name="EdgeProjectionsDir">Katalog organizujący rzuty krawędzi</param>
	/// <param name="p1">Pierwszy zrzutowany punkt krawędzi</param>
	/// <param name="p2">Drugi zrzutowany punkt krawędzi</param>
	/// <param name="nOfProj">Numer rzutni</param>
	/// <param name="label">Etykieta krawędzi</param>
	/// <returns>Rzut krawędzi na daną płaszczyznę</returns>
	private EdgeProjection CreateEgdeProjection(GameObject EdgeProjectionsDir, VertexProjection p1, VertexProjection p2, string label, int nOfProj){
		GameObject edge = new GameObject(EdgeProjectionsDir.name+" P("+nOfProj +") " + p1.vertexName+p2.vertexName);
		edge.transform.SetParent(EdgeProjectionsDir.transform);
		LineSegment drawEdge = edge.AddComponent<LineSegment>();
		drawEdge.SetStyle(projectionInfo.edgeLineColor, projectionInfo.edgeLineWidth);
		drawEdge.SetLabel(label, projectionInfo.edgeLabelSize, projectionInfo.edgeLabelColor);	
		return new EdgeProjection(nOfProj, drawEdge, p1, p2);		
	}
}
// Debug.Log("Punkt " + vertice.Key);
// Debug.Log(p.x.ToString("F3") + ", " + p.y.ToString("F3") + ", " + p.z.ToString("F3"));
// Debug.Log("W1 " + wall1);
// Debug.Log(v1.vertex.GetCoordinates().x.ToString("F3") + ", " + v1.vertex.GetCoordinates().y.ToString("F3") + ", " + v1.vertex.GetCoordinates().z.ToString("F3"));
// Debug.Log("W2 " + wall2);
// Debug.Log(v2.vertex.GetCoordinates().x.ToString("F3") + ", " + v2.vertex.GetCoordinates().y.ToString("F3") + ", " + v2.vertex.GetCoordinates().z.ToString("F3"));			
				
// 				//Vector3 f = FindCommonAxis(p, v1.vertex.GetCoordinates(), v2.vertex.GetCoordinates());
// //Debug.Log(f.x.ToString("F3") + ", " + f.y.ToString("F3") + ", " + f.z.ToString("F3"));
// 				Vector3 f = FindCrossingPoint(p, v1.vertex.GetCoordinates(), v2.vertex.GetCoordinates());
// Debug.Log(f.x.ToString("F3") + ", " + f.y.ToString("F3") + ", " + f.z.ToString("F3"));
//return;