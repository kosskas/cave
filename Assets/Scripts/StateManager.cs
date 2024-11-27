using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System;
using Assets.Scripts.Experimental;
using Assets.Scripts.Experimental.Items;
using System.Reflection;

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
            private class LineJson
            {
                public string PlaneName { get; set; }
                public Vector3 StartPosition { get; set; }
                public Vector3 EndPosition { get; set; }
                public List<string> Labels { get; set; }
                public List<string> BoundPointsByLabel { get; set; }
                public float LineWidth { get; set; }
            }

            private class PointJson
            {
                public string PlaneName { get; set; }
                public Vector3 Position { get; set; }
                public List<string> Labels { get; set; }
            }

            private class CircleJson
            {
                public string PlaneName { get; set; }
                public Vector3 StartPosition { get; set; }
                public Vector3 EndPosition { get; set; }
                public float LineWidth { get; set; }
            }

            private class WallJson
            {
                public List<KeyValuePair<string, Vector3>> Vertices { get; set; }
            }

            private static List<LineJson> _lines = new List<LineJson>();
            private static List<PointJson> _points = new List<PointJson>();
            private static List<CircleJson> _circles = new List<CircleJson>();
            private static List<WallJson> _walls = new List<WallJson>();


            public static void Save()
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                settings.Converters.Add(new Vector3Converter());

                var data = new
                {
                    POINTS = _points,
                    LINES = _lines,
                    CIRCLES = _circles,
                    WALLS = _walls,
                };

                var json = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
                File.WriteAllText(pathToFolderWithSavedStates + "/stateModeExperimental.json", json);

                _points.Clear();
                _lines.Clear();
                _circles.Clear();
                _walls.Clear();

                Debug.Log("State saved to JSON file.");
            }

            public static void StoreLine(string planeName, Vector3 startPosition, Vector3 endPosition, List<string> boundPointsByLabel, List<string> labels, float lineWidth)
            {
                _lines.Add(new LineJson()
                {
                    PlaneName = planeName,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    BoundPointsByLabel = boundPointsByLabel,
                    Labels = labels,
                    LineWidth = lineWidth
                });
            }

            public static void StorePoint(string planeName, Vector3 position, List<string> labels)
            {
                _points.Add(new PointJson()
                {
                    PlaneName = planeName,
                    Position = position,
                    Labels = labels
                });
            }

            public static void StoreCircle(string planeName, Vector3 startPosition, Vector3 endPosition, float lineWidth)
            {
                _circles.Add(new CircleJson()
                {
                    PlaneName = planeName,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    LineWidth = lineWidth
                });
            }

            public static void StoreWall(List<KeyValuePair<string, Vector3>> vertices)
            {
                _walls.Add(new WallJson()
                {
                    Vertices = vertices
                });
            }

            // - - -

            public static void Load()
            {
                var path = pathToFolderWithSavedStates + "/stateModeExperimental.json";
                if (!File.Exists(path))
                    return;

                var json = File.ReadAllText(path);
                var data = JsonConvert.DeserializeObject<Dictionary<string, JArray>>(json);
                if (data == null)
                    return;

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                settings.Converters.Add(new Vector3Converter());

                var template = new
                {
                    POINTS = new List<PointJson>(),
                    LINES = new List<LineJson>(),
                    CIRCLES = new List<CircleJson>(),
                    WALLS = new List<WallJson>()
                };

                var result = JsonConvert.DeserializeAnonymousType(json, template, settings);

                _points = result?.POINTS ?? new List<PointJson>();
                _lines = result?.LINES ?? new List<LineJson>();
                _circles = result?.CIRCLES ?? new List<CircleJson>();
                _walls = result?.WALLS ?? new List<WallJson>();

                Debug.Log("State load from JSON file.");
            }

            public static void RestorePoints(Action<WallInfo, Vector3, List<string>> restorePoint)
            {
                var planes = GameObject.Find("Walls")?.GetComponent<WallController>();
                if (planes == null)
                    return;

                _points.ForEach(point =>
                {
                    restorePoint(planes.GetWallByName(point.PlaneName), point.Position, point.Labels);
                });

                _points.Clear();
            }

            public static void RestoreLines(Action<WallInfo, Vector3, Vector3, List<string>, List<string>, float> restoreLine)
            {
                var planes = GameObject.Find("Walls")?.GetComponent<WallController>();
                if (planes == null)
                    return;

                _lines.ForEach(line =>
                {
                    restoreLine(planes.GetWallByName(line.PlaneName), line.StartPosition, line.EndPosition, line.BoundPointsByLabel, line.Labels, line.LineWidth);
                });

                _lines.Clear();
            }

            public static void RestoreCircles(Action<WallInfo, Vector3, Vector3, float> restoreCircle)
            {
                var planes = GameObject.Find("Walls")?.GetComponent<WallController>();
                if (planes == null)
                    return;

                _circles.ForEach(circle =>
                {
                    restoreCircle(planes.GetWallByName(circle.PlaneName), circle.StartPosition, circle.EndPosition, circle.LineWidth);
                });

                _circles.Clear();
            }

            public static void RestoreWalls(WallGenerator wg)
            {
                var restoredWalls = new List<WallJson>();
                restoredWalls.AddRange(_walls);

                _walls.Clear();

                restoredWalls.ForEach(wall =>
                {
                    wg.GenerateWall(wall.Vertices);
                });
            }

        }

    }
}