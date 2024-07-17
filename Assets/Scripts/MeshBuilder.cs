using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MeshBuilder : MonoBehaviour {
	
	private class PointProjection
	{
		public GameObject pointObject;

		public PointProjection(GameObject pointObject)
		{
			this.pointObject = pointObject;
		}

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
		verticesOnWalls[wall][label] = pointObject;

		Debug.Log($"Point added: wall[{wall.number}] label[{label}] N={wall.GetNormal()} ---- {pointObject.transform.position}");

		//* Gdy 2 o tej samej etykiecie, na innych ścianach, sprawdź czy można postawić w 3D 
		//sprawdz czy juz są dwa - postaw 3 z automatu?
		CheckReconstruction(label);

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
	}

	private void CheckReconstruction(string label)
	{
		int licz = 0;

        GameObject[] pointsInfo = new GameObject[MAXWALLSNUM];

		foreach (WallInfo wall in verticesOnWalls.Keys)
		{
			if (verticesOnWalls[wall].ContainsKey(label))
			{
				pointsInfo[licz++] = verticesOnWalls[wall][label];
			}
		}
		if(licz == 2)
		{
			Vector3 rzut1 = pointsInfo[0].transform.position;
			Vector3 rzut2 = pointsInfo[1].transform.position;
			Vector3 pkt3D = ReconstructPoint(rzut1, rzut2);
			if (pkt3D != Vector3.zero)
			{
                GameObject obj = new GameObject("Vertex3D " + label);
                obj.transform.SetParent(reconstrVertDir.transform);

                Point vertexObject = obj.AddComponent<Point>();
                vertexObject.SetStyle(POINT_COLOR, POINT_DIAMETER);
                vertexObject.SetCoordinates(pkt3D);
                vertexObject.SetLabel(label, VERTEX_LABEL_SIZE, Color.white);

                vertices3D[label] = vertexObject;

            }
			else
			{
				//podswietl je na czerwono
                Debug.Log($"Rzuty pktu nie leza na jednej plaszczyznie");


            }


        }
		if (licz > 2)
		{
            //Problem
			//Sprawdz czy 3 jest dobrze polozony
            Debug.Log($"Jest więcej rzutow pktu niz 2");
        }

	}
    private Vector3 ReconstructPoint(Vector3 vec1, Vector3 vec2)
    {

        /*
		v1 = (A, b, c)
		v2 = (A, e, c)
		v3 = (A, b, f)
		ret = (A, e, f)
		*/
        Vector3 ret = Vector3.zero;
        const float eps = 0.0001f;
        bool cmp_x = Mathf.Abs(vec1[0] - vec2[0]) < eps;
        bool cmp_y = Mathf.Abs(vec1[1] - vec2[1]) < eps;
        bool cmp_z = Mathf.Abs(vec1[2] - vec2[2]) < eps;

		Debug.Log($"{cmp_x}{cmp_y}{cmp_z}");
		if(!(cmp_x || cmp_y || cmp_z))
		{
			return ret;
		}

		if (cmp_x)
		{
			ret.x = vec1.x;
			ret.y = Mathf.Max(vec1.y,vec2.y);
            ret.z = Mathf.Max(vec1.z, vec2.z);
        }
        if (cmp_y)
        {
            ret.x = Mathf.Min(vec1.x,vec2.x);
            ret.y = vec1.y;
            ret.z = Mathf.Max(vec1.z, vec2.z);
        }
        if (cmp_z)
        {
            ret.x = Mathf.Min(vec1.x, vec2.x);
            ret.y = Mathf.Max(vec1.y, vec2.y);
            ret.z = vec2.z;
        }
        ///Jeżeli rzuty nie znajdują sie na jednej płasz. to DO STH
        return ret;
    }
}
