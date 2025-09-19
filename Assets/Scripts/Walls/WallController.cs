using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Scripts.Walls;
using Assets.Scripts;

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
    private Dictionary<WallInfo, List<GameObject>> wallRelatedObjects;

    int wallcounter;
    // Use this for initialization
    void Start()
    {
        basicwalls = InitWalls();
        walls = new List<WallInfo>(basicwalls);
        playerAddedWalls = new List<WallInfo>();
        wallRelatedObjects = new Dictionary<WallInfo, List<GameObject>>();
    }
    private List<WallInfo> InitWalls()
    {
        GameObject[] wallsobject = GameObject.FindGameObjectsWithTag("Wall");
        Debug.Log(wallsobject.Length);
        List<WallInfo> ret = new List<WallInfo>();
        int idx = 0;
        foreach (GameObject wall in wallsobject)
        {
            ret.Add(new WallInfo(wall, idx, wall.name, null,
                true,   //show projection
                true,  //showLines
                true,  //showReferenceLines
                false   //canDelete
            ));
            idx++;
        }
        wallcounter = ret.Count;
        return ret;
    }
    /// <summary>
    /// Dodaje nową ścianę do kolekcji ścian
    /// </summary>
    /// <param name="point1">Koordynaty 1</param>
    /// <param name="point2">Koordynaty 2</param>
    /// <param name="parentWallInfo">Ściana, od której powstała nowa ściana</param>
    /// <param name="wallPrefab">Szablon ściany</param>
    public WallInfo CreateWall(Vector3 point1, Vector3 point2, WallInfo parentWallInfo, GameObject wallPrefab, string fixedName = null)
    {
        Vector3 direction = point2 - point1;
        Vector3 parentNormal = parentWallInfo.GetNormal();
        Vector3 newWallNormal = Vector3.Cross(direction.normalized, parentNormal).normalized;
        Vector3 toCenterNormal = FindVectorFromPlaneTowardsSolid(point1, point2);

        float dot = Vector3.Dot(toCenterNormal, newWallNormal);
        newWallNormal *= Mathf.Sign(dot);
        Debug.DrawRay(point1, newWallNormal, Color.blue, 60.0f);

        GameObject newWall = Instantiate(wallPrefab);

        if (fixedName == null)
        {
            newWall.name = newWall.name + newWall.GetHashCode();
        }
        else
        {
            newWall.name = fixedName;
        }

        newWall.transform.parent = GameObject.Find("Walls").transform;
        newWall.tag = "Wall";

        newWall.transform.position = point1;

        Vector3 currentScale = newWall.transform.localScale;
        currentScale.x = 0.01f;
        currentScale.y = 10.0f;
        currentScale.z = 10.0f;
        newWall.transform.localScale = currentScale;

        newWall.transform.rotation = Quaternion.Euler(0, 0, 0);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, newWallNormal);
        newWall.transform.rotation = rotation;

        // newWall.transform.RotateAround(point1, Vector3.up, 10);
        // newWall.transform.RotateAround(point1, Vector3.right, 15);

        MeshRenderer meshRenderer = newWall.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
        Color color = meshRenderer.material.color;
        color.a = 0.5f;
        meshRenderer.material.color = color;

        BoxCollider boxCollider = newWall.GetComponent<BoxCollider>();
        boxCollider.isTrigger = false;

        WallInfo wall = new WallInfo(newWall, wallcounter++, newWall.name, parentWallInfo.name , true, true, true, true);
        walls.Add(wall);
        playerAddedWalls.Add(wall);
        ResetProjection();

        StateManager.Exp.StoreWall(newWall.name, point1, point2, parentWallInfo.name);
        return wall;
    }
    private Vector3 FindVectorFromPlaneTowardsSolid(Vector3 point1, Vector3 point2)
    {
        // from Object3D
        Vector3 mPoint = new Vector3(0.0f, 1.0f, 0.0f);

        // wektor delta ('point1' --> 'point2')
        Vector3 delta = (point2 - point1);

        // t - parameter from parametric equation of the plane
        float t = (delta.x * (mPoint.x - point1.x) + delta.y * (mPoint.y - point1.y) + delta.z * (mPoint.z - point1.z)) / (delta.x * delta.x + delta.y * delta.y + delta.z * delta.z);

        // wektor b leżący na wysokości h trójkąta opadającej z wierzchołka 'mPoint' na bok 'point1.point2', skierowany w stronę 'mPoint'
        //  niech punkt k będzie punktem przyłożenia wektora b, czyli punktem przecięcia wysokości h i prostej 'point1.point2'
        Vector3 kPoint = (point1 + t * delta);

        // określenie wektora b ('kPoint' --> 'mPoint')
        //  przyłożonego do punktu przecięcia wysokości h z prostą 'point1.point2'
        //  i skierowanego w stronę 'mPoint'
        //  i prostopadłego do prostej 'point1.point2'
        //  ALE NIEKONIECZNIE PROSTOPADŁEGO DO PŁASZCZYZNY
        Vector3 b = (mPoint - kPoint);

        //         m         //            m
        //         |         //            |
        //         |         //            |
        //         |         //            |
        // 1-------k-----2   // 1---2------k

        // zwrócenie znormalizowanego wektora b
        return b.normalized;
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

    public void LinkConstructionToWall(WallInfo wall, GameObject constructionObject)
    {
        if (!wallRelatedObjects.ContainsKey(wall))
        {
            wallRelatedObjects[wall] = new List<GameObject>();
        }
        wallRelatedObjects[wall].Add(constructionObject);
    }

    public void RemoveWall(WallInfo wall)
    {
        if(!wall.canDelete)
            return;

        if (wallRelatedObjects.ContainsKey(wall))
        {
            foreach (var constructionObject in wallRelatedObjects[wall])
            {
                if (constructionObject != null)
                {
                    Debug.Log("Destroying" + constructionObject.name);
                    Destroy(constructionObject);
                }
            }
        }
        Destroy(wall.gameObject);
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
            RemoveWall(lastWall);
            lastWall.gameObject = null;
            wallcounter--;
            ResetProjection();
        } 
    }

    public WallInfo GetLastAddedWall()
    {
        if (playerAddedWalls != null && playerAddedWalls.Count > 0)
        {
            return playerAddedWalls[playerAddedWalls.Count - 1];
        }

        return null;
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
