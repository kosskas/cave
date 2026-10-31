using Assets.Scripts.Experimental;
using Assets.Scripts.Experimental.Items;
using Assets.Scripts.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.Scripts.FileManagers
{
    public class StateManager
    {

#if UNITY_EDITOR
        private const string PathToFolderWithSavedStates = "./Assets/SavedWorkspaces";
        private const string PathToFolderWithPrints = "./Assets/Prints";
#else
    private const string PathToFolderWithSavedStates = "./SavedWorkspaces";
    private const string PathToFolderWithPrints = "./Prints";
#endif

        private static List<string> _savedStates = new List<string>();
        private static int _currentSavedState = 0;

        private static String CameraObjectId = null;
        private static String OrthoObjectId = null;
        private static String AngleObjectId = null;
        private static String AutoSaverObjectId = null;
        private static String ModeObjectId = null;
        private static Text jsonText = GameObject.Find("FileText").GetComponent<Text>();

        public class Exp
        {
            /*   J S O N   */

            public class Settings
            {
                public string Unit { get; set; }
                public string CameraObjectId { get; set; }
                public string OrthoObjectId { get; set; }
                public string AngleObjectId { get; set; }
                public string AutoSaverObjectId { get; set; }
                public string ModeObjectId { get; set; }
                public bool AutoSaveEnabled { get; set; }
            }
        
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
                [JsonConverter(typeof(Vector3Converter))] public Vector3? ParentNormal { get; set; }
            }

            private class FaceJson
            {
                public List<KeyValuePair<string, Vector3>> Vertices { get; set; }
            }

            private class SceneState
            {
                public Settings SETTINGS { get; set; }
                public List<PointJson> POINTS { get; set; }
                public List<LineJson> LINES { get; set; }
                public List<CircleJson> CIRCLES { get; set; }
                public List<WallJson> WALLS { get; set; }
                public List<FaceJson> FACES { get; set; }
            }

            /*   P D F   */

            private sealed class MongeMap
            {
                public Vector3 Origin, U, V;
                public Vector2 Origin2, U2, V2;
                public int ProjectionNumber;

                public Vector2 Apply(Vector3 p)
                {
                    Vector3 d = p - Origin;
                    return Origin2 + U2 * Vector3.Dot(d, U) + V2 * Vector3.Dot(d, V);
                }
            }

            private sealed class MongeAxis
            {
                public Vector2 A, B;
                public string Label;
                public string ParentName, ChildName;
            }

            private sealed class MongeBounds
            {
                public double MinX = double.PositiveInfinity, MinY = double.PositiveInfinity;
                public double MaxX = double.NegativeInfinity, MaxY = double.NegativeInfinity;

                public void Add(Vector2 p, double radius = 0)
                {
                    MinX = Math.Min(MinX, p.x - radius); MaxX = Math.Max(MaxX, p.x + radius);
                    MinY = Math.Min(MinY, p.y - radius); MaxY = Math.Max(MaxY, p.y + radius);
                }
            }

            private struct MongeRect
            {
                public double MinX, MinY, MaxX, MaxY;

                public double Width { get { return MaxX - MinX; } }
                public double Height { get { return MaxY - MinY; } }

                public MongeRect Inflate(double pad)
                {
                    return new MongeRect
                    {
                        MinX = MinX - pad,
                        MinY = MinY - pad,
                        MaxX = MaxX + pad,
                        MaxY = MaxY + pad
                    };
                }

                public Vector2 Baseline(double fontSize)
                {
                    return new Vector2((float)MinX, (float)(MinY + 0.22 * fontSize));
                }
            }

            private struct MongeSegmentObstacle
            {
                public Vector2 A, B;
                public double HalfWidth;
            }

            private struct MongeCircleObstacle
            {
                public Vector2 Center;
                public double Radius;
                public double HalfWidth;
            }

            private struct MongePointObstacle
            {
                public Vector2 Center;
                public double Radius;
            }

            private sealed class MongePointLabelItem
            {
                public int PointIndex;
                public Vector2 Position;
                public string Text;
                public int CrowdingScore;
            }


            /*   P R I V A T E   M E T H O D S  :  [ JSON ]   */

            private static void SaveJson(string json, string dirPath, string fileName, bool withTimestamp)
            {
                const string extension = "json";

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                var fullFileName = withTimestamp ? $"{fileName}_{timestamp}.{extension}" : $"{fileName}.{extension}";
                var fullPath = Path.Combine(dirPath, fullFileName);

                try
                {
                    // Upewnij sie, ze katalog istnieje
                    Directory.CreateDirectory(dirPath);

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

            private static string GetCurrentJson()
            {
                var folderPath = PathToFolderWithSavedStates;

                if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                    return null;

                _savedStates = Directory.EnumerateFiles(folderPath, "*.json", SearchOption.TopDirectoryOnly)
                    .ToList();

                string json = _savedStates[_currentSavedState % _savedStates.Count()];
                _currentSavedState++;

                return json;
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
                    UpdateFileText(fullPath);
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

            private static void UpdateFileText(string fullPath)
            {
                if (jsonText == null)
                {
                    jsonText = GameObject.Find("FileText").GetComponent<Text>();
                }
                String filePath = fullPath.Substring(fullPath.LastIndexOf("\\") + 1);
                jsonText.text = filePath;
            }


            /*   P R I V A T E   M E T H O D S  :  [ PDF ]   */

            private static void ExportMongePdf(SceneState state, string path, string fileName,
                float emptyBaseY = 0.005f, float emptyBaseZ = 1.695f)
            {
                if (state == null) throw new ArgumentNullException("state");
                if (string.IsNullOrEmpty(path)) throw new ArgumentException("Specify an output directory.");
                if (string.IsNullOrEmpty(fileName) || Path.GetFileName(fileName) != fileName)
                    throw new ArgumentException("fileName must contain a filename, without directories.");

                var points = state.POINTS ?? new List<PointJson>();
                var lines = state.LINES ?? new List<LineJson>();
                var circles = state.CIRCLES ?? new List<CircleJson>();
                var walls = (state.WALLS ?? new List<WallJson>()).ToDictionary(w => w.WallName);
                var samples = new Dictionary<string, List<Vector3>>();
                Action<string, Vector3> add = (name, p) =>
                {
                    if (string.IsNullOrEmpty(name)) throw new ArgumentException("Missing PlaneName.");
                    if (float.IsNaN(p.x) || float.IsInfinity(p.x) || float.IsNaN(p.y) ||
                        float.IsInfinity(p.y) || float.IsNaN(p.z) || float.IsInfinity(p.z))
                        throw new ArgumentException("Non-finite coordinates.");
                    if (!samples.ContainsKey(name)) samples[name] = new List<Vector3>();
                    samples[name].Add(p);
                };
                foreach (var p in points) add(p.PlaneName, p.Position);
                foreach (var l in lines) { add(l.PlaneName, l.StartPosition); add(l.PlaneName, l.EndPosition); }
                foreach (var c in circles) { add(c.PlaneName, c.StartPosition); add(c.PlaneName, c.EndPosition); }
                // Empty planes still contribute hinge data to their parent's calibration.
                // Root WALLS entries do not contain transforms: fallback values are configurable.
                var calibration = samples.ToDictionary(kv => kv.Key, kv => new List<Vector3>(kv.Value));
                foreach (var w in walls.Values)
                    if (!string.IsNullOrEmpty(w.ParentWallName) && w.ConstPoint1.HasValue && w.ConstPoint2.HasValue)
                    {
                        if (!calibration.ContainsKey(w.ParentWallName)) calibration[w.ParentWallName] = new List<Vector3>();
                        calibration[w.ParentWallName].Add(w.ConstPoint1.Value);
                        calibration[w.ParentWallName].Add(w.ConstPoint2.Value);
                    }
                float y0 = calibration.ContainsKey("Wall3")
                    ? MongeMedian(calibration["Wall3"].Select(p => p.y)) : emptyBaseY;
                float z0 = calibration.ContainsKey("Wall1")
                    ? MongeMedian(calibration["Wall1"].Select(p => p.z)) : emptyBaseZ;
                var maps = new Dictionary<string, MongeMap>();
                maps["Wall1"] = new MongeMap
                {
                    Origin = new Vector3(0, y0, z0),
                    U = Vector3.right,
                    V = Vector3.up,
                    U2 = Vector2.right,
                    V2 = Vector2.up,
                    ProjectionNumber = 2
                };
                maps["Wall3"] = new MongeMap
                {
                    Origin = new Vector3(0, y0, z0),
                    U = Vector3.right,
                    V = Vector3.forward,
                    U2 = Vector2.right,
                    V2 = Vector2.up,
                    ProjectionNumber = 1
                };
                var axes = new List<MongeAxis>();
                var visiting = new HashSet<string>();
                int nextProjectionNumber = 3;
                Func<string, MongeMap> build = null;
                build = name =>
                {
                    if (maps.ContainsKey(name)) return maps[name];
                    if (!visiting.Add(name)) throw new ArgumentException("Cyclic plane hierarchy: " + name);
                    WallJson w;
                    if (!walls.TryGetValue(name, out w) || string.IsNullOrEmpty(w.ParentWallName) ||
                        !w.ConstPoint1.HasValue || !w.ConstPoint2.HasValue || !w.ParentNormal.HasValue)
                        throw new ArgumentException("Missing plane or hinge data: " + name);
                    MongeMap parent = build(w.ParentWallName);
                    Vector3 a = w.ConstPoint1.Value, b = w.ConstPoint2.Value;
                    Vector3 u = MongeUnit(b - a);
                    // For perpendicular Monge planes, the parent's normal lies in the child plane.
                    Vector3 v = MongeUnit(w.ParentNormal.Value - u * Vector3.Dot(w.ParentNormal.Value, u));
                    List<Vector3> childSamples;
                    if (samples.TryGetValue(name, out childSamples))
                    {
                        float side = childSamples.Average(p => Vector3.Dot(p - a, v));
                        if (side < 0) v = -v;
                    }
                    Vector2 aa = parent.Apply(a), bb = parent.Apply(b);
                    Vector2 uu = (bb - aa).normalized;
                    if ((bb - aa).sqrMagnitude < 1e-12f) throw new ArgumentException("Invalid hinge: " + name);
                    Vector2 vv = new Vector2(-uu.y, uu.x);
                    // Put child content opposite the parent drawing relative to their hinge.
                    List<Vector3> parentSamples;
                    float parentSide = 0;
                    if (samples.TryGetValue(w.ParentWallName, out parentSamples) && parentSamples.Count > 0)
                        parentSide = parentSamples.Average(p => Vector2.Dot(parent.Apply(p) - aa, vv));
                    if (Math.Abs(parentSide) < 1e-7f)
                        parentSide = Vector2.Dot(parent.Apply(parent.Origin) - aa, vv);
                    // No content on either side: use a deterministic orientation for the empty plane.
                    if (parentSide > 0) vv = -vv;
                    var result = new MongeMap
                    {
                        Origin = a,
                        U = u,
                        V = v,
                        Origin2 = aa,
                        U2 = uu,
                        V2 = vv,
                        ProjectionNumber = nextProjectionNumber++
                    };
                    maps[name] = result;
                    axes.Add(new MongeAxis
                    {
                        A = aa,
                        B = bb,
                        Label = "x" + parent.ProjectionNumber + result.ProjectionNumber,
                        ParentName = w.ParentWallName,
                        ChildName = name
                    });
                    visiting.Remove(name);
                    return result;
                };
                // Preserve WALLS order; build parents first. Number 1 and 2 remain reserved.
                foreach (var w in state.WALLS ?? new List<WallJson>())
                    if (!string.IsNullOrEmpty(w.ParentWallName)) build(w.WallName);
                foreach (string name in samples.Keys) build(name);

                var bounds = new MongeBounds();
                foreach (var p in points) bounds.Add(maps[p.PlaneName].Apply(p.Position));
                foreach (var l in lines)
                {
                    MongeCheckWidth(l.LineWidth);
                    bounds.Add(maps[l.PlaneName].Apply(l.StartPosition));
                    bounds.Add(maps[l.PlaneName].Apply(l.EndPosition));
                }
                foreach (var c in circles)
                {
                    MongeCheckWidth(c.LineWidth);
                    MongeMap m = maps[c.PlaneName];
                    Vector2 a = m.Apply(c.StartPosition), b = m.Apply(c.EndPosition);
                    bounds.Add(a, Vector2.Distance(a, b));
                }
                // Expand each hinge extent to cover all content on both adjacent planes.
                foreach (var axis in axes)
                {
                    Vector2 u = (axis.B - axis.A).normalized;
                    float min = 0, max = Vector2.Distance(axis.A, axis.B);
                    foreach (string name in new[] { axis.ParentName, axis.ChildName })
                    {
                        List<Vector3> planeSamples;
                        if (!samples.TryGetValue(name, out planeSamples)) continue;
                        foreach (var p in planeSamples)
                        {
                            float t = Vector2.Dot(maps[name].Apply(p) - axis.A, u);
                            min = Math.Min(min, t); max = Math.Max(max, t);
                        }
                    }
                    Vector2 origin = axis.A;
                    axis.A = origin + min * u; axis.B = origin + max * u;
                }
                var baseXY = new List<float>();
                foreach (string name in new[] { "Wall1", "Wall3" })
                    if (calibration.ContainsKey(name)) baseXY.AddRange(calibration[name].Select(p => p.x));
                axes.Insert(0, new MongeAxis
                {
                    A = new Vector2(baseXY.Count > 0 ? baseXY.Min() : -0.5f, 0),
                    B = new Vector2(baseXY.Count > 0 ? baseXY.Max() : 0.5f, 0),
                    Label = "x12"
                });
                foreach (var axis in axes) { bounds.Add(axis.A); bounds.Add(axis.B); }

                const double mm = 72.0 / 25.4;
                const double pageWidth = 210 * mm, pageHeight = 297 * mm;
                // Reserve margins for labels, point radius and fixed stroke thickness.
                double maxStrokeMm = lines
                    .Select(l => MongeLineWidthMm(l.LineWidth))
                    .Concat(circles.Select(c => MongeLineWidthMm(c.LineWidth)))
                    .DefaultIfEmpty(0)
                    .Max();
                double marginMm = 28 + maxStrokeMm / 2;
                double availableW = pageWidth - 2 * marginMm * mm;
                const double headerHeight = 18 * mm;
                double availableH = pageHeight - 2 * marginMm * mm - headerHeight;
                if (availableW <= 0 || availableH <= 0)
                    throw new ArgumentException("LineWidth is too large to fit on A4.");
                double width = bounds.MaxX - bounds.MinX, height = bounds.MaxY - bounds.MinY;
                double scale = Math.Min(availableW / Math.Max(width, 1e-9),
                                        availableH / Math.Max(height, 1e-9));
                if (width < 1e-9 && height < 1e-9) scale = mm;
                Func<Vector2, Vector2> page = p => new Vector2(
                    (float)(pageWidth / 2 + (p.x - (bounds.MinX + bounds.MaxX) / 2) * scale),
                    (float)((pageHeight - headerHeight) / 2 + (p.y - (bounds.MinY + bounds.MaxY) / 2) * scale));

                var content = new StringBuilder("0 G 0 g\n1 J 1 j\n");
                // Header: two lines, right-aligned, 10 mm from the right page edge.
                MongePdfTextRight(content, pageWidth - 10 * mm, pageHeight - 12 * mm,
                    ReconstructionInfo.APP_NAME ?? string.Empty, 9, pageWidth - 20 * mm);
                MongePdfTextRight(content, pageWidth - 10 * mm, pageHeight - 17 * mm,
                    ReconstructionInfo.GITHUB_LINK ?? string.Empty, 8, pageWidth - 20 * mm);

                var segmentObstacles = new List<MongeSegmentObstacle>();
                var circleObstacles = new List<MongeCircleObstacle>();
                var pointObstacles = new List<MongePointObstacle>();
                var placedLabels = new List<MongeRect>();

                // 1. Draw lines and register line obstacles
                var pageLines = new List<KeyValuePair<MongeSegmentObstacle, string>>();
                foreach (var l in lines)
                {
                    Vector2 a = page(maps[l.PlaneName].Apply(l.StartPosition));
                    Vector2 b = page(maps[l.PlaneName].Apply(l.EndPosition));
                    double strokePt = MongeLineWidthMm(l.LineWidth) * mm;
                    MongePdfLine(content, a, b, strokePt);

                    var seg = new MongeSegmentObstacle { A = a, B = b, HalfWidth = strokePt * 0.5 };
                    segmentObstacles.Add(seg);

                    if (l.Labels != null && l.Labels.Count > 0)
                    {
                        var nonEmpty = l.Labels.Where(lbl => !string.IsNullOrEmpty(lbl)).ToArray();
                        if (nonEmpty.Length > 0)
                            pageLines.Add(new KeyValuePair<MongeSegmentObstacle, string>(seg, string.Join(", ", nonEmpty)));
                    }
                }

                // 2. Draw circles and register circle obstacles
                foreach (var c in circles)
                {
                    MongeMap m = maps[c.PlaneName];
                    Vector2 a = m.Apply(c.StartPosition), b = m.Apply(c.EndPosition);
                    Vector2 center = page(a);
                    double radius = Vector2.Distance(a, b) * scale;
                    double strokePt = MongeLineWidthMm(c.LineWidth) * mm;
                    MongePdfCircle(content, center, radius, strokePt, false);

                    circleObstacles.Add(new MongeCircleObstacle
                    {
                        Center = center,
                        Radius = radius,
                        HalfWidth = strokePt * 0.5
                    });
                }

                // 3. Draw axis lines and register axis obstacles
                var pageAxes = new List<KeyValuePair<MongeSegmentObstacle, string>>();
                foreach (var axis in axes)
                {
                    Vector2 a = page(axis.A), b = page(axis.B);
                    Vector2 u = (b - a).normalized;
                    if (u.sqrMagnitude < 1e-12f) u = Vector2.right;
                    // Always extend BOTH ends by 8 mm on paper, regardless of drawing scale.
                    a -= u * (float)(8 * mm); b += u * (float)(8 * mm);
                    double strokePt = 0.2 * mm;
                    MongePdfLine(content, a, b, strokePt);

                    var seg = new MongeSegmentObstacle { A = a, B = b, HalfWidth = strokePt * 0.5 };
                    segmentObstacles.Add(seg);
                    if (!string.IsNullOrEmpty(axis.Label))
                        pageAxes.Add(new KeyValuePair<MongeSegmentObstacle, string>(seg, axis.Label));
                }

                // 4. Merge coincident points within the same plane and draw them (white fill + black outline)
                const float pointMergeEps = 0.00001f;
                var groups = new List<List<PointJson>>();
                foreach (var p in points)
                {
                    MongeMap map = maps[p.PlaneName];
                    var group = groups.FirstOrDefault(g => g[0].PlaneName == p.PlaneName &&
                        (map.Apply(g[0].Position) - map.Apply(p.Position)).sqrMagnitude <= pointMergeEps * pointMergeEps);
                    if (group == null) { group = new List<PointJson>(); groups.Add(group); }
                    group.Add(p);
                }

                const double pointRadiusPt = 0.75 * mm;
                const double pointStrokePt = 0.22 * mm;
                var pointLabelItems = new List<MongePointLabelItem>();

                foreach (var group in groups)
                {
                    PointJson p = group[0];
                    Vector2 xy = page(maps[p.PlaneName].Apply(p.Position));
                    MongePdfCircle(content, xy, pointRadiusPt, pointStrokePt, true);

                    int ptIdx = pointObstacles.Count;
                    pointObstacles.Add(new MongePointObstacle
                    {
                        Center = xy,
                        Radius = pointRadiusPt + pointStrokePt * 0.5
                    });

                    string suffix = new string('\'', maps[p.PlaneName].ProjectionNumber);
                    var labels = group.SelectMany(q => q.Labels ?? new List<string>())
                        .Where(label => !string.IsNullOrEmpty(label)).Distinct().Select(label => label + suffix).ToArray();
                    if (labels.Length > 0)
                    {
                        pointLabelItems.Add(new MongePointLabelItem
                        {
                            PointIndex = ptIdx,
                            Position = xy,
                            Text = string.Join(" = ", labels)
                        });
                    }
                }

                // 5. Place and draw labels without overlapping lines, circles, points or other labels
                const double labelFontSize = 9.0;
                var safePageBounds = new MongeRect
                {
                    MinX = 4 * mm,
                    MinY = 4 * mm,
                    MaxX = pageWidth - 4 * mm,
                    MaxY = pageHeight - headerHeight - 2 * mm
                };

                // Sort point labels so points in tighter/crowded areas claim free space first
                double neighborhoodRadius = 8.0 * mm;
                foreach (var item in pointLabelItems)
                {
                    int crowd = 0;
                    foreach (var seg in segmentObstacles)
                        if (MongeDistPointToSegment(item.Position, seg.A, seg.B) <= neighborhoodRadius) crowd += 2;
                    foreach (var pt in pointObstacles)
                        if (Vector2.Distance(item.Position, pt.Center) <= neighborhoodRadius) crowd += 3;
                    item.CrowdingScore = crowd;
                }

                foreach (var item in pointLabelItems.OrderByDescending(i => i.CrowdingScore))
                {
                    MongeRect rect = MongePlacePointLabel(
                        item.Position, item.PointIndex, item.Text, labelFontSize,
                        segmentObstacles, circleObstacles, pointObstacles, placedLabels, safePageBounds);
                    placedLabels.Add(rect);
                    MongePdfText(content, rect.Baseline(labelFontSize), item.Text, labelFontSize);
                }

                foreach (var lineLabel in pageLines)
                {
                    MongeRect rect = MongePlaceLineLabel(
                        lineLabel.Key, lineLabel.Value, labelFontSize,
                        segmentObstacles, circleObstacles, pointObstacles, placedLabels, safePageBounds);
                    placedLabels.Add(rect);
                    MongePdfText(content, rect.Baseline(labelFontSize), lineLabel.Value, labelFontSize);
                }

                foreach (var axisLabel in pageAxes)
                {
                    MongeRect rect = MongePlaceAxisLabel(
                        axisLabel.Key, axisLabel.Value, labelFontSize,
                        segmentObstacles, circleObstacles, pointObstacles, placedLabels, safePageBounds);
                    placedLabels.Add(rect);
                    MongePdfText(content, rect.Baseline(labelFontSize), axisLabel.Value, labelFontSize);
                }

                if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)) fileName += ".pdf";
                Directory.CreateDirectory(path);
                MongeWritePdf(Path.Combine(path, fileName), content.ToString(), pageWidth, pageHeight);
            }

            /*   P R I V A T E   M E T H O D S  :  [ PDF LABEL PLACEMENT ]   */

            private static double MongeTextWidth(string text, double fontSize)
            {
                return MongeWinAnsiBytes(text).Sum(b => MongeHelveticaWidths[b]) / 1000.0 * fontSize;
            }

            private static MongeRect MongeMakePointCandidateRect(Vector2 anchor, Vector2 dir, double gap, double w, double h)
            {
                double hw = w * 0.5, hh = h * 0.5;
                double ax = Math.Max(-1.0, Math.Min(1.0, dir.x * 1.41421356));
                double ay = Math.Max(-1.0, Math.Min(1.0, dir.y * 1.41421356));
                double cx = anchor.x + dir.x * gap + ax * hw;
                double cy = anchor.y + dir.y * gap + ay * hh;
                return new MongeRect { MinX = cx - hw, MaxX = cx + hw, MinY = cy - hh, MaxY = cy + hh };
            }

            private static MongeRect MongeMakeLineCandidateRect(Vector2 anchor, Vector2 normalDir, double gap, double w, double h)
            {
                double hw = w * 0.5, hh = h * 0.5;
                double offset = gap + Math.Abs(normalDir.x) * hw + Math.Abs(normalDir.y) * hh;
                double cx = anchor.x + normalDir.x * offset;
                double cy = anchor.y + normalDir.y * offset;
                return new MongeRect { MinX = cx - hw, MaxX = cx + hw, MinY = cy - hh, MaxY = cy + hh };
            }

            private static MongeRect MongePlacePointLabel(
                Vector2 pt, int ownPointIndex, string text, double fontSize,
                List<MongeSegmentObstacle> segments,
                List<MongeCircleObstacle> circles,
                List<MongePointObstacle> points,
                List<MongeRect> placedLabels,
                MongeRect pageBounds)
            {
                const double mm = 72.0 / 25.4;
                double w = Math.Max(MongeTextWidth(text, fontSize), 1.0);
                double h = 0.97 * fontSize;

                // Collect angles of lines incident to or passing very close to this point
                var incidentAngles = new List<double>();
                foreach (var seg in segments)
                {
                    if (MongeDistPointToSegment(pt, seg.A, seg.B) <= 2.2 * mm)
                    {
                        if ((seg.A - pt).sqrMagnitude > 1.0f)
                            incidentAngles.Add(Math.Atan2(seg.A.y - pt.y, seg.A.x - pt.x));
                        if ((seg.B - pt).sqrMagnitude > 1.0f)
                            incidentAngles.Add(Math.Atan2(seg.B.y - pt.y, seg.B.x - pt.x));
                    }
                }

                var candidateAngles = new List<double>();
                // Sector bisectors between adjacent incident lines get highest priority
                if (incidentAngles.Count >= 2)
                {
                    incidentAngles.Sort();
                    for (int i = 0; i < incidentAngles.Count; i++)
                    {
                        double a1 = incidentAngles[i];
                        double a2 = (i + 1 < incidentAngles.Count)
                            ? incidentAngles[i + 1]
                            : incidentAngles[0] + 2.0 * Math.PI;
                        double span = a2 - a1;
                        if (span > 0.25)
                        {
                            candidateAngles.Add(a1 + span * 0.5);
                            if (span > 0.9)
                            {
                                candidateAngles.Add(a1 + span * 0.35);
                                candidateAngles.Add(a1 + span * 0.65);
                            }
                        }
                    }
                }

                // Standard 36 directions around the point (starting from top-right 45 deg)
                double[] preferredDeg = new double[]
                {
                    45, 135, -45, -135, 30, 60, 120, 150, -30, -60, -120, -150,
                    90, 0, 180, -90, 15, 75, 105, 165, -15, -75, -105, -165,
                    10, 20, 50, 70, 80, 100, 110, 140, 160, 170,
                    -10, -20, -50, -70, -80, -100, -110, -140, -160, -170
                };
                foreach (double deg in preferredDeg)
                    candidateAngles.Add(deg * Math.PI / 180.0);

                double[] gaps = new double[]
                {
                    1.35 * mm, 1.85 * mm, 2.45 * mm, 3.2 * mm, 4.1 * mm, 5.2 * mm, 6.5 * mm
                };

                MongeRect bestRect = MongeMakePointCandidateRect(pt, new Vector2(0.7071f, 0.7071f), 1.5 * mm, w, h);
                double bestScore = double.PositiveInfinity;

                foreach (double gap in gaps)
                {
                    for (int i = 0; i < candidateAngles.Count; i++)
                    {
                        double ang = candidateAngles[i];
                        Vector2 dir = new Vector2((float)Math.Cos(ang), (float)Math.Sin(ang));
                        MongeRect rect = MongeMakePointCandidateRect(pt, dir, gap, w, h);

                        double score = MongeScoreRect(rect, segments, circles, points, placedLabels, pageBounds, -1);

                        // Distance & association cost: keep label close to its own point and away from other points
                        double distOwn = MongeDistPointToRect(pt, rect);
                        score += (gap / mm) * 14.0;

                        for (int j = 0; j < points.Count; j++)
                        {
                            if (j == ownPointIndex) continue;
                            double distOther = MongeDistPointToRect(points[j].Center, rect);
                            if (distOther < distOwn + 0.8 * mm)
                                score += 6000.0 * Math.Pow((distOwn + 0.8 * mm - distOther) / mm, 2);
                        }

                        // Slight preference for sector bisectors and diagonal placements
                        if (i >= preferredDeg.Length) score += 2.0;

                        if (score < bestScore)
                        {
                            bestScore = score;
                            bestRect = rect;
                        }
                    }

                    // Early exit if a completely collision-free spot near the point was found
                    if (bestScore < 80.0) break;
                }

                return bestRect;
            }

            private static MongeRect MongePlaceLineLabel(
                MongeSegmentObstacle ownSeg, string text, double fontSize,
                List<MongeSegmentObstacle> segments,
                List<MongeCircleObstacle> circles,
                List<MongePointObstacle> points,
                List<MongeRect> placedLabels,
                MongeRect pageBounds)
            {
                const double mm = 72.0 / 25.4;
                double w = Math.Max(MongeTextWidth(text, fontSize), 1.0);
                double h = 0.97 * fontSize;

                Vector2 ab = ownSeg.B - ownSeg.A;
                float len = ab.magnitude;
                if (len < 1e-3f)
                    return MongePlacePointLabel(ownSeg.A, -1, text, fontSize, segments, circles, points, placedLabels, pageBounds);

                Vector2 u = ab / len;
                Vector2 n = new Vector2(-u.y, u.x);

                int ownSegIndex = -1;
                for (int i = 0; i < segments.Count; i++)
                {
                    if ((segments[i].A - ownSeg.A).sqrMagnitude < 1e-4f && (segments[i].B - ownSeg.B).sqrMagnitude < 1e-4f)
                    {
                        ownSegIndex = i;
                        break;
                    }
                }

                double[] tSamples = new double[] { 0.5, 0.42, 0.58, 0.34, 0.66, 0.26, 0.74, 0.18, 0.82, 0.12, 0.88 };
                double[] gaps = new double[] { 0.9 * mm + ownSeg.HalfWidth, 1.5 * mm + ownSeg.HalfWidth, 2.3 * mm + ownSeg.HalfWidth, 3.3 * mm + ownSeg.HalfWidth };
                float[] sides = new float[] { 1f, -1f };

                MongeRect bestRect = MongeMakeLineCandidateRect(ownSeg.A + u * (0.5f * len), n, gaps[0], w, h);
                double bestScore = double.PositiveInfinity;

                foreach (double gap in gaps)
                {
                    foreach (double t in tSamples)
                    {
                        Vector2 anchor = ownSeg.A + u * (float)(t * len);
                        foreach (float side in sides)
                        {
                            Vector2 normalDir = n * side;
                            MongeRect rect = MongeMakeLineCandidateRect(anchor, normalDir, gap, w, h);

                            double score = MongeScoreRect(rect, segments, circles, points, placedLabels, pageBounds, ownSegIndex);
                            score += Math.Abs(t - 0.5) * 40.0 + (gap / mm) * 15.0;

                            if (score < bestScore)
                            {
                                bestScore = score;
                                bestRect = rect;
                            }
                        }
                    }
                    if (bestScore < 60.0) break;
                }

                return bestRect;
            }

            private static MongeRect MongePlaceAxisLabel(
                MongeSegmentObstacle axisSeg, string text, double fontSize,
                List<MongeSegmentObstacle> segments,
                List<MongeCircleObstacle> circles,
                List<MongePointObstacle> points,
                List<MongeRect> placedLabels,
                MongeRect pageBounds)
            {
                const double mm = 72.0 / 25.4;
                double w = Math.Max(MongeTextWidth(text, fontSize), 1.0);
                double h = 0.97 * fontSize;

                Vector2 ab = axisSeg.B - axisSeg.A;
                float len = ab.magnitude;
                Vector2 u = len > 1e-4f ? ab / len : Vector2.right;
                Vector2 n = new Vector2(-u.y, u.x);
                if (n.y < 0) n = -n;

                int ownSegIndex = -1;
                for (int i = 0; i < segments.Count; i++)
                {
                    if ((segments[i].A - axisSeg.A).sqrMagnitude < 1e-4f && (segments[i].B - axisSeg.B).sqrMagnitude < 1e-4f)
                    {
                        ownSegIndex = i;
                        break;
                    }
                }

                // Prefer near end B (above/below tip, pulled slightly inward so it stays within margins), fallback toward A
                double[] backOffsets = new double[] { 0.5 * w, 1.0 * w, 0.0, 2.0 * w, 3.5 * w };
                double[] gaps = new double[] { 1.1 * mm, 1.8 * mm, 2.6 * mm };
                float[] sides = new float[] { 1f, -1f };

                MongeRect bestRect = MongeMakeLineCandidateRect(axisSeg.B - u * (float)(0.5 * w), n, 1.1 * mm, w, h);
                double bestScore = double.PositiveInfinity;

                for (int end = 0; end < 2; end++)
                {
                    Vector2 tip = end == 0 ? axisSeg.B : axisSeg.A;
                    Vector2 inward = end == 0 ? -u : u;

                    foreach (double back in backOffsets)
                    {
                        if (back > len * 0.45) continue;
                        Vector2 anchor = tip + inward * (float)back;
                        foreach (double gap in gaps)
                        {
                            foreach (float side in sides)
                            {
                                MongeRect rect = MongeMakeLineCandidateRect(anchor, n * side, gap, w, h);
                                double score = MongeScoreRect(rect, segments, circles, points, placedLabels, pageBounds, ownSegIndex);
                                score += end * 80.0 + (back / mm) * 3.0 + (gap / mm) * 10.0;
                                if (side < 0) score += 5.0;

                                if (score < bestScore)
                                {
                                    bestScore = score;
                                    bestRect = rect;
                                }
                            }
                        }
                    }
                }

                return bestRect;
            }

            private static double MongeScoreRect(
                MongeRect rect,
                List<MongeSegmentObstacle> segments,
                List<MongeCircleObstacle> circles,
                List<MongePointObstacle> points,
                List<MongeRect> placedLabels,
                MongeRect pageBounds,
                int ignoreSegmentIndex)
            {
                const double mm = 72.0 / 25.4;
                const double hardPad = 0.55 * mm;
                const double softPad = 1.65 * mm;

                double score = 0;

                // 1. Keep label inside page margins
                if (rect.MinX < pageBounds.MinX) score += 300000.0 + (pageBounds.MinX - rect.MinX) * 5000.0;
                if (rect.MaxX > pageBounds.MaxX) score += 300000.0 + (rect.MaxX - pageBounds.MaxX) * 5000.0;
                if (rect.MinY < pageBounds.MinY) score += 300000.0 + (pageBounds.MinY - rect.MinY) * 5000.0;
                if (rect.MaxY > pageBounds.MaxY) score += 300000.0 + (rect.MaxY - pageBounds.MaxY) * 5000.0;

                // 2. Check against line & axis segments
                MongeRect paddedRect = rect.Inflate(hardPad);
                for (int i = 0; i < segments.Count; i++)
                {
                    var seg = segments[i];
                    double d = MongeDistSegmentToRect(seg.A, seg.B, rect) - seg.HalfWidth;
                    if (d <= 0)
                    {
                        double clipLen = MongeClippedSegmentLength(seg.A, seg.B, paddedRect);
                        score += 120000.0 + clipLen * 4000.0 + Math.Abs(d) * 5000.0;
                    }
                    else if (d < hardPad)
                    {
                        if (i != ignoreSegmentIndex)
                            score += 8000.0 * Math.Pow((hardPad - d) / hardPad, 2);
                    }
                    else if (d < softPad && i != ignoreSegmentIndex)
                    {
                        score += 45.0 * Math.Pow((softPad - d) / (softPad - hardPad), 2);
                    }
                }

                // 3. Check against circles
                foreach (var c in circles)
                {
                    double d = MongeDistCircleToRect(c.Center, c.Radius, rect) - c.HalfWidth;
                    if (d <= 0)
                        score += 120000.0 + Math.Abs(d) * 5000.0;
                    else if (d < hardPad)
                        score += 8000.0 * Math.Pow((hardPad - d) / hardPad, 2);
                    else if (d < softPad)
                        score += 45.0 * Math.Pow((softPad - d) / (softPad - hardPad), 2);
                }

                // 4. Check against points
                foreach (var pt in points)
                {
                    double d = MongeDistPointToRect(pt.Center, rect) - pt.Radius;
                    if (d <= 0)
                        score += 160000.0 + Math.Abs(d) * 8000.0;
                    else if (d < hardPad)
                        score += 9000.0 * Math.Pow((hardPad - d) / hardPad, 2);
                    else if (d < softPad)
                        score += 35.0 * Math.Pow((softPad - d) / (softPad - hardPad), 2);
                }

                // 5. Check against already placed labels
                foreach (var other in placedLabels)
                {
                    double d = MongeDistRectToRect(rect, other);
                    if (d <= 0)
                    {
                        double ox = Math.Max(0.0, Math.Min(rect.MaxX, other.MaxX) - Math.Max(rect.MinX, other.MinX));
                        double oy = Math.Max(0.0, Math.Min(rect.MaxY, other.MaxY) - Math.Max(rect.MinY, other.MinY));
                        score += 220000.0 + (ox * oy) * 5000.0;
                    }
                    else if (d < 0.75 * mm)
                    {
                        score += 10000.0 * Math.Pow((0.75 * mm - d) / (0.75 * mm), 2);
                    }
                    else if (d < 1.5 * mm)
                    {
                        score += 40.0 * Math.Pow((1.5 * mm - d) / (0.75 * mm), 2);
                    }
                }

                return score;
            }

            private static double MongeDistPointToRect(Vector2 p, MongeRect r)
            {
                double dx = Math.Max(r.MinX - p.x, Math.Max(0.0, p.x - r.MaxX));
                double dy = Math.Max(r.MinY - p.y, Math.Max(0.0, p.y - r.MaxY));
                return Math.Sqrt(dx * dx + dy * dy);
            }

            private static double MongeDistRectToRect(MongeRect a, MongeRect b)
            {
                double dx = Math.Max(a.MinX - b.MaxX, Math.Max(0.0, b.MinX - a.MaxX));
                double dy = Math.Max(a.MinY - b.MaxY, Math.Max(0.0, b.MinY - a.MaxY));
                return Math.Sqrt(dx * dx + dy * dy);
            }

            private static double MongeDistPointToSegment(Vector2 p, Vector2 a, Vector2 b)
            {
                Vector2 ab = b - a;
                float lenSq = ab.sqrMagnitude;
                if (lenSq < 1e-12f) return Vector2.Distance(p, a);
                float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / lenSq);
                return Vector2.Distance(p, a + ab * t);
            }

            private static double MongeClippedSegmentLength(Vector2 a, Vector2 b, MongeRect r)
            {
                double t0 = 0.0, t1 = 1.0;
                double dx = b.x - a.x, dy = b.y - a.y;
                double[] p = new double[] { -dx, dx, -dy, dy };
                double[] q = new double[] { a.x - r.MinX, r.MaxX - a.x, a.y - r.MinY, r.MaxY - a.y };

                for (int i = 0; i < 4; i++)
                {
                    if (Math.Abs(p[i]) < 1e-12)
                    {
                        if (q[i] < 0) return 0.0;
                    }
                    else
                    {
                        double t = q[i] / p[i];
                        if (p[i] < 0)
                        {
                            if (t > t1) return 0.0;
                            if (t > t0) t0 = t;
                        }
                        else
                        {
                            if (t < t0) return 0.0;
                            if (t < t1) t1 = t;
                        }
                    }
                }
                if (t0 > t1) return 0.0;
                double len = Math.Sqrt(dx * dx + dy * dy);
                return Math.Max(1e-3, (t1 - t0) * len);
            }

            private static double MongeDistSegmentToRect(Vector2 a, Vector2 b, MongeRect r)
            {
                if (MongeClippedSegmentLength(a, b, r) > 0.0) return 0.0;

                double d = Math.Min(MongeDistPointToRect(a, r), MongeDistPointToRect(b, r));
                d = Math.Min(d, MongeDistPointToSegment(new Vector2((float)r.MinX, (float)r.MinY), a, b));
                d = Math.Min(d, MongeDistPointToSegment(new Vector2((float)r.MaxX, (float)r.MinY), a, b));
                d = Math.Min(d, MongeDistPointToSegment(new Vector2((float)r.MinX, (float)r.MaxY), a, b));
                d = Math.Min(d, MongeDistPointToSegment(new Vector2((float)r.MaxX, (float)r.MaxY), a, b));
                return d;
            }

            private static double MongeDistCircleToRect(Vector2 center, double radius, MongeRect r)
            {
                double dMin = MongeDistPointToRect(center, r);
                double dxMax = Math.Max(Math.Abs(center.x - r.MinX), Math.Abs(center.x - r.MaxX));
                double dyMax = Math.Max(Math.Abs(center.y - r.MinY), Math.Abs(center.y - r.MaxY));
                double dMax = Math.Sqrt(dxMax * dxMax + dyMax * dyMax);

                if (dMin <= radius && radius <= dMax) return 0.0;
                return Math.Min(Math.Abs(dMin - radius), Math.Abs(dMax - radius));
            }

            private static float MongeMedian(IEnumerable<float> values)
            {
                float[] a = values.OrderBy(v => v).ToArray();
                return a.Length % 2 == 0 ? (a[a.Length / 2 - 1] + a[a.Length / 2]) / 2 : a[a.Length / 2];
            }

            private static Vector3 MongeUnit(Vector3 v)
            {
                if (v.sqrMagnitude < 1e-12f) throw new ArgumentException("Degenerate plane basis.");
                return v.normalized;
            }

            private static double MongeLineWidthMm(float width)
            {
                const float epsilon = 0.000001f;

                if (Math.Abs(width - 0.02f) <= epsilon) return 0.2;
                if (Math.Abs(width - 0.004f) <= epsilon) return 0.1;
                if (Math.Abs(width - 0.008f) <= epsilon) return 0.8;

                return width * 100.0;
            }

            private static void MongeCheckWidth(float width)
            {
                if (width <= 0 || float.IsNaN(width) || float.IsInfinity(width))
                    throw new ArgumentException("LineWidth must be finite and positive.");
            }

            private static string MongeNumber(double v)
            {
                return v.ToString("0.######", CultureInfo.InvariantCulture);
            }

            private static void MongePdfLine(StringBuilder s, Vector2 a, Vector2 b, double width)
            {
                s.AppendFormat(CultureInfo.InvariantCulture, "{0} w {1} {2} m {3} {4} l S\n",
                    MongeNumber(width), MongeNumber(a.x), MongeNumber(a.y), MongeNumber(b.x), MongeNumber(b.y));
            }

            private static void MongePdfCircle(StringBuilder s, Vector2 p, double r, double width, bool fill)
            {
                double x = p.x, y = p.y, k = r * .5522847498307936;
                if (fill)
                {
                    // Bialy srodek (1 g) i czarna obwódka (0 G)
                    double strokeWidth = width > 0 ? width : 0.22 * (72.0 / 25.4);
                    s.Append("1 g 0 G ").Append(MongeNumber(strokeWidth)).Append(" w\n");
                }
                else
                {
                    s.Append(MongeNumber(width)).Append(" w\n");
                }
                s.Append(MongeNumber(x + r)).Append(' ').Append(MongeNumber(y)).Append(" m\n");
                Action<double, double, double, double, double, double> curve = (a, b, c, d, e, f) =>
                    s.Append(string.Join(" ", new[] { MongeNumber(a), MongeNumber(b), MongeNumber(c), MongeNumber(d), MongeNumber(e), MongeNumber(f) })).Append(" c\n");
                curve(x + r, y + k, x + k, y + r, x, y + r); curve(x - k, y + r, x - r, y + k, x - r, y);
                curve(x - r, y - k, x - k, y - r, x, y - r); curve(x + k, y - r, x + r, y - k, x + r, y);
                s.Append(fill ? "h B\n0 g\n" : "h S\n");
            }

            // Built-in conversion avoids Encoding.GetEncoding(1252), unavailable in some Unity builds.
            private static byte[] MongeWinAnsiBytes(string text)
            {
                // Unicode counterparts of Windows-1252 bytes 0x80..0x9F.
                const string special = "\u20AC\u0081\u201A\u0192\u201E\u2026\u2020\u2021"
                    + "\u02C6\u2030\u0160\u2039\u0152\u008D\u017D\u008F"
                    + "\u0090\u2018\u2019\u201C\u201D\u2022\u2013\u2014"
                    + "\u02DC\u2122\u0161\u203A\u0153\u009D\u017E\u0178";
                var bytes = new byte[text.Length];
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (c <= 0x7F || (c >= 0xA0 && c <= 0xFF))
                        bytes[i] = (byte)c;
                    else
                    {
                        int index = special.IndexOf(c);
                        if (index < 0)
                            throw new ArgumentException("Character U+" + ((int)c).ToString("X4")
                                + " is unsupported by the PDF Helvetica/WinAnsi font.");
                        bytes[i] = (byte)(0x80 + index);
                    }
                }
                return bytes;
            }

            private static void MongePdfText(StringBuilder s, Vector2 p, string text, double fontSize = 9)
            {
                // Octal escapes preserve WinAnsi bytes and protect PDF syntax.
                byte[] bytes = MongeWinAnsiBytes(text);
                string escaped = string.Concat(bytes.Select(b => "\\" + Convert.ToString(b, 8).PadLeft(3, '0')).ToArray());
                s.Append("BT /F1 ").Append(MongeNumber(fontSize)).Append(" Tf 1 0 0 1 ").Append(MongeNumber(p.x)).Append(' ')
                    .Append(MongeNumber(p.y)).Append(" Tm (").Append(escaped).Append(") Tj ET\n");
            }

            // Helvetica widths in thousandths of an em, indexed by WinAnsi byte.
            private static readonly int[] MongeHelveticaWidths = new int[]
            {
                761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761,
                761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761, 761,
                278, 278, 355, 556, 556, 889, 667, 191, 333, 333, 389, 584, 278, 333, 278, 278,
                556, 556, 556, 556, 556, 556, 556, 556, 556, 556, 278, 278, 584, 584, 584, 556,
                1015, 667, 667, 722, 722, 667, 611, 778, 722, 278, 500, 667, 556, 833, 722, 778,
                667, 778, 722, 667, 611, 722, 667, 944, 667, 667, 611, 278, 278, 278, 469, 556,
                333, 556, 556, 500, 556, 556, 278, 556, 556, 222, 222, 500, 222, 833, 556, 556,
                556, 556, 333, 500, 278, 556, 500, 722, 500, 500, 500, 334, 260, 334, 584, 761,
                556, 0, 222, 556, 333, 1000, 556, 556, 333, 1000, 667, 333, 1000, 0, 611, 0,
                0, 222, 222, 333, 333, 350, 556, 1000, 333, 1000, 500, 333, 944, 0, 500, 667,
                278, 333, 556, 556, 556, 556, 260, 556, 333, 737, 370, 556, 584, 333, 737, 333,
                400, 584, 333, 333, 333, 556, 537, 278, 333, 333, 365, 556, 834, 834, 834, 611,
                667, 667, 667, 667, 667, 667, 1000, 722, 667, 667, 667, 667, 278, 278, 278, 278,
                722, 722, 778, 778, 778, 778, 778, 584, 778, 722, 722, 722, 722, 667, 667, 611,
                556, 556, 556, 556, 556, 556, 889, 500, 556, 556, 556, 556, 278, 278, 278, 278,
                556, 556, 556, 556, 556, 556, 556, 584, 611, 556, 556, 556, 556, 500, 556, 500,
            };

            private static void MongePdfTextRight(StringBuilder s, double right, double baseline,
                string text, double fontSize, double maxWidth)
            {
                text = text.Replace("\r", " ").Replace("\n", " ");
                if (text.Length == 0) return;
                double widthAtOnePoint = MongeWinAnsiBytes(text).Sum(b => MongeHelveticaWidths[b]) / 1000.0;
                // Fit unusually long headers to the available page width.
                if (widthAtOnePoint * fontSize > maxWidth)
                    fontSize = maxWidth / widthAtOnePoint;
                MongePdfText(s, new Vector2((float)(right - widthAtOnePoint * fontSize), (float)baseline), text, fontSize);
            }

            private static void MongeWritePdf(string target, string drawing, double width, double height)
            {
                byte[] stream = Encoding.ASCII.GetBytes(drawing);
                var objects = new List<byte[]>();
                Func<string, byte[]> ascii = Encoding.ASCII.GetBytes;
                objects.Add(ascii("<< /Type /Catalog /Pages 2 0 R >>"));
                objects.Add(ascii("<< /Type /Pages /Kids [3 0 R] /Count 1 >>"));
                objects.Add(ascii("<< /Type /Page /Parent 2 0 R /MediaBox [0 0 " + MongeNumber(width) + " " +
                    MongeNumber(height) + "] /Resources << /Font << /F1 4 0 R >> >> /Contents 5 0 R >>"));
                objects.Add(ascii("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>"));
                using (var buffer = new MemoryStream())
                {
                    byte[] head = ascii("<< /Length " + stream.Length + " >>\nstream\n");
                    buffer.Write(head, 0, head.Length); buffer.Write(stream, 0, stream.Length);
                    byte[] tail = ascii("\nendstream"); buffer.Write(tail, 0, tail.Length); objects.Add(buffer.ToArray());
                }
                // Assemble fully before overwriting the destination file.
                using (var pdf = new MemoryStream())
                {
                    Action<string> write = text => { byte[] b = ascii(text); pdf.Write(b, 0, b.Length); };
                    write("%PDF-1.4\n"); var offsets = new List<long>();
                    for (int i = 0; i < objects.Count; i++)
                    {
                        offsets.Add(pdf.Position); write((i + 1) + " 0 obj\n");
                        pdf.Write(objects[i], 0, objects[i].Length); write("\nendobj\n");
                    }
                    long xref = pdf.Position;
                    write("xref\n0 " + (objects.Count + 1) + "\n0000000000 65535 f \n");
                    foreach (long offset in offsets) write(offset.ToString("D10", CultureInfo.InvariantCulture) + " 00000 n \n");
                    write("trailer\n<< /Size " + (objects.Count + 1) + " /Root 1 0 R >>\nstartxref\n" +
                        xref.ToString(CultureInfo.InvariantCulture) + "\n%%EOF\n");
                    File.WriteAllBytes(target, pdf.ToArray());
                }
            }


            /*   P U B L I C   M E T H O D S   */

            public static void Save(string dirPath = PathToFolderWithSavedStates, string fileName = "sceneState", bool withTimestamp = true)
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
                    SETTINGS = new Settings(),
                    POINTS = new List<PointJson>(),
                    LINES = new List<LineJson>(),
                    CIRCLES = new List<CircleJson>(),
                    WALLS = new List<WallJson>(),
                    FACES = new List<FaceJson>()
                };

                var id = Guid.NewGuid().ToString("D").Split('-');

                CameraObjectId = string.IsNullOrEmpty(CameraObjectId) ? id[0] : CameraObjectId;
                OrthoObjectId = string.IsNullOrEmpty(OrthoObjectId) ? id[1] : OrthoObjectId;
                AngleObjectId = string.IsNullOrEmpty(AngleObjectId) ? id[2] : AngleObjectId;
                AutoSaverObjectId = string.IsNullOrEmpty(AutoSaverObjectId) ? id[3] : AutoSaverObjectId;
                ModeObjectId = string.IsNullOrEmpty(ModeObjectId) ? id[4] : ModeObjectId;

                ss.SETTINGS.Unit = "Meters";
                ss.SETTINGS.CameraObjectId = CameraObjectId;
                ss.SETTINGS.OrthoObjectId = OrthoObjectId;
                ss.SETTINGS.AngleObjectId = AngleObjectId;
                ss.SETTINGS.AutoSaverObjectId = AutoSaverObjectId;
                ss.SETTINGS.ModeObjectId = ModeObjectId;
                ss.SETTINGS.AutoSaveEnabled = true;

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
                        ParentWallName = wall.parentName,
                        ParentNormal = wall.parentNormal
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

                SaveJson(json, dirPath, fileName, withTimestamp);
            }

            public static void Print()
            {
                var ss = new SceneState()
                {
                    SETTINGS = null,
                    POINTS = new List<PointJson>(),
                    LINES = new List<LineJson>(),
                    CIRCLES = new List<CircleJson>(),
                    WALLS = new List<WallJson>(),
                    FACES = null
                };

                var points = ItemsController.GetPoints();
                var lines = ItemsController.GetLines();
                var circles = ItemsController.GetCircles();
                var walls = ItemsController.GetWalls();

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
                        ParentWallName = wall.parentName,
                        ParentNormal = wall.parentNormal
                    });
                });

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"monge_{timestamp}.pdf";

                try
                {
                    ExportMongePdf(ss, PathToFolderWithPrints, fileName);
                    Debug.Log($"State printed to PDF file: {fileName}");
                }
                catch (Exception ex)
                {
                    Debug.LogError("PDF blad:\n" + ex);
                }
            }

            public static void Load(string fullFilePath = "")
            {
                //var path = GetLexicographicallyLastJson();
                var path = string.IsNullOrEmpty(fullFilePath) ? GetCurrentJson() : fullFilePath;

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

                AngleObjectId = ss.SETTINGS.AngleObjectId;
                CameraObjectId = ss.SETTINGS.CameraObjectId;
                OrthoObjectId = ss.SETTINGS.OrthoObjectId;
                AutoSaverObjectId = ss.SETTINGS.AutoSaverObjectId;
                ModeObjectId = ss.SETTINGS.ModeObjectId;

                ss.WALLS.ForEach(wall =>
                {
                    ItemsController.AddWall(
                        wall.ConstPoint1,
                        wall.ConstPoint2,
                        wall.ParentNormal,
                        wall.ParentWallName,
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