using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WallCreator : MonoBehaviour {

	public Ray ray;
	public RaycastHit hit;
	public List<GameObject> points = new List<GameObject>();
	public GameObject newWall;
	public int wallsCounter;
    [SerializeField] public GameObject wallPrefab;
	private WallController wallController;

    // Use this for initialization
    void Start () {
		ray =  Camera.main.ScreenPointToRay(Input.mousePosition);
		wallsCounter = 0;
		GameObject wallsObject = GameObject.Find("Walls");
		wallController = wallsObject.GetComponent<WallController>();
	}
	
	// Update is called once per frame
	void Update () {
		ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		

		if (Physics.Raycast(ray, out hit, 100))
		{
			//Debug.Log(hit.transform.name);
			//Debug.Log("hit");
			//Debug.DrawLine(ray.origin, hit.point, Color.red);

		}

		if (Input.GetKeyDown("v"))
		{
			if (hit.collider != null)
            {
				if (hit.collider.tag == "Wall")
				{
					Debug.Log("hit: " + hit.collider.name);
					WallInfo info = wallController.FindWallInfoByGameObject(hit.collider.gameObject);
					if (info != null)
                    {
						if (info.showLines)
						{
							wallController.SetWallInfo(hit.collider.gameObject, false, false, false, false);
						}
						else
						{
							wallController.SetWallInfo(hit.collider.gameObject, true, true, true, true);
						}
					}
					
				}
			}
			
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
		if (points.Count % 2 == 0 && points.Count > 0)
        {
			CreateWall(points[wallsCounter*2].transform.position, points[wallsCounter*2+1].transform.position);
			wallsCounter++;
			foreach (GameObject obj in points)
			{
				// Usuwanie obiektu ze sceny
				if (obj != null)
				{
					Destroy(obj);
				}
			}
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

        newWall = Instantiate(wallPrefab);
        newWall.transform.parent = GameObject.Find("Walls").transform;
        newWall.tag = "Wall";
        // Ustawienie orientacji prostopadłościanu zgodnie z kierunkiem wektora
        Quaternion look = Quaternion.LookRotation(direction);
        float angle = 90f; ///liczyć dynamicznie;
        Quaternion rotation = Quaternion.Euler(look.eulerAngles.x, look.eulerAngles.y, look.eulerAngles.z - angle);
        newWall.transform.rotation = rotation;
        //Debug.Log(newWall.transform.rotation);
        Debug.Log("Player position:" + transform.position);
		newWall.transform.localScale = new Vector3(newWall.transform.localScale.x, newWall.transform.localScale.y * 3f, newWall.transform.localScale.y * 3f);

		// Ustawienie pozycji prostopadłościanu na środek linii między punktami
		Vector3 magicOffset = new Vector3(1.7f, 0, 0); ///liczyć dynamicznie;
        //newWall.transform.position = Vector3.zero; //localposition???
        newWall.transform.position = point1 + (direction / 2);

        //WallRotator rotator = newWall.AddComponent<WallRotator>();


        MeshRenderer meshRenderer = newWall.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
        Color color = meshRenderer.material.color;
        color.a = 0.5f;
        meshRenderer.material.color = color;

        BoxCollider boxCollider = newWall.GetComponent<BoxCollider>();
        boxCollider.isTrigger = false;

        
        
    }
}
/*
        float angle;
        if(point2.y < 1f && point1.y < 1f)
		{
			angle = -90f;
        }
		//else if (newWall.transform.right == Vector3.forward)
		//{
		//	angle = 0f;
		//}
        else
{
    angle = 90f;
}
*/