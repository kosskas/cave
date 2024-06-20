using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;

public class GridCreator : MonoBehaviour {

    /// <summary>
	/// Ścieżka względna dostępu do pliku konfiguracyjnego
	/// </summary>
	#if UNITY_EDITOR
		private const string pathToConfigFile = "./Assets/Config/coordinate_grid.txt";
	#else
		private const string pathToConfigFile = "./Config/coordinate_grid.txt";
	#endif

    private const float WALL_WEIGHT = 0.1f;
    private const float WALL_SIZE = 3.4f;

    private Dictionary<string, string> config = new Dictionary<string, string>();
    private GameObject grid = null;

	
    public GridCreator()
    {
        ParseConfigFile();

        //
        
        //

        grid = new GameObject("Grid");

        // floor



        int numOfLinesZ = int.Parse(config["number_of_lines"]);

        AddParallelLines(numOfLinesZ, 'Z', 'X'); 
        AddParallelLines(numOfLinesZ, 'X', 'Z');    

        AddParallelLines(numOfLinesZ, 'Z', 'Y');
        AddParallelLines(numOfLinesZ, 'Y', 'Z');    

        AddParallelLines(numOfLinesZ, 'X', 'Y');
        AddParallelLines(numOfLinesZ, 'Y', 'X');

        // wall right

        // wall left



    }

    private void AddParallelLines(int numOfLines, char parallelAxisName, char perpendicularAxisName)
    {
        float numOfSpaces = numOfLines-1;

        for (int ithLine = 0; ithLine < numOfLines; ithLine++)
        {


            //
            float lineWeight = float.Parse(config["line_weight"], CultureInfo.InvariantCulture);
            //

            Vector3 start = Vector3.zero;
            Vector3 stop = Vector3.zero;

            const float offsetFromWall = WALL_WEIGHT/2;
            float xMinPos = -(WALL_SIZE/2)+(offsetFromWall+lineWeight/2);
            float xMaxPos =  (WALL_SIZE/2)-(offsetFromWall+lineWeight/2);
            float zMinPos = -(WALL_SIZE/2)+(offsetFromWall+lineWeight/2);
            float zMaxPos =  (WALL_SIZE/2)-(offsetFromWall+lineWeight/2);
            float yMinPos =  0+(offsetFromWall+lineWeight/2);
            float yMaxPos =  WALL_SIZE-(offsetFromWall+lineWeight/2);

            if (parallelAxisName == 'Z')
            {
                if (perpendicularAxisName == 'X')
                {
                    float pos = xMinPos + (((float)ithLine/(float)numOfSpaces) * (xMaxPos-xMinPos));
                    start = new Vector3(pos, yMinPos, zMinPos);
                    stop = new Vector3(pos, yMinPos, zMaxPos);
                }
                else if (perpendicularAxisName == 'Y')
                {
                    float pos = yMinPos + (((float)ithLine/(float)numOfSpaces) * (yMaxPos-yMinPos));
                    start = new Vector3(xMinPos, pos, zMinPos);
                    stop = new Vector3(xMinPos, pos, zMaxPos);
                }
            }
            else if (parallelAxisName == 'X')
            {
                if (perpendicularAxisName == 'Z')
                {
                    float pos = zMinPos + (((float)ithLine/(float)numOfSpaces) * (zMaxPos-zMinPos));
                    start = new Vector3(xMinPos, yMinPos, pos);
                    stop = new Vector3(xMaxPos, yMinPos, pos);
                }
                else if (perpendicularAxisName == 'Y')
                {
                    float pos = yMinPos + (((float)ithLine/(float)numOfSpaces) * (yMaxPos-yMinPos));
                    start = new Vector3(xMinPos, pos, zMinPos);
                    stop = new Vector3(xMaxPos, pos, zMinPos);
                }
            }
            else if (parallelAxisName == 'Y')
            {
                if (perpendicularAxisName == 'X')
                {
                    float pos = xMinPos + (((float)ithLine/(float)numOfSpaces) * (xMaxPos-xMinPos));
                    start = new Vector3(pos, yMinPos, zMinPos);
                    stop = new Vector3(pos, yMaxPos, zMinPos);
                }
                else if (perpendicularAxisName == 'Z')
                {
                    float pos = zMinPos + (((float)ithLine/(float)numOfSpaces) * (zMaxPos-zMinPos));
                    start = new Vector3(xMinPos, yMinPos, pos);
                    stop = new Vector3(xMinPos, yMaxPos, pos);
                }
            }


            GameObject obj = new GameObject($"grid{perpendicularAxisName}{parallelAxisName}_{perpendicularAxisName}={ithLine}");
            obj.transform.SetParent(grid.transform);

            LineSegment line = obj.AddComponent<LineSegment>();
            if (ithLine == 0)
            {
                line.SetStyle(Color.black, lineWeight*2);
            }
            else
            {
                line.SetStyle(Color.grey, lineWeight);
            }
            line.SetCoordinates(start, stop); 
        }
    }


    private void ParseConfigFile()
    {
        try 
		{
            foreach (var line in File.ReadLines(pathToConfigFile))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    var key = parts[0].Trim();
                    var value = parts[1].Trim();
                    config[key] = value;
                }
                else
                {
                    Console.WriteLine($"[CAVE] Invalid line ({line}) in config file.");
                }
            }
		}
		catch (System.Exception) 
		{
			Debug.LogError($"[CAVE] It seems that config file {Application.dataPath}{pathToConfigFile} does not exist.");
		}  
    }
}
