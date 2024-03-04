using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRotator : MonoBehaviour {

	// Use this for initialization
	/// <summary>
	/// Szybkość obrotu
	/// </summary>
	[SerializeField] public float rotationSpeed = 3f;
	/// <summary>
	/// Zwykły materiał
	/// </summary>
	[SerializeField] public Material baseMaterial;
	/// <summary>
	/// Materiał, kiedy istnieje możliwość obrotu
	/// </summary>
	[SerializeField] public Material hoverMaterial;
	/// <summary>
	/// Obiekt gracza
	/// </summary>
	[SerializeField] public GameObject player;
	/// <summary>
	/// Odległość obiektu od gracza umożliwiająca obrót
	/// </summary>
	[SerializeField] public float hoverDistance = 15f;
	private bool mouseOnObject = false;

	/// <summary>
	/// Kamera potrzebna do wyliczania obrotu
	/// </summary>
	public Camera cam;

	void Update()
    {
		if (Vector3.Distance(gameObject.transform.position, player.transform.position) > hoverDistance)
		{
			gameObject.GetComponent<MeshRenderer>().material = baseMaterial;
		}
		if (Vector3.Distance(gameObject.transform.position, player.transform.position) <= hoverDistance && mouseOnObject)
        {
			gameObject.GetComponent<MeshRenderer>().material = hoverMaterial;
		}
	}
	void OnMouseDrag()
	{

		if (Vector3.Distance(gameObject.transform.position, player.transform.position) <= hoverDistance)
		{
			gameObject.GetComponent<MeshRenderer>().material = hoverMaterial;
			float rotationX = Input.GetAxis("Mouse X") * rotationSpeed;
			float rotationY = Input.GetAxis("Mouse Y") * rotationSpeed;

			Vector3 right = Vector3.Cross(cam.transform.up, transform.position - cam.transform.position);
			Vector3 up = Vector3.Cross(transform.position - cam.transform.position, right);
			transform.rotation = Quaternion.AngleAxis(-rotationX, up) * transform.rotation;
			transform.rotation = Quaternion.AngleAxis(rotationY, right) * transform.rotation;

		}

		

	}

	void OnMouseEnter()
    {
		if (Vector3.Distance(gameObject.transform.position,player.transform.position) <= hoverDistance)
        {
			gameObject.GetComponent<MeshRenderer>().material = hoverMaterial;
			
		}
		mouseOnObject = true;

	}

	void OnMouseExit()
	{
		gameObject.GetComponent<MeshRenderer>().material = baseMaterial;
		mouseOnObject = false;
	}

}
