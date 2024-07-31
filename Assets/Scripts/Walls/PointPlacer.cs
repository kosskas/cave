﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PointPlacer : MonoBehaviour {

	// Use this for initialization
	private GameObject cursor;
    private Renderer cursorRenderer;
    private GameObject cursorLabelObj;
    private Label cursorLabel;
	private const float CURSOR_SIZE = 0.05f;
    private Color CURSOR_COLOR = new Color(1, 1, 1, 0.3f);
    private Color CURSOR_COLOR_FOCUSED = new Color(1, 0, 0, 1f);

    private const float POINT_SIZE = 0.025f;
    private Color POINT_COLOR = Color.black;

    private const float LABEL_SIZE_PLACED = 0.04f;
    private const float LABEL_SIZE_PICKED = 0.06f;
    private const float LABEL_OFFSET_FROM_POINT = 0.03f;
    private Color LABEL_COLOR_PLACED = Color.white;
    private Color LABEL_COLOR_PICKED = Color.red;

    private char[] labelsColl = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
    private int labelsIdx = 0;

    private WallController wc;
    private MeshBuilder mc;
    

    void Start()
    {
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
    private WallInfo _EstimateWall(Vector3 wallNormal, Vector3 pointPosition)
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

    private void _LocateLabels(GameObject pointClicked, WallInfo wall)
    {
        Vector3 wallNormal = wall.GetNormal();
        int labelsNum = pointClicked.transform.childCount;
        float theta = 2.0f * Mathf.PI / (float)labelsNum;

        for (int i = 0; i < labelsNum; i++)
        {
            float alpha_i = (float)i * theta;
            Vector3 offset = Vector3.zero;

            if ((int)Mathf.Abs(wallNormal.x) == 1) {
                offset.y = 0.01f + LABEL_OFFSET_FROM_POINT * Mathf.Cos(alpha_i);
                offset.z = 0.02f + LABEL_OFFSET_FROM_POINT * Mathf.Sin(alpha_i);
            }
            else if((int)Mathf.Abs(wallNormal.y) == 1) {
                offset.x = 0.02f + LABEL_OFFSET_FROM_POINT * Mathf.Cos(alpha_i);
                offset.z = 0.02f + LABEL_OFFSET_FROM_POINT * Mathf.Sin(alpha_i);
            }
            else if((int)Mathf.Abs(wallNormal.z) == 1) {
                offset.x = 0.02f + LABEL_OFFSET_FROM_POINT * Mathf.Cos(alpha_i);
                offset.y = 0.01f + LABEL_OFFSET_FROM_POINT * Mathf.Sin(alpha_i);
            }

            pointClicked
                .transform
                .GetChild(i)
                .Find("Label")
                .GetComponent<Label>()
                .SetOffset(offset);
        }
    }


	public void CreateCursor() 
    {
		cursor = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        cursorRenderer = cursor.GetComponent<Renderer>();

        // Tworzymy nowy materiał
        Material transparentMaterial = new Material(Shader.Find("Standard"));

        // Ustawiamy kolor i przezroczystość materiału
        transparentMaterial.color = CURSOR_COLOR;

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
        cursorRenderer.material = transparentMaterial;
        cursor.layer = LayerMask.NameToLayer("Ignore Raycast");
        
        // Ustawiamy rozmiar
        cursor.transform.localScale = new Vector3(CURSOR_SIZE, CURSOR_SIZE, CURSOR_SIZE);

        // Dodajemy obiekt etykiety
        cursorLabelObj = new GameObject("CursorLabel");
        cursorLabelObj.transform.SetParent(cursor.transform);
        cursorLabelObj.transform.position = cursor.transform.position + new Vector3(0, 0.1f, 0);

        // Dodajemy etykietę
        cursorLabel = cursorLabelObj.AddComponent<Label>();
        cursorLabel.SetLabel($"{labelsColl[labelsIdx]}", LABEL_SIZE_PICKED, LABEL_COLOR_PICKED);		
    }
	
	public void MoveCursor(RaycastHit hit)
    {
		if (hit.collider == null || (hit.collider.tag != "Wall" && hit.collider.tag != "GridPoint")) {
            return;
        }

        cursor.transform.position = hit.point;

        if (hit.collider.tag == "GridPoint")
        {
            cursorRenderer.material.color = CURSOR_COLOR_FOCUSED;
            cursorLabel.SetLabel($"{labelsColl[labelsIdx]}");
            cursorLabel.SetEnable(true);
        }
        else
        {
            cursorRenderer.material.color = CURSOR_COLOR;
            cursorLabel.SetEnable(false);
        }
	}

    public void NextLabel()
    {
        labelsIdx = (labelsIdx+1) % labelsColl.Length;
    }

    public void PreviousLabel()
    {
        labelsIdx = ((labelsIdx-1) < 0) ? (labelsColl.Length-1) : (labelsIdx-1);
    }

    public void AddPoint(RaycastHit hit)
    {
        if (hit.collider == null || hit.collider.tag != "GridPoint") {
            return;
        }
        
        GameObject pointClicked = hit.collider.gameObject;
        if (pointClicked == null) {
            return;
        }
        
        WallInfo wall = this._EstimateWall(hit.normal, pointClicked.transform.position);
        if (wall == null) {
            return;
        }

        string labelText = $"{labelsColl[labelsIdx]}";
        if(mc.CheckIfAlreadyExist(wall, labelText)) {
            Debug.LogError($"Rzut {labelText} juz jest na tej scianie");
            return;
        }

        int index = wc.GetWallIndex(wall);

        GameObject labelObj = new GameObject(labelText);
        labelObj.transform.parent = pointClicked.transform;

        Point point = labelObj.AddComponent<Point>();
        point.SetCoordinates(pointClicked.transform.position);
        point.SetStyle(POINT_COLOR, POINT_SIZE);
        point.SetEnable(true);
        point.SetLabel($"{labelText + new string('\'', index)}", LABEL_SIZE_PLACED, LABEL_COLOR_PLACED);
        
        ///linia rzutująca
        LineSegment lineseg = labelObj.AddComponent<LineSegment>();
        lineseg.SetStyle(Color.blue, 0.002f);

        mc.AddPointProjection(wall, labelText, labelObj);

        _LocateLabels(pointClicked, wall);
    }

    public void RemovePoint(RaycastHit hit)
    {
        if (hit.collider == null || hit.collider.tag != "GridPoint") {
            return;
        }

        GameObject pointClicked = hit.collider.gameObject;
        if (pointClicked == null) {
            return;
        }

        WallInfo wall = this._EstimateWall(hit.normal, pointClicked.transform.position);
        if (wall == null) {
            return;
        }

        string labelText = $"{labelsColl[labelsIdx]}";
        Transform labelObjTrabs = pointClicked.transform.Find(labelText);             
        if (labelObjTrabs == null) {
            Debug.LogError($"Wezel nie ma takiego dziecka jak {labelText}");
            return;
        }

        mc.RemovePointProjection(wall, labelText);

        GameObject labelObj = labelObjTrabs.transform.gameObject;
        Destroy(labelObj);

        _LocateLabels(pointClicked, wall);
    }
}
