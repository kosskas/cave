using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

public class SolidImporter : MonoBehaviour {

	private const string pathToFolderWithSolids = "./Assets/Figures3D";
	private const string solidFileExt = "*.wobj";
	
	private string[] solidFiles;

	// Use this for initialization
	void Start () {
		solidFiles = Directory.GetFiles(pathToFolderWithSolids, solidFileExt);

		LogStatus();
	}
	
	// Update is called once per frame
	void Update () {

	}

	private void LogStatus() {
		if (solidFiles.Length > 0)
		{
			StringBuilder infoString = new StringBuilder();
			infoString.Append(String.Format("\n | All solids:"));
			foreach (string solidFile in solidFiles)
			{
				infoString.Append(String.Format(" {0}", Path.GetFileName(solidFile)));
			}
			Debug.Log(infoString.ToString());
		}
		else
		{
			Debug.LogError("No solids found!");
		}
	}
}
