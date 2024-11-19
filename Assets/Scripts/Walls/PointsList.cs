using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml;
using TMPro;
using UnityEngine;

public class PointsList : MonoBehaviour {

	private static List<GameObject> buttonsList = new List<GameObject>();

	//private List<string> tempList = new List<string>();

    //public static List<PointINFO> infoList = new List<PointINFO>();
    public static List<KeyValuePair<string, Vector3>> points = new List<KeyValuePair<string, Vector3>>();

    private int lastInfoListLength = 0;
    private static MeshBuilder _mb;
    private bool isMbFound = false;

	public static GameObject ceilingWall;
	public static GameObject pointsList;
	public static GameObject wallText;
    public static GameObject listText;
    public static TextMeshPro listTextComponent;


    // Use this for initialization
    void Start () {
		ceilingWall = GameObject.Find("Wall5");
		pointsList = GameObject.Find("PointsList");
		wallText = GameObject.Find("WallText");
		listText = GameObject.Find("ListText");
		listTextComponent = listText.GetComponent<TextMeshPro>();
        GameObject mainObject = GameObject.Find("MainObject");

        _mb = mainObject.GetComponent<MeshBuilder>();
        //if (_mb != null)
        //{
        //    Debug.Log("Pomyślnie znaleziono i przypisano komponent MeshBuilder.");
        //}
        //else
        //{
        //    Debug.LogWarning("Komponent MeshBuilder nie został znaleziony na obiekcie MainObject.");
        //}

        GameObject[] buttonObjects = GameObject.FindGameObjectsWithTag("PointButton");
		buttonsList.AddRange(buttonObjects);
		buttonsList.Sort((x, y) => x.name.CompareTo(y.name));

		UpdatePointsList();
	}
	
	// Update is called once per frame
	void Update () {
        if (!isMbFound)
        {
            GameObject mainObject = GameObject.Find("MainObject");

            _mb = mainObject.GetComponent<MeshBuilder>();
			if (_mb != null)
			{
				Debug.Log("Pomyslnie znaleziono i przypisano komponent MeshBuilder.");
                isMbFound = true;
				UpdatePointsList();
            }
			
		}
        if (points.Count != lastInfoListLength) {
			lastInfoListLength = points.Count;
			Debug.Log($"<color=yellow> -- POINTS LIST ({lastInfoListLength}) -- </color>");
			points.ForEach(entry => Debug.Log($"<color=yellow> {entry.ToString()} </color>"));

			UpdatePointsList();
		}
        if (Input.GetKeyDown("9"))
        {
			PointListGoUp();
			//UpdatePointsList();
        }
		if (Input.GetKeyDown("0"))
		{
			//PointListGoDown();
			//UpdatePointsList();
			HideListAndLogs();
		}
	}

	public static void UpdatePointsList()
    {
        if (_mb == null)
            return;

        var pointsDictionary = _mb.GetPoints3D();

		if (pointsDictionary == null) 
            return;

        points.RemoveAll(p => !pointsDictionary.ContainsKey(p.Key));
        foreach (var point in pointsDictionary)
        {
            if (!points.Any(p => p.Key == point.Key))
            {
                points.Add(point);
            }
        }

        for (int i = 0; i < buttonsList.Count; i++)
		{
			Transform pointTextTransform = buttonsList[i].transform.Find("PointText");
			TMPro.TextMeshPro pointText = pointTextTransform.GetComponent<TMPro.TextMeshPro>();

			if (points.Count > i)
			{
				//pointText.text = infoList[i].ToString();
                pointText.text = $"{points[i].Key}: {points[i].Value}";
            }
			else
			{
				pointText.text = "EMPTY";
			}
		}
	}

	

	public static void PointListGoUp() {

		if (points.Count > buttonsList.Count)
		{
			KeyValuePair<string,Vector3> first = points[0];
			points.RemoveAt(0);
			points.Add(first);
			UpdatePointsList();
		}
	}


	public static void PointListGoDown()
	{
		
		if (points.Count > buttonsList.Count)
		{
            KeyValuePair<string, Vector3> last = points[points.Count - 1];
			points.RemoveAt(points.Count - 1);
			points.Insert(0, last);
			UpdatePointsList();
		}

	}

	public static void HideListAndLogs()
    {
		//GameObject wallText = GameObject.Find("WallText");
		//GameObject list = GameObject.Find("PointsList");

		if (wallText != null)
        {
			wallText.SetActive(false);
        }
		if (pointsList != null)
		{
			pointsList.SetActive(false);
		}
	}

	public static void ShowListAndLogs()
	{
		//GameObject wallText = GameObject.Find("WallText");
		//GameObject list = GameObject.Find("PointsList");

		if (wallText != null)
		{
			wallText.SetActive(true);
		}
		if (pointsList != null)
		{
			pointsList.SetActive(true);
		}
	}

    public static KeyValuePair<string, Vector3> AddPointToVerticesList(GameObject clickedButton)
    {
        int index = buttonsList.IndexOf(clickedButton);
        Debug.Log("Clicked button number:" + index.ToString());
        if (index < points.Count)
		{
            KeyValuePair<string, Vector3> clickedPoint = points[index];
            listTextComponent.text += $"{points[index].Key} ";
            //points.RemoveAt(index);
            //UpdatePointsList();
            return clickedPoint;
		}
		else
        {
            return new KeyValuePair<string, Vector3>("empty", new Vector3(0, 0, 0));
        }
	}

    //public static PointINFO RemovePointOnClick(GameObject clickedButton)
    //   {
    //	int index = buttonsList.IndexOf(clickedButton);
    //	Debug.Log("Clicked button number:" + index.ToString());
    //	if (index < infoList.Count)
    //       {
    //		PointINFO clickedPoint = infoList[index];
    //		infoList.RemoveAt(index);
    //		UpdatePointsList();
    //		return clickedPoint;
    //	}
    //       else
    //       {
    //		PointINFO notFoundPoint = new PointINFO(null,null,null,null); //troche dziwne, moze trzeba poprawic
    //		return notFoundPoint;
    //       }


    //   }

}
