using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class WallCreator : MonoBehaviour {

	public Ray ray;
	public RaycastHit hit;
	public List<GameObject> points = new List<GameObject>();
	public GameObject newWall;
    [SerializeField] public GameObject wallPrefab;
	private WallController wallController;
	private WallInfo hitWallInfo = null;

	private const float POINT_SIZE = 0.05f;

	/// <summary>
	/// Określa współrzędne środka ciężkości bryły, które służą jako punkt odniesienia np. do symulacji rotacji bryły 3D
	/// </summary>
	public Vector3 midPoint = new Vector3(0.0f, 1.0f, 0.0f);

    // Use this for initialization
    void Start () {
		ray =  Camera.main.ScreenPointToRay(Input.mousePosition);
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
					//Debug.Log("hit: " + hit.collider.name);
					WallInfo info = wallController.FindWallInfoByGameObject(hit.collider.gameObject);
					if (info != null)
                    {
						if (info.showLines)
						{
							wallController.SetWallInfo(hit.collider.gameObject, false, false, false);
						}
						else
						{
							wallController.SetWallInfo(hit.collider.gameObject, true, true, true);
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
		point.transform.localScale = new Vector3(POINT_SIZE, POINT_SIZE, POINT_SIZE);
		point.transform.position = hit.point;
		Debug.Log(hit.point);
		points.Add(point);

		if (points.Count == 1)
		{
			hitWallInfo = wallController.FindWallInfoByGameObject(hit.collider.gameObject);
		}
		else if (points.Count == 2)
        {
			if (hitWallInfo == null)
			{
				Debug.LogError("CANNOT create new Wall object because hitWallInfo is NULL");
				ClearPoints();
				return;
			}

			if (hitWallInfo != wallController.FindWallInfoByGameObject(hit.collider.gameObject))
			{
				Debug.LogError("CANNOT create a new Wall object because the same wall has not been hit twice.");
				ClearPoints();
				return;
			}

			CreateWall(points[0].transform.position, points[1].transform.position, hitWallInfo);
			ClearPoints();
            wallController.AddWall(newWall, true, true, true);
        }
	}

	void ClearPoints()
	{
		foreach (GameObject obj in points)
		{
			if (obj != null)
			{
				Destroy(obj);	// Usuwanie obiektu ze sceny
			}
		}

		points.Clear();
	}

	void CreateWall(Vector3 point1, Vector3 point2, WallInfo parentWallInfo)
	{
		Vector3 direction = point2 - point1;
		Vector3 parentNormal = parentWallInfo.GetNormal();
		Vector3 newWallNormal = Vector3.Cross(direction.normalized, parentNormal).normalized;
		Vector3 toCenterNormal = FindVectorFromPlaneTowardsSolid(point1, point2);

		float dot = Vector3.Dot(toCenterNormal, newWallNormal);
		newWallNormal *= Mathf.Sign(dot);
		Debug.DrawRay(point1, newWallNormal, Color.blue, 60.0f);

		newWall = Instantiate(wallPrefab);
        newWall.transform.parent = GameObject.Find("Walls").transform;		
        newWall.tag = "Wall";

		newWall.transform.position = point1;

        Vector3 currentScale = newWall.transform.localScale;
        currentScale.x = 0.01f;
		currentScale.y = 10.0f;
		currentScale.z = 10.0f;
        newWall.transform.localScale = currentScale;

		newWall.transform.rotation = Quaternion.Euler(0, 0, 0);
        Quaternion rotation = Quaternion.FromToRotation(Vector3.right, newWallNormal);
        newWall.transform.rotation = rotation;

		// newWall.transform.RotateAround(point1, Vector3.up, 10);
		// newWall.transform.RotateAround(point1, Vector3.right, 15);

		MeshRenderer meshRenderer = newWall.GetComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Transparent/Diffuse"));
        Color color = meshRenderer.material.color;
        color.a = 0.5f;
        meshRenderer.material.color = color;

        BoxCollider boxCollider = newWall.GetComponent<BoxCollider>();
        boxCollider.isTrigger = false;        
    }

	private Vector3 FindVectorFromPlaneTowardsSolid(Vector3 point1, Vector3 point2)
	{
		// from Object3D
		Vector3 mPoint = new Vector3(0.0f, 1.0f, 0.0f);
		
		// wektor delta ('point1' --> 'point2')
		Vector3 delta = ( point2 - point1 );

		// t - parameter from parametric equation of the plane
		float t = ( delta.x*(mPoint.x-point1.x) + delta.y*(mPoint.y-point1.y) + delta.z*(mPoint.z-point1.z) )/( delta.x*delta.x + delta.y*delta.y + delta.z*delta.z );

		// wektor b leżący na wysokości h trójkąta opadającej z wierzchołka 'mPoint' na bok 'point1.point2', skierowany w stronę 'mPoint'
		//  niech punkt k będzie punktem przyłożenia wektora b, czyli punktem przecięcia wysokości h i prostej 'point1.point2'
		Vector3 kPoint = ( point1 + t*delta );

		// określenie wektora b ('kPoint' --> 'mPoint')
		//  przyłożonego do punktu przecięcia wysokości h z prostą 'point1.point2'
		//  i skierowanego w stronę 'mPoint'
		//  i prostopadłego do prostej 'point1.point2'
		//  ALE NIEKONIECZNIE PROSTOPADŁEGO DO PŁASZCZYZNY
		Vector3 b = ( mPoint - kPoint );

		//         m         //            m
		//         |         //            |
		//         |         //            |
		//         |         //            |
		// 1-------k-----2   // 1---2------k

		// zwrócenie znormalizowanego wektora b
		return b.normalized;
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
