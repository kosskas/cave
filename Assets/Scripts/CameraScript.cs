using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

	[SerializeField] GameObject cam1; //moving cam
	[SerializeField] GameObject cam2; //static cam
	[SerializeField] FPSController controller;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown("1"))
        {
			SetCam1();
			controller.canMove = true;
			
        }
		if (Input.GetKeyDown("2"))
		{
			SetCam2();
			controller.canMove = false;
		}
	}

	void SetCam1()
    {
		cam1.SetActive(true);
		cam2.SetActive(false);
    }

	void SetCam2()
	{
		cam1.SetActive(false);
		cam2.SetActive(true);
	}
}
