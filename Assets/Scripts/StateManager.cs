using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System;

namespace Assets.Scripts
{
    public class StateManager
    {

#if UNITY_EDITOR
        private static readonly string pathToFolderWithSavedStates = "./Assets/SavedWorkspaces";
#else
    private static readonly string pathToFolderWithSavedStates = "./SavedWorkspaces";
#endif

        public class Grid
        {
            //static public List<GridINFO> Grids = new List<GridINFO>();

            public static List<PointINFO> Points = new List<PointINFO>();

            public static List<EdgeINFO> Edges = new List<EdgeINFO>();

            public static List<List<KeyValuePair<string, Vector3>>> Walls = new List<List<KeyValuePair<string, Vector3>>>();

            public static void Save()
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                settings.Converters.Add(new Vector3Converter());
                settings.Converters.Add(new PointINFOConverter());
                settings.Converters.Add(new EdgeINFOConverter());

                var data = new
                {
                    //GRIDS = Grids,
                    POINTS = Points,
                    EDGES = Edges,
                    WALLS = Walls
                };

                var json = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
                File.WriteAllText(pathToFolderWithSavedStates + "/stateMode2Dto3D.json", json);

                Debug.Log("State saved to JSON file.");
            }

            public static void Load(PointPlacer pp, WallGenerator wg)
            {
                var path = pathToFolderWithSavedStates + "/stateMode2Dto3D.json";
                if (!File.Exists(path))
                    return;

                var json = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<Dictionary<string, JArray>>(json);
                if (data == null)
                    return;

                var projectionWalls = GameObject.Find("Walls")?.GetComponent<WallController>()?.GetWalls();
                if (projectionWalls == null)
                    return;

                var pointInfos = new List<PointINFO>();
                JArray points;
                if (data.TryGetValue("POINTS", out points))
                {
                    foreach (var point in points)
                    {
                        var gridPoint = point["GridPoint"]?.ToString();
                        var wallNumber = point["WallNumber"]?.ToObject<int>();
                        var label = point["Label"]?.ToString();
                        var fullLabel = point["FullLabel"]?.ToString();

                        if (gridPoint == null || wallNumber == null || label == null || fullLabel == null)
                            continue;

                        var pointInfo = new PointINFO(GameObject.Find(gridPoint), projectionWalls[(int)wallNumber], label, fullLabel);
                        pointInfos.Add(pointInfo);
                        pp.AddPoint(pointInfo);
                    }
                }

                JArray edges;
                if (data.TryGetValue("EDGES", out edges))
                {
                    foreach (var edge in edges)
                    {
                        var edgeName = edge["EdgeName"]?.ToString();

                        if (edgeName == null)
                            continue;

                        var fullLabelText1 = edgeName.Split('-')[0];
                        var fullLabelText2 = edgeName.Split('-')[1];

                        var point1 = pointInfos.FirstOrDefault(p => p.FullLabel.Equals(fullLabelText1));
                        var point2 = pointInfos.FirstOrDefault(p => p.FullLabel.Equals(fullLabelText2));

                        pp.AddEdge(point1, point2);
                    }
                }

                JArray walls;
                if (data.TryGetValue("WALLS", out walls))
                {
                    foreach (var wall in walls)
                    {
                        var wallPointsJson = wall.ToArray();
                        var wallPoints = new List<KeyValuePair<string, Vector3>>();

                        foreach (var wallPointJson in wallPointsJson)
                        {
                            var key = wallPointJson["Key"]?.ToString();
                            var value = wallPointJson["Value"];

                            if (key == null || value == null)
                                continue;

                            var x = value["x"]?.ToObject<float>();
                            var y = value["y"]?.ToObject<float>();
                            var z = value["z"]?.ToObject<float>();

                            if (x == null || y == null || z == null)
                                continue;

                            wallPoints.Add(new KeyValuePair<string, Vector3>(key, new Vector3((float)x, (float)y, (float)z)));
                        }

                        wg.GenerateWall(wallPoints);
                    }
                }

                Debug.Log("State restored from JSON file.");
            }
        }

        public class Exp
        {
            public static void Save()
            {
            }

            public static void Load()
            {
            }
        }

    }
}