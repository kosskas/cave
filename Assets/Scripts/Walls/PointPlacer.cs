using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPlacer : MonoBehaviour {

	// Use this for initialization
	private GameObject point;
    private Renderer pointRenderer;
	private const float POINT_SIZE = 0.05f;
    private const float POINT_DIAMETER = 0.015f;

    private const float VERTEX_LABEL_SIZE = 0.04f;

    private Color LABEL_COLOR = Color.white;
    private int labelText = 1;
    private Color POINT_COLOR = Color.black;

    private GameObject pointsDir;
    private WallController wc;
    private MeshBuilder mc;

    //private List<GameObject> activePoints = new List<GameObject>();


    void Start()
    {
        pointsDir = new GameObject("PointsDir");
        GameObject wallsObject = GameObject.Find("Walls");
        wc = wallsObject.GetComponent<WallController>();
        mc = (MeshBuilder)FindObjectOfType(typeof(MeshBuilder));
    }


    /// <summary>
    /// Metoda określa na której ścianie znajduje się punkt trafiony Raycastem
    /// </summary>
    /// <param name="wallNormal">Wektor normalny collidera punktu trafionego Raycastem</param>
    /// <param name="pointPosition">Współrzędne trafionego punktu</param>
    /// <returns>Obiekt opisujący ścianę, na której najprawdopodobniej znajduje się trafiony punkt</returns>
    private WallInfo EstimateWall(Vector3 wallNormal, Vector3 pointPosition)
    {
        List<WallInfo> walls = wc.GetWalls();
        WallInfo closestWall = null;
        float distanceToClosestWall = Mathf.Infinity;
        
        foreach (var wall in walls)
        {
            float distance = Vector3.Distance(wall.gameObject.transform.position, pointPosition);

            if (distance < distanceToClosestWall && wall.GetNormal() == wallNormal)
            {
                closestWall = wall;
                distanceToClosestWall = distance;
                //Debug.Log($"num: {closestWall.number}  Nwall: {wall.GetNormal()}  Nhit: {wallNormal} - {distanceToClosestWall}");
            }
        }

        return closestWall;
    }


	public void CreatePoint() 
    {
		point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointRenderer = point.GetComponent<Renderer>();

        // Tworzymy nowy materiał
        Material transparentMaterial = new Material(Shader.Find("Standard"));

        // Ustawiamy kolor i przezroczystość materiału
        Color color = new Color(1, 1, 1, 0.3f); // Kolor biały z 50% przezroczystością
        transparentMaterial.color = color;

        // Włączamy renderowanie przezroczystości
        transparentMaterial.SetFloat("_Mode", 3); // Ustawienie trybu renderowania na przeźroczystość
        transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transparentMaterial.SetInt("_ZWrite", 0);
        transparentMaterial.DisableKeyword("_ALPHATEST_ON");
        transparentMaterial.EnableKeyword("_ALPHABLEND_ON");
        transparentMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        transparentMaterial.renderQueue = 3000;

        // Przypisujemy materiał do sfery
        pointRenderer.material = transparentMaterial;
        point.layer = LayerMask.NameToLayer("Ignore Raycast");
    }
	
	public void MovePointPrototype(RaycastHit hit)
    {
		if (hit.collider != null)
        {
            if (hit.collider.tag != "Wall" && hit.collider.tag != "GridPoint")
            {
                return;
            }

            pointRenderer.material.color = (hit.collider.tag == "GridPoint") ? new Color(1, 0, 0, 1f) : new Color(1, 1, 1, 0.3f);
            
            point.transform.localScale = new Vector3(POINT_SIZE, POINT_SIZE, POINT_SIZE);
            point.transform.position = hit.point;
			
		}
		
	}

    public void OnClick(RaycastHit hit)
    {
        if (hit.collider != null)
        {
            if (hit.collider.tag == "GridPoint")
            {
                GameObject pointClicked = hit.collider.gameObject;
                Point point = pointClicked.GetComponent<Point>();

                WallInfo wall = this.EstimateWall(hit.normal, point.GetCoordinates());

                //if (activePoints.Contains(pointClicked))
                if (point.IsEnabled())
                {
                    //activePoints.Remove(pointClicked);
                    mc.RemovePointProjection(wall, $"{labelText}", pointClicked);
                    point.SetEnable(false);
                }
                else
                {
                    point.SetLabel($"{labelText}", VERTEX_LABEL_SIZE, LABEL_COLOR);
                    mc.AddPointProjection(wall, $"{labelText}", pointClicked);
                    labelText++;

                    //activePoints.Add(pointClicked);
                    point.SetEnable(true);
                }

                //Debug.Log($"Num of activePoints = {activePoints.Count}");
            }
        }
    }

    public void CreateLabel(RaycastHit hit, string label)
    {
        if (hit.collider != null)
        {
            if (hit.collider.tag == "GridPoint")
            {
                GameObject pointClicked = hit.collider.gameObject;
                WallInfo wall = this.EstimateWall(hit.normal, pointClicked.transform.position);

                //sprawdz czy na takiej scianie juz jest taki rzut
                //nakladanie sie pointów(sphere na siebie, zfighting?)

                int index = wc.GetWallIndex(wall);
                GameObject labelObj = new GameObject(label);
                labelObj.transform.parent = pointClicked.transform;

                Point point = labelObj.AddComponent<Point>();
                point.SetCoordinates(pointClicked.transform.position);
                point.SetStyle(Color.black, 0.01f); //niech gridcreator wystawia wartosci
                point.SetEnable(true);
                point.SetLabel($"{label+new string('\'', index)}", VERTEX_LABEL_SIZE, LABEL_COLOR);
                //Debug.Log($"TEST {label}");

                mc.AddPointProjection(wall, $"{label}", labelObj);
            }
        }
    }

    public void RemoveLabel(RaycastHit hit, string label)
    {
        if (hit.collider != null)
        {
            if (hit.collider.tag == "GridPoint")
            {
                GameObject pointClicked = hit.collider.gameObject;
                WallInfo wall = this.EstimateWall(hit.normal, pointClicked.transform.position);

                Transform labelObjTrabs = pointClicked.transform.Find(label);             
                if (labelObjTrabs == null)
                {
                    Debug.LogError($"Wezel nie ma takiego dziecka jak {label}");
                    return;
                }
                GameObject labelObj = labelObjTrabs.transform.gameObject;
                mc.RemovePointProjection(wall, $"{label}", labelObj);
                Destroy(labelObj);
            }
        }
    }
}
