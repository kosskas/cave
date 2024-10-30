using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public class PointINFOjson
{
    public string Label { get; set; }
    public string FullLabel { get; set; }
    public int WallNumber { get; set; }
    public string GridPoint { get; set; }
}

public class EdgeINFOjson
{
    public string EdgeName { get; set; }
}

public class DataJson
{
    public List<object> GRIDS { get; set; }
    public List<PointINFOjson> POINTS { get; set; }
    public List<EdgeINFOjson> EDGES { get; set; }
}

static class State {

    /// <summary>
    /// Ścieżka względna dostępu do katalogu zawierającego pliki w formacie .wobj
    /// </summary>
    #if UNITY_EDITOR
        static private readonly string pathToFolderWithSavedStates = "./Assets/SavedWorkspaces";
    #else
        static private readonly string pathToFolderWithSavedStates = "./SavedWorkspaces";
    #endif


    static public List<GridINFO> Grids = new List<GridINFO>();

    static public List<PointINFO> Points = new List<PointINFO>();

    static public List<EdgeINFO> Edges = new List<EdgeINFO>();


    static public void Save()
    {
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        settings.Converters.Add(new Vector3Converter());
        settings.Converters.Add(new PointINFOConverter());
        settings.Converters.Add(new EdgeINFOConverter());

        var data = new
        {
            GRIDS = Grids,
            POINTS = Points,
            EDGES = Edges
        };

        string json = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
        System.IO.File.WriteAllText(pathToFolderWithSavedStates + "/data.json", json);
        
        Debug.Log("State saved to JSON file.");
    }

    static public void Restore(PointPlacer _pp)
    {
        string path = pathToFolderWithSavedStates + "/data.json";
        if (!File.Exists(path)) {
            return;
        }

        string json = File.ReadAllText(path);
        DataJson data = JsonConvert.DeserializeObject<DataJson>(json);

        GameObject wallsObject = GameObject.Find("Walls");
        WallController _wc = wallsObject.GetComponent<WallController>();
        List<WallInfo> walls = _wc.GetWalls();

        Points = data.POINTS
            .Select(p => new PointINFO(GameObject.Find(p.GridPoint), walls[p.WallNumber], p.Label, p.FullLabel))
            .ToList();

        Points.ForEach(p => _pp.AddPoint(p));

        Edges = data.EDGES
            .Select(e => {
                string[] fullLabelTexts = e.EdgeName.Split('-');
                string fullLabelText_1 = fullLabelTexts[0];
                string fullLabelText_2 = fullLabelTexts[1];

                return _pp.AddEdge(Points.FirstOrDefault(p => p.FullLabel.Equals(fullLabelText_1)), Points.FirstOrDefault(p => p.FullLabel.Equals(fullLabelText_2)));
            })
            .ToList();

        Debug.Log("State restored from JSON file.");
    }

    static public void ListAll()
    {
        Debug.Log($"GRIDS {Grids.Count}");
        foreach (var grid in Grids)
        {
            Debug.Log(grid.ToString());
        }

        Debug.Log($"POINTS {Points.Count}");
        foreach (var point in Points)
        {
            Debug.Log(point.ToString());
        }

        Debug.Log($"EDGES {Edges.Count}");
        foreach (var edge in Edges)
        {
            Debug.Log(edge.ToString());
        }
        
    }
}