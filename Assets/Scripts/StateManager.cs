using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System;
using Assets.Scripts.JsonConverters;

namespace Assets.Scripts
{
    public class StateManager
    {

#if UNITY_EDITOR
        private static readonly string pathToFolderWithSavedStates = "./Assets/SavedWorkspaces";
#else
    private static readonly string pathToFolderWithSavedStates = "./SavedWorkspaces";
#endif

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
                public string WallName { get; set; }
                public Vector3 ConstPoint1 { get; set; }
                public Vector3 ConstPoint2 { get; set; }
                public string ParentWallName { get; set; }
            }
            private class FaceJson
            {
                public List<KeyValuePair<string, Vector3>> Vertices { get; set; }
            }

            private static List<LineJson> _lines = new List<LineJson>();
            private static List<PointJson> _points = new List<PointJson>();
            private static List<CircleJson> _circles = new List<CircleJson>();
            private static List<WallJson> _walls = new List<WallJson>();
            private static List<FaceJson> _faces = new List<FaceJson>();


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
                    FACES = _faces,
                };

                var json = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
                File.WriteAllText(pathToFolderWithSavedStates + "/stateModeExperimental.json", json);

                _points.Clear();
                _lines.Clear();
                _circles.Clear();
                _walls.Clear();
                _faces.Clear();

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

            public static void StoreWall(string wallName, Vector3 point1, Vector3 point2, string parentWallName)
            {
                _walls.Add(new WallJson()
                {
                     WallName = wallName,
                     ConstPoint1 = point1,
                     ConstPoint2 = point2,
                     ParentWallName = parentWallName
                });
            }

            public static void StoreFace(List<KeyValuePair<string, Vector3>> vertices)
            {
                _faces.Add(new FaceJson()
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
                    WALLS = new List<WallJson>(),
                    FACES = new List<FaceJson>()
                };

                var result = JsonConvert.DeserializeAnonymousType(json, template, settings);

                _points = result?.POINTS ?? new List<PointJson>();
                _lines = result?.LINES ?? new List<LineJson>();
                _circles = result?.CIRCLES ?? new List<CircleJson>();
                _walls = result?.WALLS ?? new List<WallJson>();
                _faces = result?.FACES ?? new List<FaceJson>();

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

            public static void RestoreFaces(FacesGenerator fg)
            {
                var restoredFaces = new List<FaceJson>();
                restoredFaces.AddRange(_faces);

                _faces.Clear();

                restoredFaces.ForEach(face =>
                {
                    fg.GenerateFace(face.Vertices);
                });
            }

            public static void RestoreWalls(WallCreator wctr)
            {
                var restoredWalls = new List<WallJson>();
                restoredWalls.AddRange(_walls);

                _walls.Clear();

                restoredWalls.ForEach(wall =>
                {
                    wctr.RestoreWall(wall.WallName, wall.ConstPoint1, wall.ConstPoint2, wall.ParentWallName);
                });
            }
        }

    }
}