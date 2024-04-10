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

}
