using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using UnityEditorInternal;


public class MeshBuilder : MonoBehaviour {
    /*TODO
     [?] Przypadki przy dodawaniu
     [x] Sprawdz numeryczne porownianie
     [ ] Aktualizuj info jakie krawędzie między wierzchołkami	 
     [ ] Jak się określi krawędzie to zrób to samo w 3D
     [ ] Zrób grafowo-matematyczny dziki algorytm żeby wypełnić ściany
	 [ ] Kiedy jest juz to ten ray tylko do wierch ale nie w INF
	 [x] podswietl dodawany na czerwono jesli zla plaszcz, umiejscowienie trzeciego
	 [X] usun takze 3D kiedy są tylko 2
     [?] jezeli sa 3, usuwasz 1, sprawdz nowa pozycje 3d, usun czerwonosci jezeli byly  TO TEST xxx
     [ ] dynamicznie zmieniaj kszatl byly jezeli zmienia sie wirzcholki
     [ ] Refaktor kodu
     [ ] 
	*/
    private class PointProjection
	{
		public GameObject pointObject;
		public bool is_ok_placed;

		public PointProjection(GameObject pointObject, bool is_ok_placed)
		{
			this.pointObject = pointObject;
            this.is_ok_placed = is_ok_placed;
        }

    }
    private enum Status {
        OK,
        PLANE_ERR,
        OTHER_ERR
    }


    /// <summary>
    /// Opisuje zbiór rzutowanych wierzchołków na danej płaszczyźnie
    /// </summary>
    /// 
    Dictionary<WallInfo, Dictionary<string, PointProjection>> verticesOnWalls;

    /// <summary>
    /// Get compoment Point
    /// </summary>
	Dictionary<string, GameObject> vertices3D;

    Dictionary<string, List<string>> edges;
	GameObject reconstrVertDir;

    private const float POINT_DIAMETER = 0.015f;                        // 0.009f
    private const float LINE_WEIGHT = 0.008f;                           // 0.005f

    private const float VERTEX_LABEL_SIZE = 0.04f;
    private const float EDGE_LABEL_SIZE = 0.01f;


    private Color LABEL_COLOR = Color.white;
    private Color POINT_COLOR = Color.black;
    private Color LINE_COLOR = Color.black;

    const int MAXWALLSNUM = 3;
    // Use this for initialization
    void Start () {
        reconstrVertDir = new GameObject("Reconstr. Verticies");
        reconstrVertDir.transform.SetParent(gameObject.transform);

        verticesOnWalls = new Dictionary<WallInfo, Dictionary<string, PointProjection>>();

		edges = new Dictionary<string, List<string>>();
		vertices3D = new Dictionary<string, GameObject>();
    }

	// Update is called once per frame
	void Update()
	{
		/*
		 * Aktualizuj info jakie krawędzie między wierzchołkami
		 
		 * Jak się określi krawędzie to zrób to samo w 3D
		 * Zrób grafowo-matematyczny dziki algorytm żeby wypełnić ściany
		 */
		
		foreach (WallInfo wall in verticesOnWalls.Keys)
		{
			foreach (string label in verticesOnWalls[wall].Keys)
			{
				Vector3 vertex = verticesOnWalls[wall][label].pointObject.transform.position;
				Vector3 direction = wall.GetNormal();
                Ray ray = new Ray(vertex, direction);
                Debug.DrawRay(vertex, direction* 10f);
				///Kiedy jest juz to ten ray tylko do wierch ale nie w INF
				///opt: jezeli jest kilka wspolniliowych to linia tylko to najdluzszego?
            }
        }
    }
    /// <summary>
    /// Dodane rzut punktu z listy punktów odtwarzanego obiektu 3D
    /// </summary>
    /// <param name="wall">Metadane ściany, na której znajduje się rzut</param>
    /// <param name="label">Etykieta rzutu</param>
    /// <param name="pointObj">Metadana o rzucie: etykieta, połozenie(position) jest takie jakie ma rodzic</param>
	public void AddPointProjection(WallInfo wall, string label, GameObject pointObj)
	{
        if (!verticesOnWalls.ContainsKey(wall)) {
			verticesOnWalls[wall] = new Dictionary<string, PointProjection>();
        }
        PointProjection toAddProj = new PointProjection(pointObj, false);
        verticesOnWalls[wall][label] = toAddProj; //zawsze dodawaj, potem sprawdz czy ok

        //sprawdz czy istnieja już dwa
        List<PointProjection> currPts = GetCurrentPointProjections(label);

		if(currPts.Count == 1)
		{
            Debug.Log("Pierwszy");
            //nie bylo zadnych rzutow tego pktu
            //bedzie 1
            MarkOK(currPts[0]);
            Debug.Log($"Point added: wall[{wall.number}] label[{label}] N={wall.GetNormal()} ---- {pointObj.transform.position.x} {pointObj.transform.position.y} {pointObj.transform.position.z}");
        }
        else if (currPts.Count == 2)
        {
            Debug.Log("Drugi");
            //jest juz 1 bedzie 2
            //podejmij rekonstrukcje
            //sprawdz plawszczyzny
            Status result = Create3DPoint(label);
            if(result == Status.PLANE_ERR)
            {
                //podswietl dodawany na czerwono
                MarkError(currPts[0]);
                MarkError(currPts[1]);

            }
            else if(result == Status.OK)
            {
                MarkOK(currPts[0]);
                MarkOK(currPts[1]);
                Debug.Log($"Point added: wall[{wall.number}] label[{label}] N={wall.GetNormal()} ---- {pointObj.transform.position.x} {pointObj.transform.position.y} {pointObj.transform.position.z}");
            }
        }
        else
		{
            Debug.Log("Trzeci");
            //sa juz 2
            //sprawdz czy dobrze postawiony trzeci
            Vector3 proj1 = currPts[0].pointObject.transform.position;
            Vector3 proj2 = currPts[1].pointObject.transform.position;

			Vector3 third_proj = pointObj.transform.position;

            Vector3 test1 = CalcPosIn3D(proj1, proj2);
            Vector3 test2 = CalcPosIn3D(proj2, third_proj);
            Vector3 test3 = CalcPosIn3D(proj1, third_proj);
            Debug.Log($"Test1  ---- {test1.x} {test1.y} {test1.z}");
            Debug.Log($"Test2  ---- {test2.x} {test2.y} {test2.z}");
            Debug.Log($"Test3  ---- {test3.x} {test3.y} {test3.z}");

            if (test1 == test2 && test1 == test3 && test2 == test3)
            {
                Debug.Log("3 polozony OK");
                MarkOK(currPts[0]);
                MarkOK(currPts[1]);
                MarkOK(currPts[2]);
                Debug.Log($"Point added: wall[{wall.number}] label[{label}] N={wall.GetNormal()} ---- {pointObj.transform.position.x} {pointObj.transform.position.y} {pointObj.transform.position.z}");
            }
            else
            {
                //podswietl dodawany na czerwono
                Debug.Log("3 polozony ZLE");
                MarkError(toAddProj);
            }
        }
       
    }
    /// <summary>
    /// Usuwa rzut punktu z listy punktów odtwarzanego obiektu 3D
    /// </summary>
    /// <param name="wall">Metadane ściany, na której znajduje się rzut</param>
    /// <param name="label">Etykieta</param>
    public void RemovePointProjection(WallInfo wall, string label)
	{
        //rozszerzyć o label
        if (!verticesOnWalls.ContainsKey(wall))
            return;
        if (!verticesOnWalls[wall].ContainsKey(label))
            return;
        PointProjection pointToRemove = verticesOnWalls[wall][label];

        if (pointToRemove == null)
            return;

        
        int count = GetCurrentPointProjections(label).Count;
        verticesOnWalls[wall].Remove(label);
        if (count == 2) //sa dwa, usun 3d
        {
            if (vertices3D.ContainsKey(label))
            {
                GameObject todel3D = vertices3D[label];
                vertices3D.Remove(label);
                Destroy(todel3D);
            }
        }
        else if (count > 2) //sa trzy
        {
            if (vertices3D.ContainsKey(label))
            {
                GameObject todel3D = vertices3D[label];
                vertices3D.Remove(label);
                Destroy(todel3D);
                //z pozostalych dwoch, podejmij reko
                Status result = Create3DPoint(label);
                List<PointProjection> currPts = GetCurrentPointProjections(label);
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
        }   
        Debug.Log($"Point removed: wall[{wall.number}] label[{label}] ");
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
    private Status Create3DPoint(string label)
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
                GameObject obj = new GameObject("Vertex3D " + label);
                obj.transform.SetParent(reconstrVertDir.transform);

                Point vertexObject = obj.AddComponent<Point>();
                vertexObject.SetStyle(POINT_COLOR, POINT_DIAMETER);
                vertexObject.SetCoordinates(pkt3D);
                vertexObject.SetLabel(label, VERTEX_LABEL_SIZE, Color.white);

                vertices3D[label] = obj;
                return Status.OK;

            }
			else
			{
                return Status.PLANE_ERR;
            }
        }
		if (licz > 2)
		{
            //Problem
			//Sprawdz czy 3 jest dobrze polozony
            Debug.Log($"Jest więcej rzutow pktu niz 2");
            return Status.OTHER_ERR;

        }
        return Status.OTHER_ERR;

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
        et.SetLabel(Color.red);
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
        et.SetLabel(Color.white);
        pointProj.is_ok_placed = true;
    }
}
