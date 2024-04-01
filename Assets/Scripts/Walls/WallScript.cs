using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour {
	
	WallInfo[] walls;

	// Use this for initialization
	void Start () {
		// GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");
		// int idx=0;
		// const float LengthOfTheRay = 10f;
		// foreach (GameObject wall in walls)
        // {
		// 	Transform tr = wall.transform;
		//    	Vector3 pos = tr.position;
		// 	Vector3 normal = tr.TransformVector(Vector3.right); //patrz w kierunku X, czewona strzałka UNITY
		// 	rayDirections[idx++] = (-1f) * normal*LengthOfTheRay; // minus bo w przeciwnym kierunku niż patzry ściana, czyli patrzymy na ścianę
        // }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
