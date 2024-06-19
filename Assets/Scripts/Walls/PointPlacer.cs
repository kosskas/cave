using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPlacer : MonoBehaviour {

	// Use this for initialization
	private GameObject point;
	private const float POINT_SIZE = 0.05f;

	public void CreatePoint() 
    {
		point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Renderer pointRenderer = point.GetComponent<Renderer>();

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
            if (hit.collider.tag == "Wall")
            {
                point.transform.localScale = new Vector3(POINT_SIZE, POINT_SIZE, POINT_SIZE);
                point.transform.position = hit.point;
            }
			
		}
		
	}
}
