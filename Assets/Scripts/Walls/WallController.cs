using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour {
	
	private List<WallInfo> walls;

	// Use this for initialization
	void Start () {
		GameObject[] wallsobject = GameObject.FindGameObjectsWithTag("Wall");
        Debug.Log(wallsobject.Length);
		walls = new List<WallInfo>();
		int idx = 0;
		foreach(GameObject wall in wallsobject)
		{
			walls.Add(new WallInfo(wall, idx, wall.name,
                true,   //show projection
                false,  //showLines
                false,  //showReferenceLines
                false   //watchPerpen
            ));
			idx++;
		}
	}
	void Update ()
	{
		//sprawdzaj czy dodano ściane
	}
    /// <summary>
    /// Szuka informacji o ścianie na podstawie jej obiketu Unity
    /// </summary>
    /// <param name="wallObject">Obiekt ściany Unity</param>
    /// <returns>Informacje o ścianie</returns>
    public WallInfo FindWallInfoByGameObject(GameObject wallObject)
    {
        foreach (WallInfo wall in walls)
        {
            if(wall.gameObject == wallObject)
            {
                return wall;
            }
        }
        return null;
    }
    /// <summary>
    /// Ustawia parametry ściany według obiektu ściany Unity
    /// </summary>
    /// <param name="wallObject">Obiekt ściany Unity</param>
    /// <param name="showProjection">Flaga dot. wyświetlania na ścianie rzutów</param>
    /// <param name="showLines">Flaga dot. wyświetlania linii rzutujących</param>
    /// <param name="showReferenceLines">Flaga dot. wyświetlania linii odnoszących</param>
    /// <param name="watchPerpendicularity">Flaga dot. pilnowania prostopadłości rzutu na ścianie</param>
    public void SetWallInfo(GameObject wallObject,bool showProjection, bool showLines, bool showReferenceLines, bool watchPerpendicularity)
    {
        for(int i = 0; i < walls.Count; i++)
        {
            if (walls[i].gameObject == wallObject)
            {
                walls[i].showProjection = showProjection;
                walls[i].showLines = showLines;
                walls[i].showReferenceLines = showReferenceLines;
                walls[i].watchPerpendicularity = watchPerpendicularity;
            }
        }
    }
    /// <summary>
    /// Resetuje pozycje ścian do pozycji początkowych
    /// </summary>
	public void ResetWallsPos()
	{
		//Uwzgl. nowe sciany
		foreach(WallInfo wall in walls)
		{
			wall.SetPrevPos();
		}
	}
    /// <summary>
    /// PObiera listę ścian i ją zwraca
    /// </summary>
    /// <returns>Lista ścian jaka jest na scenie</returns>
    public List<WallInfo> GetWalls() {
        return walls;
    }
    /// <summary>
    /// Odczytuje liczbe wszystkich ścian
    /// </summary>
    /// <returns>Liczba ścian</returns>
    public int GetWallCount()
    {
        return walls.Count;
    }
    /// <summary>
    /// Odczytuje ze ścian ich wektory normalne
    /// </summary>
    /// <returns>Tablica wektorów normalnych ścian</returns>
	public Vector3[] GetWallNormals()
	{
		Vector3[] normals = new Vector3[walls.Count];
		for(int i = 0; i < walls.Count; i++)
		{
			normals[i] = walls[i].GetNormal();
		}
		return normals;
	}
    /// <summary>
    /// Znajduje ściany podstopadłe do ściany leżącej na podłodze
    /// </summary>
    /// <returns>Lista par ścian prostopadłych do siebie, null jeśli nie ma ściany leżązej na podłodze</returns>
    public List<Tuple<WallInfo, WallInfo>> FindPerpendicularWallsToGroundWall()
    {
        //Debug.Log(Vector3.up); jesłi tylko podłoga to znajdz == wall.transform.right
        List<Tuple<WallInfo, WallInfo>> tmp = new List<Tuple<WallInfo, WallInfo>>();
        //znajdz groundwall
        WallInfo groundWall = null;
        foreach (WallInfo wall in walls)
        {
            if (wall.GetNormal() == Vector3.up)
            {
                groundWall = wall;
                break;
            }
        }
        if (groundWall == null)
        {
            Debug.Log("Nie znaleziono sciany na podlodze");
            return null;
        }
        Debug.Log(groundWall.name);
        foreach (WallInfo wall in walls)
        {
            float dot = Vector3.Dot(groundWall.GetNormal(), wall.GetNormal());
            const float eps = 1e-6F;
            if (dot < eps && dot > -eps)
            {
                tmp.Add(new Tuple<WallInfo, WallInfo>(groundWall, wall));
                Debug.Log(groundWall.name + "   " + wall.name);
            }
        }
        return tmp;
    }

    /// <summary>
    /// Znajduje punkt przecięcia prostopadłych ścian dla dwóch rzutów i punktu w 3D
    /// </summary>
    /// <param name="vec1">Punkt w 3D</param>
    /// <param name="vec2">Rzut wierzchołka na 1 ścianie</param>
    /// <param name="vec3">Rzut wierzchołka na 2 ścianie</param>
    /// <returns>Punkt przecięcia ścian, wektor zerowy jeśli nie ma</returns>
    public Vector3 FindCrossingPoint(Vector3 vec1, Vector3 vec2, Vector3 vec3)
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
}
