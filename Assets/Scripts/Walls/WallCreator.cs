using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Klasa odpowiadająca za dodawanie nowej ściany przechodzącej przez 2 punkty i prostopadłej do ściany, na której użytkownik wskazał te punkty
public class WallCreator : MonoBehaviour
{
    /// <summary>
    /// Szablon obiektu ściany
    /// </summary>
    [SerializeField] public GameObject wallPrefab;

    /// <summary>
    /// Określa współrzędne środka ciężkości bryły, które służą jako punkt odniesienia np. do symulacji rotacji bryły 3D
    /// </summary>
    public Vector3 midPoint = new Vector3(0.0f, 1.0f, 0.0f);
    private List<GameObject> points = new List<GameObject>();
	private WallController wallController;
	private WallInfo hitWallInfo = null;

	private const float POINT_SIZE = 0.05f;

    // Use this for initialization
    void Start () {
		GameObject wallsObject = GameObject.Find("Walls");
		wallController = wallsObject.GetComponent<WallController>();
	}
	

	/// <summary>
	/// Stworzy punkt na ścianie przez który będzie przechodzić nowa ściana
	/// </summary>
	/// <param name="hit">Metadane o kolizji</param>
	public void AddAnchorPoint(RaycastHit hit) 
	{
		WallInfo justHit = null;
		
		try
		{
			justHit = wallController.FindWallInfoByGameObject(hit.collider.gameObject) as WallInfo;
		}
		catch (System.NullReferenceException)
		{
			Debug.LogError("CANNOT place new Point because NULL has been hit");
			return;
		}

		if (justHit == null)
		{
			Debug.LogError("CANNOT place new Point because NO WALL has been hit");
			return;
		}

		GameObject point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		point.transform.localScale = new Vector3(POINT_SIZE, POINT_SIZE, POINT_SIZE);
		point.transform.position = hit.point;
		Debug.Log(hit.point);
		points.Add(point);

		if (points.Count == 1)
		{
			hitWallInfo = justHit;
		}

		if (points.Count == 2)
		{
			if (hitWallInfo != justHit)
			{
				Debug.LogError("CANNOT create a new Wall object because DIFFERENT WALLS has been hit.");
				ClearPoints();
				return;
			}

			wallController.CreateWall(points[0].transform.position, points[1].transform.position, hitWallInfo, wallPrefab);
			ClearPoints();
        }

		if (points.Count > 2)
		{
			Debug.LogError("TO MANY Points have been placed.");
			ClearPoints();
		}
	}

    public WallInfo WCrCreateWall(Vector3 point1, Vector3 point2, WallInfo parentWallInfo, string fixedName = null)
    {
        return wallController.CreateWall(point1, point2, parentWallInfo, wallPrefab, fixedName);
    }
    public void RestoreWall(string wallName, Vector3 point1, Vector3 point2, string parentWallName)
    {
        WallInfo parentWallInfo = wallController.GetWallByName(parentWallName);
        if (parentWallInfo == null)
        {
            Debug.LogError("Nie znaleziono rodzica potrzebnego do stworzenia sciany");
			return;
        }
        wallController.CreateWall(point1, point2, parentWallInfo, wallPrefab, wallName);
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
