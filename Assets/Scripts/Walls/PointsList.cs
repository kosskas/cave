using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsList : MonoBehaviour {

	private List<GameObject> buttonsList = new List<GameObject>();

	private List<string> tempList = new List<string>();

	public static List<PointInfo> infoList = new List<PointInfo>();
	private int lastInfoListLength = 0;


	// Use this for initialization
	void Start () {
		GameObject[] buttonObjects = GameObject.FindGameObjectsWithTag("PointButton");

		buttonsList.AddRange(buttonObjects);

		tempList.Add("napis1");
		tempList.Add("napis2");
		tempList.Add("napis3");
		tempList.Add("napis4");
		for (int i = 0; i < buttonObjects.Length; i++)
        {
			Transform pointTextTransform = buttonObjects[i].transform.Find("PointText");
			TMPro.TextMeshPro pointText = pointTextTransform.GetComponent<TMPro.TextMeshPro>();
			pointText.text = tempList[i];
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (infoList.Count != lastInfoListLength) {
			lastInfoListLength = infoList.Count;
			Debug.Log($"<color=yellow> -- POINTS LIST ({lastInfoListLength}) -- </color>");
			infoList.ForEach(entry => Debug.Log($"<color=yellow> {entry.ToString()} </color>"));
		}
	}

	public void PointListGoUp() {

		//wywołać przy kliknięciu w górę
    }


	public void PointListGoDown()
	{
		//wywołać przy kliknięciu w dół

	}

}
