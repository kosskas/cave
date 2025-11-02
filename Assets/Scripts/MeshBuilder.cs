using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Experimental.Items;
using Assets.Scripts.Experimental.Utils;
using UnityEngine.Experimental.UIElements;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

public class Edge3D
{
    public GameObject edgeObject; // Ma w sobie LineSegment jako obiekt wizualizacji w 3D
    public string firstPoint;
    public string secondPoint;
    /// <summary>
    /// Ile krawędzi jest na ścianach
    /// </summary>
    public int wallOrigins;
    public bool standalone = false;

    public Edge3D(GameObject edgeObject, string firstPoint, string secondPoint)
    {
        this.edgeObject = edgeObject;
        this.firstPoint = firstPoint;
        this.secondPoint = secondPoint;
        this.wallOrigins = 0;
    }

}
public class Vertice3D
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

/// <summary>
/// Klasa MeshBuilder zawiera informację o odtwarzanych punktach i krawędziach w 3D. Jej zadaniem jej wyświetlanie tych obiektów na scenie w sposób poprawny wraz z liniami z nimi związanymi.
/// </summary>
public class MeshBuilder : MonoBehaviour
{
    const float PTS_3D_EQ_MARGIN = 1e-4f;
    private class PointProjection
    {
        public GameObject pointObject;
        public LineSegment projLine;
        public bool is_ok_placed;
        public Vector3 wallNormal;

        public PointProjection(GameObject pointObject, LineSegment projLine, Vector3 wallNormal, bool is_ok_placed)
        {
            this.pointObject = pointObject;
            this.projLine = projLine;
            this.is_ok_placed = is_ok_placed;
            this.wallNormal = wallNormal;
        }
        public Vector3 GetIntersection(PointProjection proj2)
        {
            /* Komentarz
             * Zobacz FindLLIntersections
             * Dodatkowo trzeba sprawdzić czy płaszczyzny nie są prostopadłe
             * Rzut jest wtedy kiedy s = t, czyli najkrótszy odcinek jest punktem
             */
            Vector3 p1 = this.pointObject.transform.position;
            Vector3 p2 = proj2.pointObject.transform.position;
            Vector3 n1 = this.wallNormal;
            Vector3 n2 = proj2.wallNormal;
            if (Math.Abs(Vector3.Dot(n1, n2)) > 1e-5f)
            {
                Debug.LogWarning($"Płaszczyzny nie są prostopadłe: n1={n1}, n2={n2}, dot={Vector3.Dot(n1, n2)}");
                return Vector3.zero;
            }

            Tuple<Vector3, Vector3> result = DescriptiveMathLib.FindLLIntersections(p1, n1, p2, n2);
            if (result == null)
            {
                return Vector3.zero;
            }

            Vector3 point1 = result.Item1;
            Vector3 point2 = result.Item2;
            if (Vector3.SqrMagnitude(point1 - point2) > PTS_3D_EQ_MARGIN)
            {
                Debug.LogWarning($"Nierzut: point1={point1}, point2={point2}");
                return Vector3.zero;
            }

            return point1;
        }

        public void MarkError()
        {
            ExPoint et = this.pointObject.GetComponent<ExPoint>();
            if (et == null)
            {
                Debug.LogWarning("GameObj nie ma komponentu ExPoint, nie mozna oznaczyc etykiety");
                //return;
            }

            if (this.projLine == null)
            {
                Debug.LogWarning("GameObj nie ma komponentu pointProj, nie mozna oznaczyc linii");
                //return;
            }
            else
            {
                this.projLine.SetStyle(ReconstructionInfo.PROJECTION_LINE_ERROR_COLOR,
                    ReconstructionInfo.PROJECTION_LINE_WIDTH);
            }

            this.is_ok_placed = false;
        }

        public void MarkOK()
        {
            ExPoint et = this.pointObject.GetComponent<ExPoint>();
            if (et == null)
            {
                Debug.LogWarning("GameObj nie ma komponentu ExPoint, nie mozna oznaczyc etykiety");
                //return;
            }

            if (this.projLine == null)
            {
                Debug.LogWarning("GameObj nie ma komponentu pointProj, nie mozna oznaczyc linii");
                //return;
            }
            else
            {
                this.projLine.SetStyle(ReconstructionInfo.PROJECTION_LINE_COLOR,
                    ReconstructionInfo.PROJECTION_LINE_WIDTH);
            }

            this.is_ok_placed = true;
        }
    }
    private class EdgeProjection
    {
        public Line line; // Linia jako obiekt konstrukcji na rzutni
        public Vector3 wallNormal;

        public EdgeProjection(Line line, Vector3 wallNormal)
        {
            this.line = line;
            this.wallNormal = wallNormal;
        }

    }
    /// <summary>
    /// Opisuje zbiór rzutowanych wierzchołków na danej płaszczyźnie
    /// </summary>
    /// 
    Dictionary<WallInfo, Dictionary<string, PointProjection>> verticesOnWalls;
    /// <summary>
    /// Opisuje zbiór rzutowanych wierzchołków na danej płaszczyźnie
    /// </summary>
    /// 
    Dictionary<WallInfo, Dictionary<string, EdgeProjection>> edgesOnWalls;
    /// <summary>
    /// Zbiór wierzchołków rysowanych w trójwymiarze. Elementy nigdy nie są usuwane jedynie oznaczane jako usunięte lub wyłączone.
    /// </summary>
	Dictionary<string, Vertice3D> vertices3D = null;

    /// <summary>
    /// Zbiór krawędzi rysowanych w 3D. (klucz to suma etykiet)
    /// </summary>
    Dictionary<string, Edge3D> edges3D = null;

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

    bool blocked = false;
    bool showProjectionLines = true;

	// Update is called once per frame
	void Update()
	{        
        if (blocked)
            return;
        ShowPoints3D();
        ShowEdges3D();
        ShowProjectionLines();
    }
    /// <summary>
    /// Inicjalizuje klasę, tworzy katalogi do przechowywania punktów i krawędzi 3D
    /// </summary>
    public void Init(bool showProjectionLines)
    {
        reconstrVertDir = new GameObject("Reconstr. Verticies");
        reconstrVertDir.transform.SetParent(gameObject.transform);
        edges3DDir = new GameObject("Reconstr. edges3DDir");
        edges3DDir.transform.SetParent(gameObject.transform);
        referenceLinesDir = new GameObject("referenceLinesDir");
        verticesOnWalls = new Dictionary<WallInfo, Dictionary<string, PointProjection>>();
        edgesOnWalls = new Dictionary<WallInfo, Dictionary<string, EdgeProjection>>();
        vertices3D = new Dictionary<string, Vertice3D>();
        edges3D = new Dictionary<string, Edge3D>();
        blocked = false;
        this.showProjectionLines = showProjectionLines;

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
        edgesOnWalls = null;
        vertices3D = null;
        edges3D = null;
    }

    /// <summary>
    /// Dodaje rzut punktu do listy punktów odtwarzanego obiektu 3D. Jeśli jest wystarczająco informacji to próbuje stworzyć go w 3D. 
    /// </summary>
    /// <param name="wall">Metadane ściany, na której znajduje się rzut</param>
    /// <param name="label">Etykieta rzutu</param>
    /// <param name="pointObj">Metadana o rzucie: etykieta, połozenie(position) jest takie jakie ma rodzic</param>
	public void AddPointProjection(WallInfo wall, string label, GameObject pointObj)
	{
        if (!verticesOnWalls.ContainsKey(wall))
        {
			verticesOnWalls[wall] = new Dictionary<string, PointProjection>();
        }

        var projLine = pointObj.GetComponent<LineSegment>();
        if (projLine == null)
        {
            projLine = pointObj.AddComponent<LineSegment>();
        }
        PointProjection toAddProj = new PointProjection(pointObj, projLine, wall.GetNormal(), false);
        verticesOnWalls[wall][label] = toAddProj;
        //sprawdz czy istnieja już dwa
        List<PointProjection> currPts = GetCurrentPointProjections(label);
        ResolveAddProjection(currPts, toAddProj, label);
    }

    private void ResolveAddProjection(List<PointProjection> currPts, PointProjection toAddProj, string label)
    {
        if (currPts.Count == 1)
        {
            Debug.Log($"Pierwszy {label}");
            //nie bylo zadnych rzutow tego pktu
            //bedzie 1
            currPts[0].MarkOK();
        }
        else if (currPts.Count == 2)
        {
            Debug.Log($"Drugi {label}");
            //jest juz 1 bedzie 2
            //podejmij rekonstrukcje
            //sprawdz plawszczyzny
            bool result = Create3DPoint(label, currPts[0], currPts[1]);
            if (result == false)
            {
                //podswietl dodawany na czerwono
                currPts[0].MarkError();
                currPts[1].MarkError();
            }
            else
            {
                currPts[0].MarkOK();
                currPts[1].MarkOK();
            }
        }
        else
        {
            Debug.Log("Trzeci lub wiecej");
            Vector3 pt3d = Get3DProj(label);
            var pairs = GetProjectionPairs(label);

            if (pt3d != Vector3.zero)
            {
                //sa juz 2
                //sprawdz sa dobrze postawione w stosunku do tego co jest    
                foreach(var pair in pairs)
                {
                    if (Vector3.SqrMagnitude(pair.Item1.GetIntersection(pair.Item2) - pt3d) < PTS_3D_EQ_MARGIN)
                    {
                        pair.Item1.MarkOK();
                        pair.Item2.MarkOK();
                        //ok ale pozjniej jako nie?
                    }
                    else
                    {
                        if(!pair.Item1.is_ok_placed)
                            pair.Item1.MarkError();
                        if (!pair.Item2.is_ok_placed)
                            pair.Item2.MarkError();
                    }
                }

            }
            else
            {
                //nie bylo wczesnej pktu, moze byc psrawdz
                Debug.Log("nie ma pktu 3d");
                foreach (var pair in pairs)
                {
                    bool result = Create3DPoint(label, pair.Item1, pair.Item2);
                    if (result == true)
                    {
                        break;
                    }
                }
                pt3d = Get3DProj(label);

                foreach (var pair in pairs)
                {
                    if ((Vector3.SqrMagnitude(pair.Item1.GetIntersection(pair.Item2) - pt3d) < PTS_3D_EQ_MARGIN) && pt3d != Vector3.zero)
                    {
                        pair.Item1.MarkOK();
                        pair.Item2.MarkOK();
                    }
                    else
                    {
                        if (!pair.Item1.is_ok_placed)
                            pair.Item1.MarkError();
                        if (!pair.Item2.is_ok_placed)
                            pair.Item2.MarkError();
                    }
                }
            }
        }
    }
    private Vector3 Get3DProj(string label)
    {
        if (vertices3D.ContainsKey(label) && !(vertices3D[label].deleted || vertices3D[label].disabled))
            return vertices3D[label].gameObject.transform.position;
        return Vector3.zero;
    }
    /// <summary>
    /// Usuwa rzut punktu z listy punktów odtwarzanego obiektu 3D. Jeśli istnieje pkt w 3D to następuje ponownw obliczenie jego pozycji lub usunięcie
    /// </summary>
    /// <param name="wall">Metadane ściany, na której znajduje się rzut</param>
    /// <param name="label">Etykieta</param>
    public void RemovePointProjection(WallInfo wall, string label)
    {
        if (verticesOnWalls == null)
            return;
        if (!verticesOnWalls.ContainsKey(wall))
            return;
        if (!verticesOnWalls[wall].ContainsKey(label))
            return;

        PointProjection pointToRemove = verticesOnWalls[wall][label];

        if (pointToRemove == null)
            return;
        
        int count = GetCurrentPointProjections(label).Count;
        List<PointProjection> currPts = GetCurrentPointProjections(label);
        verticesOnWalls[wall].Remove(label);
        if (count == 2) //sa dwa, usun 3d
        {
            if (vertices3D.ContainsKey(label))
            {
                vertices3D[label].deleted = true;
                FacesGenerator.RemoveFacesFromPoint(label);
            }
            currPts[0].MarkOK();
            currPts[1].MarkOK();
        }
        else if (count > 2) //sa trzy lub wiecej
        {
            if (vertices3D.ContainsKey(label)) //sa 3 lub wiecej i jest obiekt 3d
            {
                vertices3D[label].deleted = true;
            }
            //moga byc 3 i zle polozone
            //Rekonstruuj
            var pairs = GetProjectionPairs(label);

            foreach (var pair in pairs)
            {
                bool result = Create3DPoint(label, pair.Item1, pair.Item2);
                if (result == true)
                {
                    FacesGenerator.RemoveFacesFromPoint(label);
                    break;
                }
            }
            Vector3 pt3d = Get3DProj(label);

            foreach (var pair in pairs)
            {
                if ((Vector3.SqrMagnitude(pair.Item1.GetIntersection(pair.Item2) - pt3d) < PTS_3D_EQ_MARGIN) && pt3d != Vector3.zero)
                {
                    pair.Item1.MarkOK();
                    pair.Item2.MarkOK();
                }
                else
                {
                    pair.Item1.MarkError();
                    pair.Item2.MarkError();
                }
            }
        }
        Debug.Log($"Point removed: wall[{wall.number}] label[{label}] ");
	}

    public void RemoveProjectionLine(GameObject pointObj)
    {
        var projLine = pointObj.GetComponent<LineSegment>();
        if (projLine != null)
        {
            Destroy(projLine);
        }
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
    /// Tworzy krawędź w 3D i jeśli jest odpowiednia ilość informacji wyświetla ją
    /// </summary>
    /// <param name="wall">Rzutnia</param>
    /// <param name="label">Etykieta linii</param>
    public void AddEdgeProjectionStandalone(WallInfo wall, string label, Line line)
    {
        Debug.Log($"Lina {line} o et {label}; ");
        if (!edgesOnWalls.ContainsKey(wall))
        {
            edgesOnWalls[wall] = new Dictionary<string, EdgeProjection>();
        }
        if (edgesOnWalls[wall].ContainsKey(label))
        {
            Debug.Log($"Juz jest taka linia na scianie {label}");
            return;
        }
        EdgeProjection toAddProj = new EdgeProjection(line, wall.GetNormal());
        edgesOnWalls[wall][label] = toAddProj;
        //sprawdz czy istnieja już dwa
        List<EdgeProjection> currEdges = GetCurrentEdgeProjections(label);
        ResolveAddEdgeProjection(currEdges, label);
    }
    private void ResolveAddEdgeProjection(List<EdgeProjection> currEdges, string label)
    {
        if (currEdges.Count == 1)
        {
            Debug.Log($"Pierwszy raz linia {label}");
        }
        else if (currEdges.Count > 1)
        {
            Debug.Log($"Drugi raz linia {label}");
            //tylko dwa pierwsze wpisy bierzemy pod uwage
            EdgeProjection e1 = currEdges[0];
            EdgeProjection e2 = currEdges[1];

            string labelA = label + "_pointA";
            string labelB = label + "_pointB";

            Vector3 t1a = DescriptiveMathLib.FindLinePlaneIntersections(e1.line.StartPosition, e1.wallNormal, e2.line.StartPosition, e2.line.StartPosition - e2.line.EndPosition, e2.wallNormal);
            Vector3 t1b = DescriptiveMathLib.FindLinePlaneIntersections(e1.line.EndPosition, e1.wallNormal, e2.line.StartPosition, e2.line.StartPosition - e2.line.EndPosition, e2.wallNormal);

            Vector3 t2a = DescriptiveMathLib.FindLinePlaneIntersections(e2.line.StartPosition, e2.wallNormal, e1.line.StartPosition, e1.line.StartPosition - e1.line.EndPosition, e1.wallNormal);
            Vector3 t2b = DescriptiveMathLib.FindLinePlaneIntersections(e2.line.EndPosition, e2.wallNormal, e1.line.StartPosition, e1.line.StartPosition - e1.line.EndPosition, e1.wallNormal);

            /// find least common part

            CreateEntryForPoint(labelA, t1a);
            CreateEntryForPoint(labelB, t1b);
            vertices3D[labelA].disabled = true;
            vertices3D[labelB].disabled = true;

            GameObject edgeObj = new GameObject("Edge3D_standalone" + labelA + labelB);
            edgeObj.transform.SetParent(edges3DDir.transform);

            LineSegment edge = edgeObj.AddComponent<LineSegment>();
            edge.SetStyle(ReconstructionInfo.EDGE_3D_COLOR, ReconstructionInfo.EDGE_3D_LINE_WIDTH); //moze inne wartosci
            //edge.SetLabel(label, ReconstructionInfo.EDGE_3D_FONT_SIZE, ReconstructionInfo.EDGE_3D_COLOR); nie dziala
            edge.SetCoordinates(
                vertices3D[labelA].gameObject.transform.position,
                vertices3D[labelB].gameObject.transform.position
            );
            edges3D[label] = new Edge3D(edgeObj, labelA, labelB);
            edges3D[label].standalone = true;
        }
    }
    /// <summary>
    /// Usuwa informację o krawędzi w 3D. Jeżeli krawędź istnieje na wielu ścianach to krawędź w 3D zostanie usunięta jeśli nie będzie jej na żadnej ścianie.
    /// </summary>
    /// <param name="labelA">Etykieta punktu A</param>
    /// <param name="labelB">Etykieta punktu B</param>
    public void RemoveEdgeProjection(string labelA, string labelB)
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

        if (edges3D[key].wallOrigins < 1)
        {
            GameObject todel = edge.edgeObject;
            edges3D.Remove(key);
            Destroy(todel);
        }

    }
    /// <summary>
    /// Usuwa informację o krawędzi w 3D. Jeżeli krawędź istnieje na wielu ścianach to krawędź w 3D zostanie usunięta jeśli nie będzie jej na żadnej ścianie.
    /// </summary>
    /// <param name="wall">Rzutnia</param>
    /// <param name="label">Etykieta linii</param>
    /// <param name="line">Komponent rysowania</param>
    public void RemoveEdgeProjectionStandalone(WallInfo wall, string label, Line line)
    {
        if (edgesOnWalls == null)
            return;
        if (!edgesOnWalls.ContainsKey(wall))
            return;
        if (!edgesOnWalls[wall].ContainsKey(label))
            return;

        EdgeProjection edgeToRemove = edgesOnWalls[wall][label];

        if (edgeToRemove == null)
            return;

        int count = GetCurrentPointProjections(label).Count;
        List<PointProjection> currPts = GetCurrentPointProjections(label);
        edgesOnWalls[wall].Remove(label);

        //zawsze usun i sprawdz na nowo

        string labelA = label + "_pointA";
        string labelB = label + "_pointB";
        if (vertices3D.ContainsKey(labelA))
        {
            vertices3D[labelA].deleted = true;
        }
        if (vertices3D.ContainsKey(labelA))
        {
            vertices3D[labelB].deleted = true;
        }

        Edge3D edge = edges3D[label];
        GameObject todel = edge.edgeObject;
        edges3D.Remove(label);
        Destroy(todel);
        
        ResolveAddEdgeProjection(GetCurrentEdgeProjections(label), label);

    }
    /// <summary>
    /// Ustawia widoczność linii rzutujących
    /// </summary>
    /// <param name="rule">Zadada wyświetlania. True - wyświetlanie, False - brak</param>
    public void SetShowRulesProjectionLine(bool rule)
    {
        showProjectionLines = rule;
    }

    public Tuple<Vector3, Vector3> GetEdge3DCoords(string label)
    {
        if (!edges3D.ContainsKey(label))
        {
            return null;
        }
        LineSegment line = edges3D[label].edgeObject.GetComponent<LineSegment>();
        return line.GetCoordinates();
    }

    public Vector3? GetPoint3DCoords(string label)
    {
        Vertice3D value;
        if (!vertices3D.TryGetValue(label, out value))
            return null;

        var point = value.gameObject.transform.position;
        return point;
    }


    /// <summary>
    /// Sprawdza czy na scianie znajduje juz sie rzut punktu
    /// </summary>
    /// <param name="wall">Metadane ściany, na której znajduje się rzut</param>
    /// <param name="label">Etykieta</param>
    /// <returns>Prawda jeśli już taki rzut jest, Fałsz jeżeli nie ma</returns>
    public bool CheckIfAlreadyExist(WallInfo wall, string label)
    {
        return verticesOnWalls.ContainsKey(wall) && verticesOnWalls[wall].ContainsKey(label);       
    }

    /// <summary>
    /// Sprawdza czy na scianie znajduje juz sie rzut krawedzi
    /// </summary>
    /// <param name="wall">Metadane ściany, na której znajduje się rzut</param>
    /// <param name="label">Etykieta</param>
    /// <returns>Prawda jeśli już taki rzut jest, Fałsz jeżeli nie ma</returns>
    public bool CheckIfEdgeAlreadyExist(WallInfo wall, string label)
    {
        return edgesOnWalls.ContainsKey(wall) && edgesOnWalls[wall].ContainsKey(label);
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

    private bool Create3DPoint(string label, PointProjection proj1, PointProjection proj2)
    {
        Vector3 pkt3D = proj1.GetIntersection(proj2);
        if (pkt3D != Vector3.zero)
        {
            CreateEntryForPoint(label, pkt3D);
            return true;
        }
        return false;
    }

    private void CreateEntryForPoint(string label, Vector3 pos)
    {
        if (vertices3D.ContainsKey(label))
        {
            //byl usuniety?, przywroc go w nowej pozycji
            Point vertexObject = vertices3D[label].gameObject.GetComponent<Point>();
            vertexObject.SetCoordinates(pos);
            vertices3D[label].deleted = false;
            vertices3D[label].disabled = false;
        }
        else
        {
            //1 raz
            GameObject obj = new GameObject("Vertex3D " + label);
            obj.transform.SetParent(reconstrVertDir.transform);

            Point vertexObject = obj.AddComponent<Point>();
            vertexObject.SetStyle(ReconstructionInfo.POINT_3D_COLOR, ReconstructionInfo.POINT_3D_DIAMETER);
            vertexObject.SetCoordinates(pos);
            vertexObject.SetLabel(label, ReconstructionInfo.LABEL_3D_SIZE, ReconstructionInfo.LABEL_3D_COLOR);

            vertices3D[label] = new Vertice3D(obj);
        }
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
            bool isStandalone = edges3D[key].standalone;
            var v1 = vertices3D[edges3D[key].firstPoint];
            var v2 = vertices3D[edges3D[key].secondPoint];

            line.SetEnable(!(v1.deleted || v2.deleted || (!isStandalone && (v1.disabled || v2.disabled))));
            line.SetCoordinates(vertices3D[edges3D[key].firstPoint].gameObject.transform.position, vertices3D[edges3D[key].secondPoint].gameObject.transform.position);
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
    private List<EdgeProjection> GetCurrentEdgeProjections(string label)
    {
        List<EdgeProjection> edgeInfo = new List<EdgeProjection>();
        //sprawdz ile jest juz rzutow jednego pktu
        foreach (WallInfo wall in edgesOnWalls.Keys)
        {
            if (edgesOnWalls[wall].ContainsKey(label))
            {
                edgeInfo.Add(edgesOnWalls[wall][label]);
            }
        }
        return edgeInfo;
    }
    private List<Tuple<PointProjection, PointProjection>> GetProjectionPairs(string label)
    {
        List<PointProjection> pointsInfo = GetCurrentPointProjections(label);
        List<Tuple<PointProjection, PointProjection>> pairs = new List<Tuple<PointProjection, PointProjection>>();

        for (int i = 0; i < pointsInfo.Count; i++)
        {
            for (int j = i + 1; j < pointsInfo.Count; j++)
            {
                pairs.Add(Tuple.Create(pointsInfo[i], pointsInfo[j]));
            }
        }
        return pairs;
    }
}
