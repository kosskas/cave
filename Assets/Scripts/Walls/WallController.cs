using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Klasa WallController zarządza właściwościami ścian
/// </summary>
public class WallController : MonoBehaviour {
    private class WallComparer : IEqualityComparer<WallInfo>
    {
        public bool Equals(WallInfo x, WallInfo y)
        {
            return x.gameObject.Equals(y.gameObject);
        }
        public int GetHashCode(WallInfo obj)
        {
            return obj.gameObject.GetHashCode();
        }
    }

    private List<WallInfo> walls;
    private List<WallInfo> basicwalls;
    private List<WallInfo> playerAddedWalls;

    int wallcounter;
    // Use this for initialization
    void Start()
    {
        basicwalls = InitWalls();
        walls = new List<WallInfo>(basicwalls);
        playerAddedWalls = new List<WallInfo>();
    }
    private List<WallInfo> InitWalls()
    {
        GameObject[] wallsobject = GameObject.FindGameObjectsWithTag("Wall");
        Debug.Log(wallsobject.Length);
        List<WallInfo> ret = new List<WallInfo>();
        int idx = 0;
        foreach (GameObject wall in wallsobject)
        {
            ret.Add(new WallInfo(wall, idx, wall.name,
                true,   //show projection
                true,  //showLines
                true  //showReferenceLines
            ));
            idx++;
        }
        wallcounter = ret.Count;
        return ret;
    }
    /// <summary>
    /// Dodaje nową ścianę do kolekcji ścian
    /// </summary>
    /// <param name="wallobject">Obiekt ściany Unity na scenie</param>
    /// <param name="showProjection">Flaga dot. wyświetlania na ścianie rzutów</param>
    /// <param name="showLines">Flaga dot. wyświetlania linii rzutujących</param>
    /// <param name="showReferenceLines">Flaga dot. wyświetlania linii odnoszących</param>
    public void AddWall(GameObject wallobject, bool showProjection, bool showLines, bool showReferenceLines)
    {
        if(wallobject != null && walls != null)
        {
            WallInfo wall = new WallInfo(wallobject, wallcounter++, wallobject.name, showProjection, showLines, showReferenceLines);
            walls.Add(wall);
            playerAddedWalls.Add(wall);
            ResetProjection();
        }
    }

    /// <summary>
    /// Szuka informacji o ścianie na podstawie jej obiektu Unity
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
    public void SetWallInfo(GameObject wallObject,bool showProjection, bool showLines, bool showReferenceLines)
    {
        for(int i = 0; i < walls.Count; i++)
        {
            if (walls[i].gameObject == wallObject)
            {
                walls[i].SetFlags(showProjection, showLines, showReferenceLines);
            }
        }
    }
    /// <summary>
    /// Usuwa wszystkie nowo dodane ściany 
    /// </summary>
	public void SetBasicWalls()
	{
        List<WallInfo> tmpwalls = walls.Except(basicwalls, new WallComparer()).ToList();
        Debug.Log("Dlugosc tmp" + tmpwalls.Count);
        Debug.Log("Dlugosc paw" + playerAddedWalls.Count);
        foreach(WallInfo tmpwall in tmpwalls)
        {
            Debug.Log("Niszczenie "+tmpwall.name);
            Destroy(tmpwall.gameObject);
        }
        walls = new List<WallInfo>(basicwalls);
        wallcounter = walls.Count;
        playerAddedWalls = new List<WallInfo>();
        ResetProjection();
    }
    /// <summary>
    /// Usuwa ściany z odwróconą chronologią ich dodania
    /// </summary>
    public void PopBackWall()
    {
        if(playerAddedWalls !=  null && playerAddedWalls.Count > 0)
        {
            WallInfo lastWall = playerAddedWalls[playerAddedWalls.Count - 1];

            lastWall.showProjection = false;
            playerAddedWalls.RemoveAt(playerAddedWalls.Count - 1);
            walls.RemoveAt(walls.Count - 1);
            Destroy(lastWall.gameObject);
            lastWall.gameObject = null;
            wallcounter--;
            ResetProjection();
        } 
    }
    /// <summary>
    /// Pobiera listę ścian i ją zwraca
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
       // Debug.Log(groundWall.name);
        foreach (WallInfo wall in walls)
        {
            float dot = Vector3.Dot(groundWall.GetNormal(), wall.GetNormal());
            const float eps = 1e-6F;
            if (dot < 0f+eps && dot > 0f-eps)
            {
                tmp.Add(new Tuple<WallInfo, WallInfo>(groundWall, wall));
                //Debug.Log(groundWall.name + "   " + wall.name);
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
    /// <summary>
    /// Znajduje punkt przecięcia dwóch prostopadłych ścian na podstawie rzutu na jednej z nich oraz punkt końca rysowania linii
    /// </summary>
    /// <param name="basicwall">Ściana 1</param>
    /// <param name="proj">Rzut na jedenj ze ścian</param>
    /// <param name="wall2">Ściana 2</param>
    /// <returns>Para wektorów(przecięcie, koniec rysowania)</returns>
    public Tuple<Vector3,Vector3> FindCrossingPointEx(WallInfo basicwall, Vector3 proj, WallInfo wall2)
    {
        Vector3 Croos = Vector3.zero,Eod = Vector3.zero;
        const float LEN = 10f;

        Vector3 cross46 = new Vector3(1.63f, 0, -1.63f);
        Vector3 cross36 = new Vector3(0f, 0.07f, -1.63f);
        Vector3 cross34 = new Vector3(1.63f, 0.07f, 0);
        if (basicwall.name == "Wall3") ///wall6 (+x), wall4 (-z)
        {
            if (wall2.name == "Wall6")
            {
                Croos = cross36;
                Croos.x = proj.x;
            }
            if (wall2.name == "Wall4")
            {
                Croos = cross34;
                Croos.z = proj.z;
            }
        }
        else if (basicwall.name == "Wall4") ///wall6 po prawej(-z), wall3 na dole(-y)
        {
            if (wall2.name == "Wall6")
            {
                Croos = cross46;
                Croos.y = proj.y;
            }
            if (wall2.name == "Wall3")
            {
                Croos = cross34;
                Croos.z = proj.z;
            }
        }
        else if (basicwall.name == "Wall6") ///wall4 po lewej(+x), wall3 na dole(-y)
        {
            if (wall2.name == "Wall4")
            {
                Croos = cross46;
                Croos.y = proj.y;
            }
            if (wall2.name == "Wall3")
            {
                Croos = cross36;
                Croos.x = proj.x;             
            }
        }
        Eod = Croos + basicwall.GetNormal() * LEN;
        return new Tuple<Vector3, Vector3>(Croos, Eod);
    }
    private void ResetProjection()
    {
        ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
        if (op != null)
        {
            op.ResetProjections();
        }
    }
    /// <summary>
    /// Ustawia flagi ścian na wartości domyślne
    /// </summary>
    public void SetDefaultShowRules()
    {
        for (int i = 0; i < walls.Count; i++)
        {
            walls[i].SetFlags(
                true,   //show projection
                true,  //showLines
                true  //showReferenceLines
            );
        }
    }


    public int GetWallIndex(WallInfo wall)
    {
        if(wall.name == "Wall3")
            return 1;
        if (wall.name == "Wall4")
            return 2;
        if (wall.name == "Wall6")
            return 3;
        return 0;
    }

    /// <summary>
    /// Znajduje ściane na podstawie nazwy
    /// </summary>
    /// <param name="name">Nazwa ściany</param>
    /// <returns></returns>
    public WallInfo GetWallByName(string name)
    {
        foreach (WallInfo wall in walls)
        {
            if (wall.name == name)
            {
                return wall;
            }
        }
        return null;
    }
}
