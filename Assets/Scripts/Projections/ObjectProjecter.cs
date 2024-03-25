using System;
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
			ProjectReferenceLines();
			//CastPointWithCollision
			if(perpendicularity){
				RefreshRayDirection();
			}
		}
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
	private void DrawEgdeLine(EdgeProjection egdeproj){
		//pobierz wsp. wierzchołków na rzutni
		Vector3 point1 = egdeproj.start.vertex.GetCoordinates();
		Vector3 point2 = egdeproj.end.vertex.GetCoordinates();

		//egdeproj.lineRenderer.SetPosition(0, point1);
		//egdeproj.lineRenderer.SetPosition(1, point2);
		egdeproj.line.SetCoordinates(point1, point2);
	}
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
		return tmp;
	}

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
				EdgeProjection refLine1 = CreateEgdeProjection(ReferenceLinesDir, CreateVertexProjection(crossPointsDir, "",wall1+2*wall2), v1, "",wall1+2*wall2+1);
				EdgeProjection refLine2 = CreateEgdeProjection(ReferenceLinesDir, CreateVertexProjection(crossPointsDir, "",wall1+2*wall2), v2, "",wall1+2*wall2+1);			
				referenceLines.Add(new Tuple<EdgeProjection, EdgeProjection>(refLine1, refLine2));
			}
		}
	}

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
				DrawEgdeLine(referenceLines[i].Item1);
				DrawEgdeLine(referenceLines[i].Item2);
				i++;
			}
		}
	}

    // Funkcja przyjmująca trzy Vector3 jako argumenty i zwracająca wektor spełniający określone warunki
    private Vector3 FindCrossingPoint(Vector3 vec1, Vector3 vec2, Vector3 vec3)
    {
		
        // Zadeklarowanie wektora wynikowego
        Vector3 resultVector = Vector3.zero;
        // Sprawdzenie dla każdej współrzędnej wektorów wejściowych
        for (int i = 0; i < 3; i++)
        {
            // Warunek, aby jedna wartość była taka sama dla wszystkich wektorów
            if (Compare(vec1[i],vec2[i]) && Compare(vec2[i],vec3[i]))
            {
                resultVector[i] = vec1[i];
            }
            // Warunek, aby jedna wartość była inna niż dla vec1 i vec2
            if (Compare(vec1[i],vec2[i]) && !Compare(vec2[i],vec3[i]))
            {
                resultVector[i] = vec3[i];
            }
			// Warunek, aby jedna wartość była inna niż dla vec2 i vec3
			else if (Compare(vec2[i],vec3[i]) && !Compare(vec3[i],vec1[i]))
            {
                resultVector[i] = vec1[i];
            }
			// Warunek, aby jedna wartość była inna niż dla vec1 i vec3
			else if (Compare(vec1[i],vec3[i]) && !Compare(vec3[i],vec2[i]))
            {
                resultVector[i] = vec2[i];
            }
            
        }

        return resultVector;
    }
    private bool Compare(float a, float b)
    {
        const float epsilon = 0.0001f; // Dokładność do 4 miejsc po przecinku
        return Mathf.Abs(a - b) < epsilon;
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