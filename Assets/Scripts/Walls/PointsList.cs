using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsList : MonoBehaviour {

	private static List<GameObject> buttonsList = new List<GameObject>();

	private List<string> tempList = new List<string>();

	public static List<PointINFO> infoList = new List<PointINFO>();
	private int lastInfoListLength = 0;


	// Use this for initialization
	void Start () {
		GameObject[] buttonObjects = GameObject.FindGameObjectsWithTag("PointButton");
		buttonsList.AddRange(buttonObjects);
		buttonsList.Sort((x, y) => x.name.CompareTo(y.name));

		UpdatePointsList();
	}
	
	// Update is called once per frame
	void Update () {
		if (infoList.Count != lastInfoListLength) {
			lastInfoListLength = infoList.Count;
			Debug.Log($"<color=yellow> -- POINTS LIST ({lastInfoListLength}) -- </color>");
			infoList.ForEach(entry => Debug.Log($"<color=yellow> {entry.ToString()} </color>"));

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

	private static void UpdatePointsList()
    {
		

		for (int i = 0; i < buttonsList.Count; i++)
		{
			Transform pointTextTransform = buttonsList[i].transform.Find("PointText");
			TMPro.TextMeshPro pointText = pointTextTransform.GetComponent<TMPro.TextMeshPro>();
			//pointText.text = tempList[i];
			if (infoList.Count > i)
			{
				pointText.text = infoList[i].ToString();
			}
			else
            {
				pointText.text = "EMPTY";
			}
		}
	}

	public static void PointListGoUp() {

		if (infoList.Count > buttonsList.Count)
		{
			PointINFO first = infoList[0];
			infoList.RemoveAt(0);
			infoList.Add(first);
			UpdatePointsList();
		}
	}


	public static void PointListGoDown()
	{
		
		if (infoList.Count > buttonsList.Count)
		{
			PointINFO last = infoList[infoList.Count - 1];
			infoList.RemoveAt(infoList.Count - 1);
			infoList.Insert(0, last);
			UpdatePointsList();
		}

	}

	public static void HideListAndLogs()
    {
		GameObject wallText = GameObject.Find("WallText");
		GameObject list = GameObject.Find("PointsList");

		if (wallText != null)
        {
			wallText.SetActive(false);
        }
		if (list != null)
		{
			list.SetActive(false);
		}
	}

	public static PointINFO RemovePointOnClick(GameObject clickedButton)
    {
		int index = buttonsList.IndexOf(clickedButton);
		Debug.Log("Clicked button number:" + index.ToString());
		if (index < infoList.Count)
        {
			PointINFO clickedPoint = infoList[index];
			infoList.RemoveAt(index);
			UpdatePointsList();
			return clickedPoint;
		}
        else
        {
			PointINFO notFoundPoint = new PointINFO(null,null,null,null); //troche dziwne, moze trzeba poprawic
			return notFoundPoint;
        }
		

    }

}
