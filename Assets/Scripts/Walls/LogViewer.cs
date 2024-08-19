using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogViewer : MonoBehaviour {

    [SerializeField] private TMPro.TextMeshPro textMesh;
    private string output = "";

    // Use this for initialization
    void Start () {
        GameObject textObject = GameObject.Find("WallText");
        textMesh = textObject.GetComponent<TMPro.TextMeshPro>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if(output.Length <= 200)
        {
            output += '\n' + logString;
        }
        else
        {
            output = logString;
        }
        
        textMesh.text = output;
    }


}
