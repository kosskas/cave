using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
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
    List<EdgeInfo> edges;

    /// <summary>
    /// Kontroler ścian
    /// </summary>
    WallController wc;
    //////////////////////////////////////////


    /// <summary>
    /// Lista krawędzi wyświetlanych na rzutniach
    /// </summary>
    List<EdgeProjection> edgesprojs;
    /// <summary>
    /// Kierunki promieni, prostopadłe
    /// </summary>
    Vector3[] rayDirections;
    //Vector3[] rayDirections = {Vector3.right*10, Vector3.down*10, Vector3.forward*10}; //ray w kierunku: X, -Y i Z +rzekome_własne_kierunki
    /// <summary>
    /// Liczba rzutni
    /// </summary>
    int nOfProjDirs; //liczba rzutni jak ww.
    /// <summary>
    /// Pokazywanie promieni rzutowania
    /// </summary>
    bool globalShowlines = false;

    /// <summary>
    /// Pokazywanie linii odnoszących
    /// </summary>
    bool showPerpenLines = false;

    /// <summary>
    /// Opisuje zbiór rzutowanych wierzchołków na danej płaszczyźnie
    /// </summary>
    Dictionary<WallInfo, Dictionary<string, VertexProjection>> verticesOnWalls;
    /// <summary>
    /// Opisuje zbiór rzutowanych krawędzi na danej płaszczyźnie
    /// </summary>
    Dictionary<WallInfo, Dictionary<int, EdgeProjection>> egdesOnWalls;
    /// <summary>
    /// Lista par ścian prostopadłych
    /// </summary>
    List<Tuple<WallInfo, WallInfo>> perpenWalls;
    /// <summary>
    /// Lista linii odnoszących
    /// </summary>
    List<Tuple<EdgeProjection, EdgeProjection>> referenceLines;
    /// <summary>
    /// Katalog organizujący rzuty
    /// </summary>
    GameObject projectionDir = null;

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
        GameObject wallsObject = GameObject.Find("Walls");
        wc = wallsObject.GetComponent<WallController>();
        ResetProjections();
    }
    /// <summary>
    /// Resetuje projekcję rzutów
    /// </summary>
    public void ResetProjections()
    {
        edgesprojs = new List<EdgeProjection>();
        rayDirections = null;
        verticesOnWalls = new Dictionary<WallInfo, Dictionary<string, VertexProjection>>();
        egdesOnWalls = new Dictionary<WallInfo, Dictionary<int, EdgeProjection>>();
        perpenWalls = null;
        referenceLines = new List<Tuple<EdgeProjection, EdgeProjection>>();

        if (projectionDir != null)
        {
            Destroy(GameObject.Find("VertexProjections"));
            Destroy(GameObject.Find("EdgeProjections"));
            Destroy(GameObject.Find("ReferenceLines"));
            Destroy(GameObject.Find("crossPointsDir"));
            Destroy(GameObject.Find("ProjectionDir"));
        }
        projectionDir = new GameObject("ProjectionDir");
        projectionDir.transform.SetParent(gameObject.transform);

        if (OBJECT3D != null && rotatedVertices != null && edges != null)
        {
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
            CheckWallPosition();
        }
	}
	/// <summary>
	/// Przełącza widoczność linii rzutujących
	/// </summary>
	public void SetShowingProjectionLines(){
        globalShowlines = !globalShowlines;
	}
	/// <summary>
	/// Przełącza widoczność linii odnoszących
	/// </summary>
	public void SetShowingReferenceLines(){
		showPerpenLines = !showPerpenLines;
	}
	/// <summary>
	/// Odświerza rzuty w kierunku prostopadłum do płaszczyzn
	/// </summary>
	public void RefreshRayDirection(){
		Vector3[] normals = wc.GetWallNormals();
		Debug.Log(normals.Length);
        const float LengthOfTheRay = 10f;
        for (int  i = 0; i < normals.Length; i++)
		{
			rayDirections[i] = (-1f) * LengthOfTheRay* normals[i]; // minus bo w przeciwnym kierunku niż patzry ściana, czyli patrzymy na ścianę

        }
	}
	/// <summary>
	/// Tworzy na podstawie pozycji ścian, wektor rzutujący w kierunku prostopadłum do płaszczyzn
	/// </summary>
	private void CreateRayDirections(){
		nOfProjDirs = wc.GetWallCount();
        rayDirections = new Vector3[nOfProjDirs];
		RefreshRayDirection();
		perpenWalls = wc.FindPerpendicularWallsToGroundWall();
	}
	/// <summary>
	/// Rzutuje punkty i krawędzie bryły na płaszczyzny
	/// </summary>
	private void ProjectObject(){
		foreach(WallInfo wall in egdesOnWalls.Keys)
		{
			foreach(int idx in egdesOnWalls[wall].Keys)
			{
				CastRay(egdesOnWalls[wall][idx].start, egdesOnWalls[wall][idx].nOfProj,wall.showProjection, globalShowlines && wall.showLines);
                CastRay(egdesOnWalls[wall][idx].end, egdesOnWalls[wall][idx].nOfProj, wall.showProjection, globalShowlines && wall.showLines);
                DrawEgdeLine(egdesOnWalls[wall][idx], wall.showProjection);
            }
		}
		/*		
		foreach (var edge in edgesprojs)
		{
				CastRay(edge.start, edge.nOfProj);
				CastRay(edge.end, edge.nOfProj);
				DrawEgdeLine(edge, true);
		}
		*/		
    }
    /// <summary>
    /// Wyznacza rzut punktu na zadaną płaszczyznę
    /// </summary>
    /// <param name="vproj">Rzut punktu</param>
    /// <param name="direction">Numer płaszczyzny</param>
    /// <param name="showProjection">Ustawienie widoczności dla rzutu</param>
    /// <param name="showLines">Ustawienie widoczności dla linii</param>
    private void CastRay(VertexProjection vproj, int direction, bool showProjection, bool showLines)
    {
		const float maxRayLength = 5f;
		Vector3 vertex = rotatedVertices[vproj.vertexid];
		Ray ray = new Ray(vertex, rayDirections[direction]);
		Debug.DrawRay(vertex, rayDirections[direction]);
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRayLength);
        for (int i = 0; i < hits.Length; i++)
        {
            ///ściany mogą się pokrywać dlatego sprawdzam pod konkretną ścianę
            if (hits[i].collider.gameObject == wc.GetWalls()[direction].gameObject)
            {
                DrawVertexProjection(vproj, ray, hits[i], showProjection, showLines);
            }
        }
    }
    /// <summary>
    /// Rysuje wierzchołek na płaszczyźnie
    /// </summary>
    /// <param name="proj">Inforamcje o wierzchołku</param>
    /// <param name="ray">Promień</param>
    /// <param name="hit">Metadana o zderzeniu</param>
    /// <param name="showProjection">Ustawienie widoczności dla rzutu</param>
    /// <param name="showlines">Ustawienie widoczności dla linii</param>
    private void DrawVertexProjection(VertexProjection proj, Ray ray, RaycastHit hit, bool showProjection, bool showlines)
    {
		const float antiztrack = 0.01f;
        //przełączanie widoczności
        proj.line.SetEnable(showProjection && showlines);
		proj.vertex.SetEnable(showProjection);
        Vector3 antiztrackhit = hit.point + antiztrack * hit.normal;
		if(showProjection && showlines)
        {
            //rysuwanie lini wychodzącej z wierzchołka do punktu kolizji
            proj.line.SetCoordinates(ray.origin, antiztrackhit);
		}
		if(showProjection)
		{
            //znacznik
            proj.vertex.SetCoordinates(antiztrackhit);
        }
	}
	/// <summary>
	/// Tworzy rzuty wierzchołków
	/// </summary>
	private void CreateHitPoints()
    {
		///katalog organizujący rzuty wierzchołków
        var VertexProjections = new GameObject("VertexProjections");
        VertexProjections.transform.SetParent(projectionDir.transform);
		///katalog organizujący rzuty krawędzi
        var EdgeProjections = new GameObject("EdgeProjections");
        EdgeProjections.transform.SetParent(projectionDir.transform);
		List<WallInfo> Walls = wc.GetWalls();
		foreach(WallInfo wall in Walls){
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
					p1 = VertexProjection.CreateVertexProjection(VertexProjections, edge.endPoints.Item1, wall.number);

					p1.SetDisplay(edge.endPoints.Item1 + new string('\'', wall.number+1), projectionInfo.pointColor, projectionInfo.pointSize, projectionInfo.pointLabelColor, projectionInfo.pointLabelSize,projectionInfo.projectionLineColor, projectionInfo.projectionLineWidth, projectionInfo.projectionLabelColor, projectionInfo.projectionLabelSize);
                    vertexOnThisPlane[edge.endPoints.Item1] = p1;
				}
				if(vertexOnThisPlane.ContainsKey(edge.endPoints.Item2)){
					p2= vertexOnThisPlane[edge.endPoints.Item2];
				}
				else{
					p2 = VertexProjection.CreateVertexProjection(VertexProjections, edge.endPoints.Item2, wall.number);
					p2.SetDisplay(edge.endPoints.Item2 + new string('\'', wall.number+1), projectionInfo.pointColor, projectionInfo.pointSize, projectionInfo.pointLabelColor, projectionInfo.pointLabelSize,projectionInfo.projectionLineColor, projectionInfo.projectionLineWidth, projectionInfo.projectionLabelColor, projectionInfo.projectionLabelSize);
                    vertexOnThisPlane[edge.endPoints.Item2] = p2;
				}
				EdgeProjection edgeProj = EdgeProjection.CreateEgdeProjection(EdgeProjections, p1, p2,edge.label, wall.number);
				edgeProj.SetDisplay(projectionInfo.edgeLineColor, projectionInfo.edgeLineWidth, projectionInfo.edgeLabelColor, projectionInfo.edgeLabelSize);
				edgesprojs.Add(edgeProj);

				egdeOnThisPlane[i++] = edgeProj;
			}
			verticesOnWalls[wall] = vertexOnThisPlane;
			egdesOnWalls[wall] = egdeOnThisPlane;
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
	/// Dodaje linie odnoszące
	/// </summary>
	private void AddReferenceLines(){
        var ReferenceLinesDir = new GameObject("ReferenceLines");
        ReferenceLinesDir.transform.SetParent(projectionDir.transform);
		var crossPointsDir = new GameObject("crossPointsDir");
        crossPointsDir.transform.SetParent(projectionDir.transform);
		referenceLines = new List<Tuple<EdgeProjection, EdgeProjection>>();
		foreach(var pair in perpenWalls){
            WallInfo wall1 = pair.Item1;
            WallInfo wall2 = pair.Item2;
			foreach(var vertice in rotatedVertices){
				VertexProjection v1 = verticesOnWalls[wall1][vertice.Key];
				VertexProjection v2= verticesOnWalls[wall2][vertice.Key];
			
				VertexProjection vp1 = VertexProjection.CreateVertexProjection(crossPointsDir, "",wall1.number+10*wall2.number);
				VertexProjection vp2 = VertexProjection.CreateVertexProjection(crossPointsDir, "",wall1.number + 10*wall2.number);

				vp1.SetDisplay(v1.vertexid,Color.black, 0f, Color.black, 0f, Color.black, 0f, Color.black, 0f);
				vp2.SetDisplay(v2.vertexid,Color.black, 0f, Color.black, 0f, Color.black, 0f, Color.black, 0f);
				

				EdgeProjection refLine1 = EdgeProjection.CreateEgdeProjection(ReferenceLinesDir, vp1, v1, "",wall1.number + 10*wall2.number);
				EdgeProjection refLine2 = EdgeProjection.CreateEgdeProjection(ReferenceLinesDir, vp2, v2, "",wall1.number + 10*wall2.number);		

				refLine1.SetDisplay(projectionInfo.referenceLineColor, projectionInfo.referenceLineWidth, projectionInfo.referenceLabelColor, projectionInfo.referenceLabelSize);
				refLine2.SetDisplay(projectionInfo.referenceLineColor, projectionInfo.referenceLineWidth, projectionInfo.referenceLabelColor, projectionInfo.referenceLabelSize);	
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
            WallInfo wall1 = pair.Item1;
            WallInfo wall2 = pair.Item2;
			foreach(var vertice in rotatedVertices){
				VertexProjection v1 = verticesOnWalls[wall1][vertice.Key];
				VertexProjection v2= verticesOnWalls[wall2][vertice.Key];
				Vector3 p = vertice.Value;		
				Vector3 cross = wc.FindCrossingPoint(p, v1.vertex.GetCoordinates(), v2.vertex.GetCoordinates());
				referenceLines[i].Item1.start.vertex.SetCoordinates(cross);
				referenceLines[i].Item2.start.vertex.SetCoordinates(cross);
                DrawEgdeLine(referenceLines[i].Item1, showPerpenLines && wall1.showProjection && wall2.showProjection && wall1.showReferenceLines && wall2.showReferenceLines);
				DrawEgdeLine(referenceLines[i].Item2, showPerpenLines && wall1.showProjection && wall2.showProjection && wall1.showReferenceLines && wall2.showReferenceLines);
				i++;
			}
		}
	}
    private void CheckWallPosition()
    {
        //Dictionary<WallInfo, Dictionary<string, VertexProjection>> verticesOnWalls;
        foreach (var wall in verticesOnWalls.Keys) {
            foreach(var str in verticesOnWalls[wall].Keys)
            {
                if (verticesOnWalls[wall][str].vertex.GetCoordinates() == Vector3.zero && wall.showProjection)
                {
                    ///sciana jest zle polozona
                    Renderer renderer = wall.gameObject.GetComponent<Renderer>();
                    if(renderer != null)
                    {
                        Color color = Color.red;
                        color.a = 0.3f;
                        renderer.material.color = color;
                        return;
                    }
                    //errwall.SetFlags(false, false, false);
                }
            }
        }
    }
}