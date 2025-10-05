using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assets.Scripts.Experimental;
using Assets.Scripts.JsonConverters;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts.FileManagers
{
    public class StateManager
    {

#if UNITY_EDITOR
        private const string PathToFolderWithSavedStates = "./Assets/SavedWorkspaces";
#else
    private const string PathToFolderWithSavedStates = "./SavedWorkspaces";
#endif

        public class Exp
        {
            /*   J S O N   */

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
                [JsonConverter(typeof(Vector3Converter))] public Vector3? ConstPoint1 { get; set; }
                [JsonConverter(typeof(Vector3Converter))] public Vector3? ConstPoint2 { get; set; }
                public string ParentWallName { get; set; }
            }
            private class FaceJson
            {
                public List<KeyValuePair<string, Vector3>> Vertices { get; set; }
            }

            private class SceneState
            {
                public List<PointJson> POINTS { get; set; }
                public List<LineJson> LINES { get; set; }
                public List<CircleJson> CIRCLES { get; set; }
                public List<WallJson> WALLS { get; set; }
                public List<FaceJson> FACES { get; set; }
            }


            /*   P R I V A T E   M E T H O D S   */

            private static void SaveJson(string json, string fileName, bool withTimestamp)
            {
                const string extension = "json";

                var folderPath = PathToFolderWithSavedStates;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                var fullFileName = withTimestamp ? $"{fileName}_{timestamp}.{extension}" : $"{fileName}.{extension}";
                var fullPath = Path.Combine(folderPath, fullFileName);

                try
                {
                    // Upewnij siê, ¿e katalog istnieje
                    Directory.CreateDirectory(folderPath);

                    File.WriteAllText(fullPath, json);

                    // Weryfikacja zapisu
                    if (File.Exists(fullPath))
                    {
                        Debug.Log($"State saved to JSON file: {fullPath}");
                    }
                    else
                    {
                        throw new Exception($"SaveFile: WriteAllText completed but file not found: {fullPath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to save file '{fullPath}'. Exception: {ex}");
                }
            }

            private static string GetLexicographicallyLastJson()
            {
                var folderPath = PathToFolderWithSavedStates;

                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                    return null;

                return Directory.EnumerateFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly)
                    .OrderBy(Path.GetFileName, StringComparer.Ordinal)
                    .LastOrDefault();
            }

            private static T LoadJson<T>(string fullPath, JsonSerializerSettings settings)
            {
                if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
                {
                    Debug.LogError($"LoadJson: File not found: '{fullPath}'.");
                    return default(T);
                }

                try
                {
                    var json = File.ReadAllText(fullPath);
                    var obj = settings == null
                        ? JsonConvert.DeserializeObject<T>(json)
                        : JsonConvert.DeserializeObject<T>(json, settings);

                    Debug.Log($"LoadJson: Successfully loaded from file {fullPath}");
                    return obj;
                }
                catch (JsonException jex)
                {
                    Debug.LogError($"LoadJson: JSON parse/deserialize error for '{fullPath}'. Exception: {jex}");
                    return default(T);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"LoadJson: Failed to load '{fullPath}'. Exception: {ex}");
                    return default(T);
                }
            }


            /*   P U B L I C   M E T H O D S   */

            public static void Save(string fileName = "sceneState", bool withTimestamp = true)
            {
                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                settings.Converters.Add(new Vector3Converter());

                var points = ItemsController.GetPoints();
                var lines = ItemsController.GetLines();
                var circles = ItemsController.GetCircles();
                var walls = ItemsController.GetWalls();
                var faces = ItemsController.GetFaces();

                var ss = new SceneState()
                {
                    POINTS = new List<PointJson>(),
                    LINES = new List<LineJson>(),
                    CIRCLES = new List<CircleJson>(),
                    WALLS = new List<WallJson>(),
                    FACES = new List<FaceJson>()
                };

                points.ForEach(point =>
                {
                    ss.POINTS.Add(new PointJson()
                    {
                        Labels = point.Labels,
                        PlaneName = point.Plane.name,
                        Position = point.Position,
                    });
                });

                lines.ForEach(line =>
                {
                    ss.LINES.Add(new LineJson()
                    {
                        BoundPointsByLabel = line.GetLabelsOfBoundPoints(),
                        EndPosition = line.EndPosition,
                        Labels = line.Labels,
                        LineWidth = line.Width,
                        PlaneName = line.Plane.name,
                        StartPosition = line.StartPosition
                    });
                });

                circles.ForEach(circle =>
                {
                    ss.CIRCLES.Add(new CircleJson()
                    {
                        EndPosition = circle.EndPosition,
                        LineWidth = circle.Width,
                        PlaneName = circle.Plane.name,
                        StartPosition = circle.StartPosition
                    });
                });

                walls.ForEach(wall =>
                {
                    ss.WALLS.Add(new WallJson()
                    {
                        ConstPoint1 = wall.constrPoint1,
                        ConstPoint2 = wall.constrPoint2,
                        WallName = wall.name,
                        ParentWallName = wall.parentName //Moze juz nie istniec
                    });
                });

                faces.ForEach(face =>
                {
                    ss.FACES.Add(new FaceJson()
                    {
                        Vertices = face.Points
                    });
                });

                var json = JsonConvert.SerializeObject(ss, Formatting.Indented, settings);

                SaveJson(json, fileName, withTimestamp);
            }

            public static void Load()
            {
                var path = GetLexicographicallyLastJson();

                if (string.IsNullOrEmpty(path) || !File.Exists(path))
                {
                    Debug.LogError("Load: No JSON file to load.");
                    return;
                }

                var settings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };
                settings.Converters.Add(new Vector3Converter());

                var ss = LoadJson<SceneState>(path, settings);
                if (ss == null)
                {
                    Debug.LogError($"Load: Deserialization returned null for '{path}'.");
                    return;
                }

                ss.WALLS.ForEach(wall =>
                {
                    ItemsController.AddWall(
                        wall.ConstPoint1,
                        wall.ConstPoint2,
                        wall.ParentWallName, //Moze juz nie istniec
                        wall.WallName);
                });

                ss.POINTS.ForEach(point =>
                {
                    ItemsController.AddPoint(
                        point.Labels, 
                        point.PlaneName,
                        point.Position);
                });

                ss.LINES.ForEach(line =>
                {
                    ItemsController.AddLine(
                        line.BoundPointsByLabel,
                        line.EndPosition, 
                        line.Labels, 
                        line.LineWidth,
                        line.PlaneName,
                        line.StartPosition);
                });

                ss.CIRCLES.ForEach(circle =>
                {
                    ItemsController.AddCircle(
                        circle.EndPosition,
                        circle.LineWidth,
                        circle.PlaneName,
                        circle.StartPosition);
                });

                ss.FACES.ForEach(face =>
                {
                    ItemsController.AddFace(
                        face.Vertices);
                });

                Debug.Log($"State from file '{path}' restored successfully.");
            }
        }
    }
}
