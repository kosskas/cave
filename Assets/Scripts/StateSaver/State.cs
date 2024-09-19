using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;


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
        
        Debug.Log("List saved to JSON file.");
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