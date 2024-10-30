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

    private Dictionary<string, dynamic> config = new Dictionary<string, dynamic>
    {
        { "wall_width", 0.1f },
        { "wall_length", 3.4f },
        { "wall_height", 3.4f }
    };

    private GameObject _workspace = null;
    private GameObject _gridRepo = null;

	
    public void Init()
    {
        _workspace = GameObject.Find("Workspace") ?? new GameObject("Workspace");

        _gridRepo = _workspace.transform.Find("GridRepo")?.gameObject ?? new GameObject("GridRepo");
        _gridRepo.transform.SetParent(_workspace.transform);

        GameObject wallsObject = GameObject.Find("Walls");
        WallController _wc = wallsObject.GetComponent<WallController>();
        List<WallInfo> walls = _wc.GetWalls();

        ParseConfigFile();

        float height = config["wall_height"] - config["wall_width"] - 2 * config["margin"];
        float length = config["wall_length"] - config["wall_width"] - 2 * config["margin"];

        int rowsNumber = config["number_of_lines"];
        int colsNumber = config["number_of_lines"];

        float lineWidth = config["line_width"];
        int scale = config["number_of_units_per_interval"];

        // left grid

        const int leftWallIdx = 1;
        Vector3 leftGridRotation = new Vector3(0f, -90f, 0f);
        Vector3 leftGridPosition = 
            walls[leftWallIdx].gameObject.transform.position
            + _GetOffsetFromWall(walls[leftWallIdx], config["distance_from_the_wall"])
            + new Vector3(0f, -height/2f, -length/2f);

        GridINFO leftGridINFO = new GridINFO(
            height,
            length,
            rowsNumber,
            colsNumber,
            "Y",
            "X",
            lineWidth,
            scale,
            leftGridPosition,
            leftGridRotation
        );

        Grid leftGrid = new Grid(
            leftGridINFO,
            _gridRepo
        );

        State.Grids.Add(leftGridINFO);

        // right grid

        const int rightWallIdx = 0;
        Vector3 rightGridRotation = new Vector3(0f, 180f, 0f);
        Vector3 rightGridPosition = 
            walls[rightWallIdx].gameObject.transform.position
            + _GetOffsetFromWall(walls[rightWallIdx], config["distance_from_the_wall"])
            + new Vector3(length/2f, -height/2f, 0f);

        GridINFO rightGridINFO = new GridINFO(
             height,
            length,
            rowsNumber,
            colsNumber,
            "Y",
            "Z",
            lineWidth,
            scale,
            rightGridPosition,
            rightGridRotation
        );

        Grid rightGrid = new Grid(
            rightGridINFO,
            _gridRepo
        );

        State.Grids.Add(rightGridINFO);

        // bottom grid

        const int bottomWallIdx = 2;
        Vector3 bottomGridRotation = new Vector3(90f, -90f, 0f);
        Vector3 bottomGridPosition = 
            walls[bottomWallIdx].gameObject.transform.position
            + _GetOffsetFromWall(walls[bottomWallIdx], config["distance_from_the_wall"])
            + new Vector3(length/2f, 0f, -height/2f);  

        GridINFO bottomGridINFO = new GridINFO(
            height,
            length,
            rowsNumber,
            colsNumber,
            "Z",
            "X",
            lineWidth,
            scale,
            bottomGridPosition,
            bottomGridRotation
        );

        Grid bottomGrid = new Grid(
            bottomGridINFO,
            _gridRepo
        );

        State.Grids.Add(bottomGridINFO);
    }

    private Vector3 _GetOffsetFromWall(WallInfo wall, float distanceFromWallEdge)
    {
        float wallHalfWidth = 0.5f * wall.gameObject.transform.localScale.x;
        float distanceFromWall = wallHalfWidth + distanceFromWallEdge;

        Vector3 wallNormal = wall.GetNormal();
        Vector3 gridOffsetFromWall = wallNormal * distanceFromWall;

        return gridOffsetFromWall;
    }

    public void Clear()
    {
        if (_gridRepo != null) {
            Destroy(_gridRepo);
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

                    try {
                        switch (key)
                        {
                            case "number_of_lines":
                            case "number_of_units_per_interval":
                                config[key] = int.Parse(value);
                                break;

                            case "line_width":
                            case "distance_from_the_wall":
                            case "margin":
                                config[key] = float.Parse(value, CultureInfo.InvariantCulture);
                                break;

                            default:
                                break;
                        }
                    } catch (ArgumentNullException) {
                        Debug.LogError($"[CAVE] Value of key ({key}) not found.");
                        return;
                    } catch (FormatException) {
                        Debug.LogError($"[CAVE] Value of ({key}) cannot be read as number.");
                        return;
                    } catch (OverflowException) {
                        Debug.LogError($"[CAVE] Value of ({key}) is too small or too large.");
                        return;
                    }
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
