using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;

public class MeshBuilder : MonoBehaviour {
	
	private class PointProjection
	{
		public GameObject pointObject;

		public PointProjection(GameObject pointObject)
		{
			this.pointObject = pointObject;
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

    Dictionary<WallInfo, Dictionary<string, GameObject>> verticesOnWalls;
	Dictionary<string, Point> vertices3D;

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

        verticesOnWalls = new Dictionary<WallInfo, Dictionary<string, GameObject>>();

		edges = new Dictionary<string, List<string>>();
		vertices3D = new Dictionary<string, Point>();
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
				Vector3 vertex = verticesOnWalls[wall][label].transform.position;
				Vector3 direction = wall.GetNormal();
                Ray ray = new Ray(vertex, direction);
                Debug.DrawRay(vertex, direction* 10f);
				///Kiedy jest juz to ten ray tylko do wierch ale nie w INF
				///opt: jezeli jest kilka wspolniliowych to linia tylko to najdluzszego?
            }
		}
		
	}

	public void AddPointProjection(WallInfo wall, string label, GameObject pointObject)
	{
		if (!verticesOnWalls.ContainsKey(wall)) {
			verticesOnWalls[wall] = new Dictionary<string, GameObject>();
        }
		//sprawdz czy istnieja już dwa
		List<GameObject> currPts = GetCurrentPointProjections(label);

		if(currPts.Count < 1)
		{
            //nie bylo zadnych rzutow tego pktu
            //bedzie 1
            verticesOnWalls[wall][label] = pointObject;
        }
        if (currPts.Count < 2)
        {
            //jest juz 1 bedzie 2
            //podejmij rekonstrukcje
            //sprawdz plawszczyzny
            Status result = ReconstructPoint(label);
        }
        else
		{
			//sa juz 2
			//sprawdz czy dobrze postawiony trzeci
            Vector3 rzut1 = currPts[0].transform.position;
            Vector3 rzut2 = currPts[1].transform.position;

			Vector3 rzut3 = pointObject.transform.position;

            Vector3 test1 = RecreatePoint(rzut1, rzut2);
            Vector3 test2 = RecreatePoint(rzut2, rzut3);
            Vector3 test3 = RecreatePoint(rzut1, rzut3);



        }
        Debug.Log($"Point added: wall[{wall.number}] label[{label}] N={wall.GetNormal()} ---- {pointObject.transform.position}");

        //* Gdy 2 o tej samej etykiecie, na innych ścianach, sprawdź czy można postawić w 3D 
        //sprawdz czy juz są dwa - postaw 3 z automatu?



    }

    public void RemovePointProjection(WallInfo wall, GameObject pointObject)
	{
		//rozszerzyć o label
		if (verticesOnWalls.ContainsKey(wall)) {
			var pointToRemove = verticesOnWalls[wall].FirstOrDefault(kvp => kvp.Value == pointObject);
			if (pointToRemove.Key != null) {
				verticesOnWalls[wall].Remove(pointToRemove.Key);
				Debug.Log($"Point removed: wall[{wall.number}] label[{pointToRemove.Key}] ");
			}
		}
		//usun takze 3D
        //jezeli sa 3, usuwasz 1, sprawdz nowa pozycje 3d
	}

	private Status ReconstructPoint(string label)
	{
		int licz = 0;

        List<GameObject> pointsInfo = GetCurrentPointProjections(label);
        //sprawdz ile jest juz rzutow jednego pktu
        licz = pointsInfo.Count;
		///ja nie wiem w jakiej kolejności to wypluwa rzuty. dopoki sa 2 to ok
		if(licz == 2)
		{
			Vector3 rzut1 = pointsInfo[0].transform.position;
			Vector3 rzut2 = pointsInfo[1].transform.position;

			Vector3 pkt3D = RecreatePoint(rzut1, rzut2);
			if (pkt3D != Vector3.zero)
			{
                GameObject obj = new GameObject("Vertex3D " + label);
                obj.transform.SetParent(reconstrVertDir.transform);

                Point vertexObject = obj.AddComponent<Point>();
                vertexObject.SetStyle(POINT_COLOR, POINT_DIAMETER);
                vertexObject.SetCoordinates(pkt3D);
                vertexObject.SetLabel(label, VERTEX_LABEL_SIZE, Color.white);

                vertices3D[label] = vertexObject;
                return Status.OK;

            }
			else
			{
				//podswietl je na czerwono
                Debug.Log($"Rzuty pktu nie leza na jednej plaszczyznie");
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
    private Vector3 RecreatePoint(Vector3 vec1, Vector3 vec2)
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

	private List<GameObject> GetCurrentPointProjections(string label)
	{
		List<GameObject> pointsInfo = new List<GameObject>();
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
}
