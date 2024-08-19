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
    private const float POINT_SIZE = 0.05f;

    private Dictionary<string, string> config = new Dictionary<string, string>();
    private GameObject grid = null;

	
    public void Init()
    {
        ParseConfigFile();


        grid = new GameObject("Grid");

        int numOfLinesX = int.Parse(config["number_of_lines"]);
        int numOfLinesY = int.Parse(config["number_of_lines"]);
        int numOfLinesZ = int.Parse(config["number_of_lines"]);

        AddParallelLines(numOfLinesX, 'Z', 'X'); 
        AddParallelLines(numOfLinesZ, 'X', 'Z');    

        AddParallelLines(numOfLinesY, 'Z', 'Y');
        AddParallelLines(numOfLinesZ, 'Y', 'Z');    

        AddParallelLines(numOfLinesX, 'Y', 'X');
        AddParallelLines(numOfLinesY, 'X', 'Y');
        
        
        AddCrossPoints(numOfLinesX, numOfLinesZ, "XZ");
        AddCrossPoints(numOfLinesY, numOfLinesZ, "YZ");
        AddCrossPoints(numOfLinesX, numOfLinesY, "XY");



    }

    private void AddParallelLines(int numOfLines, char parallelAxisName, char perpendicularAxisName)
    {
        float numOfSpaces = numOfLines-1;

        float lineWeight = float.Parse(config["line_weight"], CultureInfo.InvariantCulture);

        const float offsetFromWall = WALL_WEIGHT/2;
        float xMinPos = -(WALL_SIZE/2)+(offsetFromWall);
        float xMaxPos =  (WALL_SIZE/2)-(offsetFromWall);
        float zMinPos = -(WALL_SIZE/2)+(offsetFromWall);
        float zMaxPos =  (WALL_SIZE/2)-(offsetFromWall);
        float yMinPos =  0+(offsetFromWall);
        float yMaxPos =  WALL_SIZE-(offsetFromWall);

        float xRange = xMaxPos-xMinPos;
        float yRange = yMaxPos-yMinPos;
        float zRange = zMaxPos-zMinPos;

        for (int ithLine = 0; ithLine < numOfLines; ithLine++)
        {
            Vector3 start = Vector3.zero;
            Vector3 stop = Vector3.zero;
            //Vector3 colliderSize = Vector3.zero;          

            if (parallelAxisName == 'Z')
            {
                if (perpendicularAxisName == 'X')
                {
                    float pos = xMinPos + (((float)ithLine/(float)numOfSpaces) * xRange);
                    start = new Vector3(pos, yMinPos, zMinPos);
                    stop = new Vector3(pos, yMinPos, zMaxPos);
                }
                else if (perpendicularAxisName == 'Y')
                {
                    float pos = yMinPos + (((float)ithLine/(float)numOfSpaces) * yRange);
                    start = new Vector3(xMaxPos, pos, zMinPos);
                    stop = new Vector3(xMaxPos, pos, zMaxPos);
                }
                //colliderSize = new Vector3(colliderWeight, colliderWeight, zRange);
            }
            else if (parallelAxisName == 'X')
            {
                if (perpendicularAxisName == 'Z')
                {
                    float pos = zMinPos + (((float)ithLine/(float)numOfSpaces) * zRange);
                    start = new Vector3(xMinPos, yMinPos, pos);
                    stop = new Vector3(xMaxPos, yMinPos, pos);
                }
                else if (perpendicularAxisName == 'Y')
                {
                    float pos = yMinPos + (((float)ithLine/(float)numOfSpaces) * yRange);
                    start = new Vector3(xMinPos, pos, zMinPos);
                    stop = new Vector3(xMaxPos, pos, zMinPos);
                }
                //colliderSize = new Vector3(xRange, colliderWeight, colliderWeight);
            }
            else if (parallelAxisName == 'Y')
            {
                if (perpendicularAxisName == 'X')
                {
                    float pos = xMinPos + (((float)ithLine/(float)numOfSpaces) * xRange);
                    start = new Vector3(pos, yMinPos, zMinPos);
                    stop = new Vector3(pos, yMaxPos, zMinPos);
                }
                else if (perpendicularAxisName == 'Z')
                {
                    float pos = zMinPos + (((float)ithLine/(float)numOfSpaces) * zRange);
                    start = new Vector3(xMaxPos, yMinPos, pos);
                    stop = new Vector3(xMaxPos, yMaxPos, pos);
                }
                //colliderSize = new Vector3(colliderWeight, yRange, colliderWeight);
            }


            GameObject obj = new GameObject($"plane{perpendicularAxisName}{parallelAxisName}_{perpendicularAxisName}={ithLine}");
            obj.transform.SetParent(grid.transform);
            obj.tag = "GridLine";

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

    private void AddCrossPoints(int numOfLinesInAxisA, int numOfLinesInAxisB, string planeName)
    {
        float numOfSpacesInA = numOfLinesInAxisA-1;
        float numOfSpacesInB = numOfLinesInAxisB-1;

        float lineWeight = float.Parse(config["line_weight"], CultureInfo.InvariantCulture);
        float pointWidth = lineWeight * 5.0f;

        const float offsetFromWall = WALL_WEIGHT/2;
        float xMinPos = -(WALL_SIZE/2)+(offsetFromWall);
        float xMaxPos =  (WALL_SIZE/2)-(offsetFromWall);
        float zMinPos = -(WALL_SIZE/2)+(offsetFromWall);
        float zMaxPos =  (WALL_SIZE/2)-(offsetFromWall);
        float yMinPos =  0+(offsetFromWall);
        float yMaxPos =  WALL_SIZE-(offsetFromWall);

        float xRange = xMaxPos-xMinPos;
        float yRange = yMaxPos-yMinPos;
        float zRange = zMaxPos-zMinPos;

        for (int ithLineInA = 0; ithLineInA < numOfLinesInAxisA; ithLineInA++)
        {
            for (int ithLineInB = 0; ithLineInB < numOfLinesInAxisB; ithLineInB++)
            {
                float pointPosX = 0f;
                float pointPosY = 0f;
                float pointPosZ = 0f;
                Vector3 colliderSize = Vector3.zero;

                if (planeName == "XY")
                {
                    pointPosX = xMinPos + (((float)ithLineInA/(float)numOfSpacesInA) * xRange);
                    pointPosY = yMinPos + (((float)ithLineInB/(float)numOfSpacesInB) * yRange);
                    pointPosZ = zMinPos;
                    colliderSize = new Vector3(2f, 2f, 0f);
                }
                else if (planeName == "XZ")
                {
                    pointPosX = xMinPos + (((float)ithLineInA/(float)numOfSpacesInA) * xRange);
                    pointPosY = yMinPos;
                    pointPosZ = zMinPos + (((float)ithLineInB/(float)numOfSpacesInB) * zRange);
                    colliderSize = new Vector3(2f, 0f, 2f);
                }
                else if (planeName == "YZ")
                {
                    pointPosX = xMaxPos;
                    pointPosY = yMinPos + (((float)ithLineInA/(float)numOfSpacesInA) * yRange);
                    pointPosZ = zMinPos + (((float)ithLineInB/(float)numOfSpacesInB) * zRange);
                    colliderSize = new Vector3(0f, 2f, 2f);
                }

                GameObject obj = new GameObject($"point{planeName}=({ithLineInA},{ithLineInB})");
                obj.transform.SetParent(grid.transform);
                obj.tag = "GridPoint";
                obj.transform.position = new Vector3(pointPosX, pointPosY, pointPosZ);


                BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
                boxCollider.size = pointWidth * colliderSize;
                boxCollider.isTrigger = true;

                // Point point = obj.AddComponent<Point>();
                // point.SetCoordinates(new Vector3(pointPosX, pointPosY, pointPosZ));
                // point.SetStyle(Color.black, pointWidth);
                // point.SetEnable(false);
            }
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
