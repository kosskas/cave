using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour {
	
	List<WallInfo> walls;

	// Use this for initialization
	void Start () {
		GameObject[] wallsobject = GameObject.FindGameObjectsWithTag("Wall");
		walls = new List<WallInfo>();
		int idx = 0;
		foreach(GameObject wall in wallsobject)
		{
			walls.Add(new WallInfo(wall, idx, wall.name, true, false, false, false));
			idx++;
		}
	}
	void Update ()
	{
		//sprawdzaj czy dodano ściane
	}
    public List<WallInfo> GetWallsInfo()
	{
		return walls;
	}
	public void ResetWallsPos()
	{
		//Uwzgl. nowe sciany
		foreach(WallInfo wall in walls)
		{
			wall.SetPrevPos();
		}
	}

	public Vector3[] GetWallNormalsVec()
	{
		Vector3[] normals = new Vector3[walls.Count];
		for(int i = 0; i < walls.Count; i++)
		{
			normals[i] = walls[i].GetNormal();
		}
		return null;

	}

    /// <summary>
    /// Znajduje ściany podstopadłe do ściany leżącej na podłodze
    /// </summary>
    /// <param name="walls">Lista wszystkich ścian</param>
    /// <returns>Lista par ścian prostopadłych do siebie, null jeśli nie ma ściany leżązej na podłodze</returns>
    private List<Tuple<int, int>> FindPerpendicularWallsToGroundWall(GameObject[] walls)
    {
        //Debug.Log(Vector3.up); jesłi tylko podłoga to znajdz == wall.transform.right
        List<Tuple<int, int>> tmp = new List<Tuple<int, int>>();
        //znajdz groundwall
        GameObject groundWall = null;
        int i = 0;
        foreach (GameObject wall in walls)
        {
            if (wall.transform.right == Vector3.up)
            {
                groundWall = wall;
                break;
            }
            i++;
        }
        if (groundWall == null)
        {
            Debug.Log("Nie znaleziono sciany na podlodze");
            return null;
        }
        Debug.Log(groundWall.name);
        int j = 0;
        foreach (GameObject wall in walls)
        {
            float dot = Vector3.Dot(groundWall.transform.right, wall.transform.right);
            float eps = 1e-6F;
            if (dot < eps && dot > -eps)
            {
                tmp.Add(new Tuple<int, int>(i, j));
                Debug.Log(groundWall.name + "   " + wall.name);
            }
            j++;
        }
        // for(int i =0; i < walls.Length; i++){
        // 	for(int j =i; j < walls.Length; j++){
        // 		float dot = Vector3.Dot(walls[i].transform.right, walls[j].transform.right);
        // 		float eps = 1e-6F;
        // 		if(dot < eps && dot > -eps){
        // 			tmp.Add(new Tuple<int, int>(i, j));
        // 			Debug.Log(walls[i].name+ "   "+walls[j].name);
        // 		}
        // 	}
        // }
        return tmp;
    }

    /// <summary>
    /// Znajduje punkt przecięcia prostopadłych ścian dla dwóch rzutów i punktu w 3D
    /// </summary>
    /// <param name="vec1">Punkt w 3D</param>
    /// <param name="vec2">Rzut wierzchołka na 1 ścianie</param>
    /// <param name="vec3">Rzut wierzchołka na 2 ścianie</param>
    /// <returns>Punkt przecięcia ścian</returns>
    private Vector3 FindCrossingPoint(Vector3 vec1, Vector3 vec2, Vector3 vec3)
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
