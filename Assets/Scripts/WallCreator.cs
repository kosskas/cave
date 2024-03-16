using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Klasa WallCreator tworzy ściany
/// </summary>
public class WallCreator : MonoBehaviour {

	// Use this for initialization
	private Camera playerCamera;

	void Start () {
		playerCamera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log(Input.mousePosition);
	}

	/// <summary>
	/// Tworzy sciane przed graczem
	/// </summary>
	public void CreateWall()
    {
		GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		wall.transform.localScale = new Vector3(0.1f, 10.0f, 10.0f);
		wall.GetComponent<Collider>().isTrigger = true; //dlaczego jest na odwrot?
		wall.transform.position = playerCamera.transform.position;
	}

}
