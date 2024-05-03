using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCreator : MonoBehaviour {

	public Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	public RaycastHit hit;
	public List<GameObject> points = new List<GameObject>();
	public GameObject newWall;
	public int wallsCounter = 0;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if (Physics.Raycast(ray, out hit, 100))
		{
			//Debug.Log(hit.transform.name);
			//Debug.Log("hit");
			Debug.DrawLine(ray.origin, hit.point, Color.red);
			
		}
		//Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
		if (Input.GetKeyDown("c"))
		{
			CreatePoint();
		}
	}

	void CreatePoint() 
	{
		GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		point.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		point.transform.position = hit.point;
		points.Add(point);
		Debug.Log(hit.point);
		if (points.Count == 2)
        {
			CreateWall(points[wallsCounter*2].transform.position, points[wallsCounter*2+1].transform.position);
			wallsCounter++;
        }
	}

	void CreateWall(Vector3 point1, Vector3 point2)
    {
		//GameObject gameObj = new GameObject();
		//LineRenderer lineRenderer = gameObj.AddComponent<LineRenderer>();
		//lineRenderer.SetWidth(0.05f, 0.05f);
		//lineRenderer.SetPosition(0, point1);
		//lineRenderer.SetPosition(1, point2);
		Vector3 direction = point2 - point1;
		float distance = direction.magnitude;

		newWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
		Collider coll = newWall.GetComponent<Collider>();
		coll.enabled = false;

		// Ustawienie rozmiaru prostopadłościanu
		newWall.transform.localScale = new Vector3(3.4f, 0.1f, distance); // Długość prostopadłościanu
																		  //rectPrism.transform.localScale += new Vector3(0, distance, 1); // Wysokość prostopadłościanu
																		  //rectPrism.transform.localScale += new Vector3(0, 0, distance); // Szerokość prostopadłościanu

		// Ustawienie orientacji prostopadłościanu zgodnie z kierunkiem wektora
		newWall.transform.rotation = Quaternion.LookRotation(direction);

		// Ustawienie pozycji prostopadłościanu na środek linii między punktami
		newWall.transform.position = point1 + (direction / 2) + new Vector3(-1.7f,0,0);
	}


}
