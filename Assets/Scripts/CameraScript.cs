using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

	[SerializeField] GameObject cam1; //moving cam
	public GameObject cam2; //static cam
	[SerializeField] PlayerController controller;
	// Use this for initialization
	void Start () {
		
	}
	
	public void SetCam1()
    {
		cam1.SetActive(true);
		cam2.SetActive(false);
		controller.canMove = true;
    }

	public void SetCam2()
	{
		cam1.SetActive(false);
		cam2.SetActive(true);
		controller.canMove = false;
	}
}
