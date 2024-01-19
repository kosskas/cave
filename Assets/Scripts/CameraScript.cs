using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Klasa CameraScript służy do pozycjonowania kamery w zależności czy gracz obraca bryłą.
/// </summary>
public class CameraScript : MonoBehaviour {

	/// <summary>
	/// Kamera pierwszoosobowa
	/// </summary>
	[SerializeField] public GameObject cam1; //moving cam
	/// <summary>
	/// Statyczna kamera na mapie
	/// </summary>
	public GameObject cam2; //static cam
	/// <summary>
	/// Klasa oznaczająca Gracza
	/// </summary>
	[SerializeField] public PlayerController controller;
	// Use this for initialization
	void Start () {
		
	}
	/// <summary>
	/// Ustawia kamera pierwszoosobową na aktywną, statyczna kamera na nieaktywną, odblokowuje ruch gracza
	/// </summary>
	public void SetCam1()
    {
		cam1.SetActive(true);
		cam2.SetActive(false);
		controller.canMove = true;
    }
	/// <summary>
	/// Ustawia kamera pierwszoosobową na nieaktywną, statyczna kamera na aktywną, blokowuje ruch gracza
	/// </summary>
	public void SetCam2()
	{
		cam1.SetActive(false);
		cam2.SetActive(true);
		controller.canMove = false;
	}
}
