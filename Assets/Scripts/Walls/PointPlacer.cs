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
    private Color POINT_COLOR = Color.black;

    private GameObject pointsDir;
    private WallController wc;
    private MeshBuilder mc;

    void Start()
    {
        pointsDir = new GameObject("PointsDir");
        GameObject wallsObject = GameObject.Find("Walls");
        wc = wallsObject.GetComponent<WallController>();
        mc = (MeshBuilder)FindObjectOfType(typeof(MeshBuilder));
    }

    private List<GameObject> activePoints = new List<GameObject>();

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

            // point.transform.localScale = new Vector3(POINT_SIZE, POINT_SIZE, POINT_SIZE);
            // point.transform.position = hit.point; ///Z-FIGHTING!!!

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

                Renderer pointRenderer = pointClicked.GetComponent<Renderer>();
                pointRenderer.enabled = !pointRenderer.enabled;

                if (pointRenderer.enabled)
                {
                    activePoints.Add(pointClicked);
                }
                else
                {
                    activePoints.Remove(pointClicked);
                }

                //Debug.Log($"{hit.collider.tag} = {hit.collider.gameObject.name}");
                Debug.Log($"Num of activePoints = {activePoints.Count}");
            }
        }
    }
    
    public void PlacePoint(RaycastHit hit)
    {
        const float antiztrack = 0.01f;
        GameObject placedPoint = null;
        string label = "PKT";
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Wall")
            {
                ///TODO sprawdz czy sciana wgl wyswietla grid
                ///TODO sprawdz czy na gridzie
                ///TODO nadaj Etykiete
                placedPoint = new GameObject("WallPoint");
                placedPoint.transform.parent = pointsDir.transform;
                Vector3 antiztrackhit = hit.point + antiztrack * hit.normal;

                Point vertexObject = placedPoint.AddComponent<Point>();
                vertexObject.SetStyle(POINT_COLOR, POINT_DIAMETER);
                vertexObject.SetCoordinates(antiztrackhit);
                vertexObject.SetLabel(label, VERTEX_LABEL_SIZE, LABEL_COLOR);


                ///TODO włóż do struktury pointy per ściana
                WallInfo wall = wc.FindWallInfoByGameObject(hit.collider.gameObject);
                mc.AddPointProjection(wall, label, placedPoint);
            }
        }

    }
}
