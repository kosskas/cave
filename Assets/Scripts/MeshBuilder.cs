using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using UnityEditorInternal;
using System.Xml.Serialization;
using System.Reflection;
using System;
using UnityEngine.Internal.Experimental.UIElements;
using System.Text.RegularExpressions;
using UnityEngine.Purchasing;

/// <summary>
/// Klasa MeshBuilder zawiera informację o odtwarzanych punktach i krawędziach w 3D. Jej zadaniem jej wyświetlanie tych obiektów na scenie w sposób poprawny wraz z liniami z nimi związanymi.
/// </summary>
public class MeshBuilder : MonoBehaviour {
    /*TODO
     [?] Przypadki przy dodawaniu
     [x] Sprawdz numeryczne porownianie
     [x] Aktualizuj info jakie krawędzie między wierzchołkami	 
     [x] Jak się określi krawędzie to zrób to samo w 3D
	 [x] Kiedy jest juz to ten ray tylko do wierch ale nie w INF
	 [x] podswietl dodawany na czerwono jesli zla plaszcz, umiejscowienie trzeciego
	 [X] usun takze 3D kiedy są tylko 2
     [x] jezeli sa 3, usuwasz 1, sprawdz nowa pozycje 3d, usun czerwonosci jezeli byly  TO TEST xxx
     [x] Przypadek, wszystko zle, potem usuwasz i robi sie ok
     [ ] Dodaj linie odnoszace i poprawnie je usuwaj
     [ ] Dodaj flage do WallTexta odnosnie pokazywania odnoszacych i rzutujacych
     [ ] Sprawdz czy pasuja "niewlasciwe" linie odnoszace, pomagajace przy dodawaniu rzutu
	*/
    private class PointProjection
	{
        private const int N_OF_REFLINES = 2; //para do podlogi, druga do scian
		public GameObject pointObject;
		public LineSegment projLine;
		public bool is_ok_placed;
        public Tuple<GameObject, GameObject>[] refLine;

		public PointProjection(GameObject pointObject, LineSegment projLine, bool is_ok_placed)
		{
			this.pointObject = pointObject;
            this.projLine = projLine;
            this.is_ok_placed = is_ok_placed;
            this.refLine = new Tuple<GameObject, GameObject>[N_OF_REFLINES];
            this.refLine[0] = null;
            this.refLine[1] = null;
        }

    }
    private enum Status {
        OK,
        PLANE_ERR,
        OTHER_ERR
    }
    private class Edge3D
    {
        public GameObject edgeObject;
        public string firstPoint;
        public string secondPoint;
        /// <summary>
        /// Ile krawędzi jest na ścianach
        /// </summary>
        public int wallOrigins;
        public Edge3D(GameObject edgeObject, string firstPoint, string secondPoint)
        {
            this.edgeObject = edgeObject;
            this.firstPoint = firstPoint;
            this.secondPoint = secondPoint;
            this.wallOrigins = 0;
        }
    }  
    private class Vertice3D
    {
        public GameObject gameObject;
        /// <summary>
        /// Flaga czy pkt3D jest usunięty
        /// </summary>
        public bool deleted = false;
        /// <summary>
        /// Flaga czy pkt3D jest wyłączony
        /// </summary>
        public bool disabled = false;

        public Vertice3D(GameObject gameObject)
        {
            this.gameObject = gameObject;
        }
    }
    const int MAXWALLSNUM = 3;
    /// <summary>
    /// Opisuje zbiór rzutowanych wierzchołków na danej płaszczyźnie
    /// </summary>
    Dictionary<WallInfo, Dictionary<string, PointProjection>> verticesOnWalls = null;

    /// <summary>
    /// Zbiór wierzchołków rysowanych w trójwymiarze. Elementy nigdy nie są usuwane jedynie oznaczane jako usunięte lub wyłączone.
    /// </summary>
	Dictionary<string, Vertice3D> vertices3D = null;

    /// <summary>
    /// Zbiór krawędzi rysowanych w 3D. (klucz to suma etykiet)
    /// </summary>
    Dictionary<string, Edge3D> edges3D = null;

    Dictionary<string, List<string>> adjList = null;

    /// <summary>
    /// Folder z wierch. rysowanymi w 3D
    /// </summary>
	GameObject reconstrVertDir = null;
    /// <summary>
    /// Folder z krawedz. rysowanymi w 3D
    /// </summary>
	GameObject edges3DDir = null;
    /// <summary>
    /// Folder z rzutami rzutującymi
    /// </summary>
    GameObject referenceLinesDir = null;

    WallController wc = null;
    bool blocked = false;
    bool showProjectionLines = true;
    bool showReferenceLines=true;

    // Use this for initialization
    void Start () {
        Init();
    }

	// Update is called once per frame
	void Update()
	{        
        if (blocked)
            return;
        ShowPoints3D();
        ShowEdges3D();
        ShowProjectionLines();
        HandleReferenceLines();
        ShowReferenceLines();
    }
    /// <summary>
    /// Inicjalizuje klasę, tworzy katalogi do przechowywania punktów i krawędzi 3D
    /// </summary>
    public void Init()
    {
        reconstrVertDir = new GameObject("Reconstr. Verticies");
        reconstrVertDir.transform.SetParent(gameObject.transform);
        edges3DDir = new GameObject("Reconstr. edges3DDir");
        edges3DDir.transform.SetParent(gameObject.transform);
        referenceLinesDir = new GameObject("referenceLinesDir");
        verticesOnWalls = new Dictionary<WallInfo, Dictionary<string, PointProjection>>();
        vertices3D = new Dictionary<string, Vertice3D>();
        edges3D = new Dictionary<string, Edge3D>();
        adjList = new Dictionary<string, List<string>>();
        wc = (WallController)FindObjectOfType(typeof(WallController));
        blocked = false;
    }
    /// <summary>
    /// Czyści klasę i blokuje jej działanie
    /// </summary>
    public void ClearAndDisable()
    {
        blocked = true;
        Destroy(reconstrVertDir);
        Destroy(edges3DDir);
        Destroy(referenceLinesDir);
        verticesOnWalls = null;
        vertices3D = null;
        edges3D = null;
        adjList = null;
    }

    /// <summary>
    /// Dodaje rzut punktu do listy punktów odtwarzanego obiektu 3D. Jeśli jest wystarczająco informacji to próbuje stworzyć go w 3D. 
    /// </summary>
    /// <param name="wall">Metadane ściany, na której znajduje się rzut</param>
    /// <param name="label">Etykieta rzutu</param>
    /// <param name="pointObj">Metadana o rzucie: etykieta, połozenie(position) jest takie jakie ma rodzic</param>
	public void AddPointProjection(WallInfo wall, string label, GameObject pointObj)
	{
        if (!verticesOnWalls.ContainsKey(wall)) {
			verticesOnWalls[wall] = new Dictionary<string, PointProjection>();
        }
        PointProjection toAddProj = new PointProjection(pointObj, pointObj.GetComponent<LineSegment>(), false);
        verticesOnWalls[wall][label] = toAddProj;
        //sprawdz czy istnieja już dwa
        List<PointProjection> currPts = GetCurrentPointProjections(label);
        ResolveAddProjection(currPts, toAddProj, label);
    }

    private void ResolveAddProjection(List<PointProjection> currPts, PointProjection toAddProj, string label)
    {
        if (currPts.Count == 1)
        {
            Debug.Log("Pierwszy");
            //nie bylo zadnych rzutow tego pktu
            //bedzie 1
            MarkOK(currPts[0]);
            //Debug.Log($"Point added: wall[{wall.number}] label[{label}] N={wall.GetNormal()} ---- {pointObj.transform.position.x} {pointObj.transform.position.y} {pointObj.transform.position.z}");
        }
        else if (currPts.Count == 2)
        {
            Debug.Log("Drugi");
            //jest juz 1 bedzie 2
            //podejmij rekonstrukcje
            //sprawdz plawszczyzny
            bool p1 = false, p2 = false, p3 = false;
            Status result = Create3DPoint(label, ref p1, ref p2, ref p3);
            if (result == Status.PLANE_ERR)
            {
                //podswietl dodawany na czerwono
                MarkError(currPts[0]);
                MarkError(currPts[1]);

            }
            else if (result == Status.OK)
            {
                MarkOK(currPts[0]);
                MarkOK(currPts[1]);
                //Debug.Log($"Point added: wall[{wall.number}] label[{label}] N={wall.GetNormal()} ---- {pointObj.transform.position.x} {pointObj.transform.position.y} {pointObj.transform.position.z}");
            }
        }
        else
        {
            Debug.Log("Trzeci");

            if (vertices3D.ContainsKey(label) && !(vertices3D[label].deleted || vertices3D[label].disabled))
            {
                //sa juz 2
                //sprawdz czy dobrze postawiony trzeci
                if (Check3Pos(currPts[0].pointObject.transform.position, currPts[1].pointObject.transform.position, currPts[2].pointObject.transform.position))
                {
                    Debug.Log("3 polozony OK");
                    MarkOK(currPts[0]);
                    MarkOK(currPts[1]);
                    MarkOK(currPts[2]);
                    //Debug.Log($"Point added: wall[{wall.number}] label[{label}] N={wall.GetNormal()} ---- {pointObj.transform.position.x} {pointObj.transform.position.y} {pointObj.transform.position.z}");
                }
                else
                {
                    MarkError(toAddProj);
                    Debug.Log("3 polozony ZLE");
                }
            }
            else
            {
                //nie bylo wczesnej pktu, moze byc psrawdz
                Debug.Log("nie ma pktu 3d");
                bool p1 = false, p2 = false, p3 = false;
                Status result = Create3DPoint(label, ref p1, ref p2, ref p3);
                if (result == Status.OK)
                {
                    if (p1)
                    {
                        MarkOK(currPts[0]);
                        MarkOK(currPts[1]);
                        MarkError(currPts[2]);
                    }
                    else if (p2)
                    {
                        MarkError(currPts[0]);
                        MarkOK(currPts[1]);
                        MarkOK(currPts[2]);
                    }
                    else // (!p3)
                    {
                        MarkOK(currPts[0]);
                        MarkError(currPts[1]);
                        MarkOK(currPts[2]);
                    }
                }
                else
                {
                    MarkError(toAddProj);
                    Debug.Log("3 polozony ZLE");
                }
            }
        }
    }
    private bool Check3Pos(Vector3 proj1, Vector3 proj2, Vector3 proj3)
    {
        Vector3 test1 = CalcPosIn3D(proj1, proj2);
        Vector3 test2 = CalcPosIn3D(proj2, proj3);
        Vector3 test3 = CalcPosIn3D(proj1, proj3);
        return (!(test1 == Vector3.zero || test2 == Vector3.zero || test3 == Vector3.zero) && (test1 == test2 && test1 == test3 && test2 == test3));
    }
    /// <summary>
    /// Usuwa rzut punktu z listy punktów odtwarzanego obiektu 3D. Jeśli istnieje pkt w 3D to następuje ponownw obliczenie jego pozycji lub usunięcie
    /// </summary>
    /// <param name="wall">Metadane ściany, na której znajduje się rzut</param>
    /// <param name="label">Etykieta</param>
    public void RemovePointProjection(WallInfo wall, string label)
	{
        //Debug.Log($"usuwanie");
        //rozszerzyć o label
        if (!verticesOnWalls.ContainsKey(wall))
            return;
        if (!verticesOnWalls[wall].ContainsKey(label))
            return;
        PointProjection pointToRemove = verticesOnWalls[wall][label];

        if (pointToRemove == null)
            return;

        
        int count = GetCurrentPointProjections(label).Count;
        CleanProjectionData(wall,label);
        verticesOnWalls[wall].Remove(label);
        if (count == 2) //sa dwa, usun 3d
        {
            if (vertices3D.ContainsKey(label))
            {
                //GameObject todel3D = vertices3D[label];
                //vertices3D.Remove(label);
                //Destroy(todel3D);
                vertices3D[label].deleted = true;
            }
            //byly 2 ale zle polozone
            List<PointProjection> currPts = GetCurrentPointProjections(label);
            MarkOK(currPts[0]);

        }
        else if (count > 2) //sa trzy
        {
            if (vertices3D.ContainsKey(label)) //sa 3 i jest obiekt 3d
            {
                vertices3D[label].deleted = true;

            }
            //moga byc 3 i zle polozone
            //Rekonstruuj
            List<PointProjection> currPts = GetCurrentPointProjections(label);
            bool p1 = false, p2 = false, p3 = false;
            Status result = Create3DPoint(label, ref p1, ref p2, ref p3);
            if (result == Status.PLANE_ERR)
            {
                MarkError(currPts[0]);
                MarkError(currPts[1]);
            }
            else if (result == Status.OK)
            {
                MarkOK(currPts[0]);
                MarkOK(currPts[1]);
            }
        }   
        Debug.Log($"Point removed: wall[{wall.number}] label[{label}] ");
	}
    /// <summary>
    /// Tworzy krawędź w 3D i jeśli jest odpowiednia ilość informacji wyświetla ją
    /// </summary>
    /// <param name="labelA">Etykieta punktu A</param>
    /// <param name="labelB">Etykieta punktu B</param>
    public void AddEdgeProjection(string labelA, string labelB)
    {
        //sprawdz czy taka krawedz juz nie istnieje(zostala stworzona z innej sciany)
        //jesli tak to odnotuj to(przy usuwaniu bedzie przydatne)
        //jesli pierwszy raz to stowrz w 3D
        if (edges3D.ContainsKey(labelA + labelB) || edges3D.ContainsKey(labelB + labelA))
        {
            Debug.Log("Krawedz juz istnieje");
            if(edges3D.ContainsKey(labelA + labelB))
            {
                edges3D[labelA + labelB].wallOrigins++;
            }
            else if (edges3D.ContainsKey(labelB + labelA))
            {
                edges3D[labelB + labelA].wallOrigins++;
            }
        }
        else //1 raz
        {
            Vector3 mocker = Vector3.zero;
            //przypadek kiedy juz jest krawedz ale nie ma okreslonej pos w 3d
            if (!vertices3D.ContainsKey(labelA))
            {
                CreateEntryForPoint(labelA, mocker);
                vertices3D[labelA].disabled = true;
            }
            if (!vertices3D.ContainsKey(labelB))
            {
                CreateEntryForPoint(labelB, mocker);
                vertices3D[labelB].disabled = true;
            }
            GameObject edgeObj = new GameObject("Edge3D " + labelA+ labelB);
            edgeObj.transform.SetParent(edges3DDir.transform);

            LineSegment edge = edgeObj.AddComponent<LineSegment>();
            edge.SetStyle(ReconstructionInfo.EDGE_3D_COLOR, ReconstructionInfo.EDGE_3D_LINE_WIDTH);
            edge.SetCoordinates(
                vertices3D[labelA].gameObject.transform.position,
                vertices3D[labelB].gameObject.transform.position
            );
            string key = labelA + labelB;
            edges3D[key] = new Edge3D(edgeObj,labelA,labelB);
            edges3D[key].wallOrigins++;
        }
    }
    /// <summary>
    /// Usuwa informację o krawędzi w 3D. Jeżeli krawędź istnieje na wielu ścianach to krawędź w 3D zostanie usunięta jeśli nie będzie jej na żadnej ścianie.
    /// </summary>
    /// <param name="labelA">Etykieta punktu A</param>
    /// <param name="labelB">Etykieta punktu B</param>
    /// <param name="nowait">Flaga określająca czy krawędz ma zostać natychmiast usunięta</param>
    public void RemoveEdgeProjection(string labelA, string labelB, bool nowait = false)
    {
        //znajdz klucz
        string key = null;
        if (edges3D.ContainsKey(labelA + labelB))
        {
            key = labelA + labelB;
        }
        else if (edges3D.ContainsKey(labelB + labelA))
        {
            key = labelB + labelA;
        }
        if(key == null)
        {
            Debug.Log("nie ma krawedzi o takich punktach");
            return;
        }
        Edge3D edge = edges3D[key];

        edges3D[key].wallOrigins--; //zmiejsz licznik scian

        if (edges3D[key].wallOrigins < 1 || nowait)
        {
            GameObject todel = edge.edgeObject;
            edges3D.Remove(key);
            Destroy(todel);
        }

    }
    /// <summary>
    /// Ustawia widoczność linii rzutujących
    /// </summary>
    /// <param name="rule">Zadada wyświetlania. True - wyświetlanie, False - brak</param>
    public void SetShowRulesProjectionLine(bool rule)
    {
        showProjectionLines = rule;
    }
    /// <summary>
    /// Ustawia widoczność linii odnoszących
    /// </summary>
    /// <param name="rule">Zadada wyświetlania. True - wyświetlanie, False - brak</param>
    public void SetShowRulesReferencenLine(bool rule)
    {
        showReferenceLines = rule;
    }
    /// <summary>
    /// Sprawdza czy na scianie znajduje juz sie rzut
    /// </summary>
    /// <param name="wall">Metadane ściany, na której znajduje się rzut</param>
    /// <param name="label">Etykieta</param>
    /// <returns>Prawda jeśli już taki rzut jest, Fałsz jeżeli nie ma</returns>
    public bool CheckIfAlreadyExist(WallInfo wall, string label)
    {
        return verticesOnWalls.ContainsKey(wall) && verticesOnWalls[wall].ContainsKey(label);       
    }
    /// <summary>
    /// Pobiera przechowywane aktywne punkty 3D
    /// </summary>
    /// <returns>Zbiór par etykiet i pozycji w 3D</returns>
    public Dictionary<string, Vector3> GetPoints3D()
    {
        Dictionary<string, Vector3> tmp = new Dictionary<string, Vector3>();
        if(vertices3D == null)
        {
            Debug.Log("Nie ma żadnych punktów 3D");
            return null;
        }
        foreach (string label in vertices3D.Keys)
        {
            if (!(vertices3D[label].deleted || vertices3D[label].disabled))
            {
                tmp[label] = vertices3D[label].gameObject.transform.position;
                Debug.Log($"{label} = {vertices3D[label].gameObject.transform.position.x}, {vertices3D[label].gameObject.transform.position.y}, {vertices3D[label].gameObject.transform.position.z}");
            }
        }

        return tmp;
    }
    /// <summary>
    /// Pobiera przechowywaną listę krawędzi wierzchołków 3D
    /// </summary>
    /// <returns>Zbiór par krawędzi</returns>
    public List<Tuple<string, string>> GetEdges3D()
    {
        List<Tuple<string, string>> tmp = new List<Tuple<string, string>>();
        foreach(string key in edges3D.Keys)
        {
            string a = edges3D[key].firstPoint;
            string b = edges3D[key].secondPoint;
            if (!(vertices3D[a].deleted || vertices3D[a].disabled) && (!(vertices3D[b].deleted || vertices3D[b].disabled)))
                tmp.Add(new Tuple<string, string>(edges3D[key].firstPoint, edges3D[key].secondPoint));
        }
        return tmp;
    }

    private Status Create3DPoint(string label,ref bool p1, ref bool p2, ref bool p3)
	{
		int licz = 0;

        List<PointProjection> pointsInfo = GetCurrentPointProjections(label);
        //pointsInfo.Add(pointNode); //dodaj do listy ten jeszcze nie dodany

        //sprawdz ile jest juz rzutow jednego pktu
        licz = pointsInfo.Count;
		///ja nie wiem w jakiej kolejności to wypluwa rzuty. dopoki sa 2 to ok
		if(licz == 2)
		{
			Vector3 rzut1 = pointsInfo[0].pointObject.transform.position;
			Vector3 rzut2 = pointsInfo[1].pointObject.transform.position;

			Vector3 pkt3D = CalcPosIn3D(rzut1, rzut2);
			if (pkt3D != Vector3.zero)
			{
                if (vertices3D.ContainsKey(label))
                {
                    //byl usuniety?, przywroc go w nowej pozycji
                    Point vertexObject = vertices3D[label].gameObject.GetComponent<Point>();
                    vertexObject.SetCoordinates(pkt3D);
                    vertices3D[label].deleted = false;
                    vertices3D[label].disabled = false;

                }
                else
                {
                    //1 raz
                    CreateEntryForPoint(label, pkt3D);
                }

                return Status.OK;

            }
			else
			{
                return Status.PLANE_ERR;
            }
        }
		if (licz > 2) //sa 3 rzuty, zadne moga nie byc wspolne
		{
            Vector3 proj1 = pointsInfo[0].pointObject.transform.position;
            Vector3 proj2 = pointsInfo[1].pointObject.transform.position;
            Vector3 proj3 = pointsInfo[2].pointObject.transform.position;

            Vector3 test1 = CalcPosIn3D(proj1, proj2);
            Vector3 test2 = CalcPosIn3D(proj2, proj3);
            Vector3 test3 = CalcPosIn3D(proj1, proj3);
            //Sprawdz czy 3 jest dobrze polozony
            p1 = (test1 != Vector3.zero);
            p2 = (test2 != Vector3.zero);
            p3 = (test3 != Vector3.zero);
            if (p1)
            {
                if (vertices3D.ContainsKey(label))
                {
                    //byl usuniety?, przywroc go w nowej pozycji
                    Point vertexObject = vertices3D[label].gameObject.GetComponent<Point>();
                    vertexObject.SetCoordinates(test1);
                    vertices3D[label].deleted = false;
                    vertices3D[label].disabled = false;

                }
                else
                {
                    //1 raz
                    CreateEntryForPoint(label, test1);
                }

                return Status.OK;
            }
            if (p2)
            {
                if (vertices3D.ContainsKey(label))
                {
                    //byl usuniety?, przywroc go w nowej pozycji
                    Point vertexObject = vertices3D[label].gameObject.GetComponent<Point>();
                    vertexObject.SetCoordinates(test2);
                    vertices3D[label].deleted = false;
                    vertices3D[label].disabled = false;

                }
                else
                {
                    //1 raz
                    CreateEntryForPoint(label, test2);
                }

                return Status.OK;
            }
            if (p3)
            {
                if (vertices3D.ContainsKey(label))
                {
                    //byl usuniety?, przywroc go w nowej pozycji
                    Point vertexObject = vertices3D[label].gameObject.GetComponent<Point>();
                    vertexObject.SetCoordinates(test3);
                    vertices3D[label].deleted = false;
                    vertices3D[label].disabled = false;

                }
                else
                {
                    //1 raz
                    CreateEntryForPoint(label, test3);
                }

                return Status.OK;
            }
            return Status.PLANE_ERR;

        }
        return Status.OTHER_ERR;

    }

    private void CreateEntryForPoint(string label, Vector3 pos)
    {
        GameObject obj = new GameObject("Vertex3D " + label);
        obj.transform.SetParent(reconstrVertDir.transform);

        Point vertexObject = obj.AddComponent<Point>();
        vertexObject.SetStyle(ReconstructionInfo.POINT_3D_COLOR, ReconstructionInfo.POINT_3D_DIAMETER);
        vertexObject.SetCoordinates(pos);
        vertexObject.SetLabel(label, ReconstructionInfo.LABEL_3D_SIZE, ReconstructionInfo.LABEL_3D_COLOR);

        vertices3D[label] = new Vertice3D(obj);
    }

    private void CleanProjectionData(WallInfo wall, string label)
    {
        PointProjection pointProjection = verticesOnWalls[wall][label];
        //usun linie odnoszące
        for(int i = 0; i < pointProjection.refLine.Length; i++)
        {
            if (pointProjection.refLine[i] != null)
            {
                Destroy(pointProjection.refLine[i].Item1);
                Destroy(pointProjection.refLine[i].Item2);
            }
            pointProjection.refLine[i] = null;
        }        
    }

    private Vector3 CalcPosIn3D(Vector3 vec1, Vector3 vec2)
    {
        Vector3 ret = Vector3.zero;
        const float eps = 0.0001f;
		const float C = 1000;
		

		bool cmp_x = Mathf.Abs(vec1.x - vec2.x) < eps;
        bool cmp_y = Mathf.Abs(vec1.y - vec2.y) < eps;
        bool cmp_z = Mathf.Abs(vec1.z - vec2.z) < eps;

		Debug.Log($"{cmp_x}{cmp_y}{cmp_z}");
		if(!(cmp_x || cmp_y || cmp_z))
		{
			return ret;
		}

		if (cmp_x)
		{
			ret.x = Mathf.Floor(vec1.x * C) / C;
			ret.y = Mathf.Floor(Mathf.Max(vec1.y, vec2.y) * C) / C;
            ret.z = Mathf.Floor(Mathf.Max(vec1.z, vec2.z) * C) / C;
        }
        if (cmp_y)
        {
            ret.x = Mathf.Floor(Mathf.Min(vec1.x, vec2.x) * C) / C;
            ret.y = Mathf.Floor(vec1.y * C) / C;
            ret.z = Mathf.Floor(Mathf.Max(vec1.z, vec2.z) * C) / C;
        }
        if (cmp_z)
        {
            ret.x = Mathf.Floor(Mathf.Min(vec1.x, vec2.x) * C) / C;
            ret.y = Mathf.Floor(Mathf.Max(vec1.y, vec2.y) * C) / C;
            ret.z = Mathf.Floor(vec2.z * C) / C;
        }
        ///Jeżeli rzuty nie znajdują sie na jednej płasz. to DO STH
        return ret;
    }
    private void ShowProjectionLines()
    {
        foreach (WallInfo wall in verticesOnWalls.Keys)
        {
            foreach (string label in verticesOnWalls[wall].Keys)
            {
                Vector3 vertexProj = verticesOnWalls[wall][label].pointObject.transform.position;


                if (vertices3D.ContainsKey(label) && !vertices3D[label].deleted && !vertices3D[label].disabled && verticesOnWalls[wall][label].is_ok_placed) //jest juz pkt 3d
                {
                    LineSegment projLine = verticesOnWalls[wall][label].projLine;
                    if (projLine != null)
                    {
                        projLine.SetCoordinates(vertexProj, vertices3D[label].gameObject.transform.position);
                        projLine.SetEnable(showProjectionLines);
                    }
                }
                else //wolny pkt
                {
                    const int LEN = 10;
                    Vector3 direction = wall.GetNormal();
                    LineSegment projLine = verticesOnWalls[wall][label].projLine;
                    if (projLine != null)
                    {
                        projLine.SetCoordinates(vertexProj, vertexProj + LEN * direction);
                        projLine.SetEnable(showProjectionLines);
                    }
                }
            }
        }
    }

    private void HandleReferenceLines()
    {
        //create if not exist
        foreach (WallInfo wall in verticesOnWalls.Keys)
        {
            foreach (string label in verticesOnWalls[wall].Keys)
            {
                if (verticesOnWalls[wall][label].refLine[0] == null)// && verticesOnWalls[wall][label].is_ok_placed)
                {
                    CreateReferenceLines(referenceLinesDir, wall, label);
                }
            }
        }      
    }

    private void ShowReferenceLines()
    {
        foreach (WallInfo wall in verticesOnWalls.Keys)
        {
            foreach (string label in verticesOnWalls[wall].Keys)
            {
                for(int idx = 0; idx < verticesOnWalls[wall][label].refLine.Length; idx++)
                {
                    if (verticesOnWalls[wall][label].refLine[idx] != null)
                    {
                        LineSegment refp1 = verticesOnWalls[wall][label].refLine[idx].Item1.GetComponent<LineSegment>();
                        LineSegment refp2 = verticesOnWalls[wall][label].refLine[idx].Item2.GetComponent<LineSegment>();
                        refp1.SetEnable(showReferenceLines);
                        refp2.SetEnable(showReferenceLines);
                    }
                }
            }
        }
    }
    private void ShowPoints3D()
    {
        foreach(string label in vertices3D.Keys)
        {
            vertices3D[label].gameObject.SetActive(!(vertices3D[label].deleted || vertices3D[label].disabled));
        }
    }
    private void ShowEdges3D()
    {
        foreach (string key in edges3D.Keys)
        {
            LineSegment line = edges3D[key].edgeObject.GetComponent<LineSegment>();
            line.SetCoordinates(vertices3D[edges3D[key].firstPoint].gameObject.transform.position, vertices3D[edges3D[key].secondPoint].gameObject.transform.position);
            line.SetEnable(!(vertices3D[edges3D[key].firstPoint].deleted || vertices3D[edges3D[key].firstPoint].disabled || vertices3D[edges3D[key].secondPoint].deleted || vertices3D[edges3D[key].secondPoint].disabled));
        }
    }
    private List<PointProjection> GetCurrentPointProjections(string label)
	{
		List<PointProjection> pointsInfo = new List<PointProjection>();
        //sprawdz ile jest juz rzutow jednego pktu
        foreach (WallInfo wall in verticesOnWalls.Keys)
        {
            if (verticesOnWalls[wall].ContainsKey(label))
            {
                pointsInfo.Add(verticesOnWalls[wall][label]);
            }
        }
		return pointsInfo;
    }

    private void MarkError(PointProjection pointProj)
    {
        Point et = pointProj.pointObject.GetComponent<Point>();
        if (et == null)
        {
            Debug.LogError("GameObj nie ma komponentu Point");
            return;
        }
        et.SetLabel(ReconstructionInfo.LABEL_3D_ERR_COLOR);
        pointProj.projLine.SetStyle(ReconstructionInfo.PROJECTION_LINE_ERROR_COLOR, ReconstructionInfo.PROJECTION_LINE_WIDTH);
        pointProj.is_ok_placed = false;

    }

    private void MarkOK(PointProjection pointProj)
    {
        Point et = pointProj.pointObject.GetComponent<Point>();
        if (et == null)
        {
            Debug.LogError("GameObj nie ma komponentu Point");
            return;
        }
        et.SetLabel(ReconstructionInfo.LABEL_3D_COLOR);
        pointProj.projLine.SetStyle(ReconstructionInfo.PROJECTION_LINE_COLOR, ReconstructionInfo.PROJECTION_LINE_WIDTH);
        pointProj.is_ok_placed = true;
    }

    private void CreateReferenceLines(GameObject dir, WallInfo wall, string label)
    {
        const float antiztrackhit = 0.001f;
        PointProjection pointProjection = verticesOnWalls[wall][label];
        for (int idx = 0; idx < pointProjection.refLine.Length; idx++)
        {
            GameObject edge1 = new GameObject($"RzutRzutujacy ({label}i{idx}) {wall.number}");
            edge1.transform.SetParent(dir.transform);
            LineSegment drawEdge1 = edge1.AddComponent<LineSegment>();
            drawEdge1.SetStyle(ReconstructionInfo.REFERENCE_LINE_COLOR, ReconstructionInfo.REFERENCE_LINE_WIDTH);

            GameObject edge2 = new GameObject($"RzutRzutujacy ({label}i{idx}) {wall.number * 10}");
            edge2.transform.SetParent(dir.transform);
            LineSegment drawEdge2 = edge2.AddComponent<LineSegment>();
            drawEdge2.SetStyle(ReconstructionInfo.REFERENCE_LINE_COLOR, ReconstructionInfo.REFERENCE_LINE_WIDTH);
            //SetTag LineSegment1, LineSegment2
            verticesOnWalls[wall][label].refLine[idx] = new Tuple<GameObject, GameObject>(edge1, edge2);
        }
        WallInfo[] wallsIdx = wc.RecGetRemainingWalls(wall);

        for (int idx = 0; idx < pointProjection.refLine.Length; idx++)
        {
            LineSegment refp1 = pointProjection.refLine[idx].Item1.GetComponent<LineSegment>();
            LineSegment refp2 = pointProjection.refLine[idx].Item2.GetComponent<LineSegment>();

            Vector3 pproj, cross, end_of_draw;
            Tuple<Vector3, Vector3> cro_eod;
            pproj = verticesOnWalls[wall][label].pointObject.transform.position + antiztrackhit * wall.GetNormal(); //rzut na scianie
            cro_eod = wc.FindCrossingPointEx(wall, pproj, wallsIdx[idx]);
            cross = cro_eod.Item1;
            end_of_draw = cro_eod.Item2;
            refp1.SetCoordinates(pproj, cross);
            refp2.SetCoordinates(end_of_draw, cross);
            refp1.SetEnable(showReferenceLines);
            refp2.SetEnable(showReferenceLines);
        }
    }
}
