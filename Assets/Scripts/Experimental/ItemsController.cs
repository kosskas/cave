using Assets.Scripts.Experimental.Items;
using Assets.Scripts.Experimental.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public class ItemsController
    {
        private const float _WALL_HALF_LENGTH = 1.7f;
        private const float _WALL_OFFSET = 0.005f;

        private const float _AXIS_LINE_WIDTH = 0.02f;
        private const float _HELP_LINE_WIDTH = 0.004f;
        private const float _BOLD_LINE_WIDTH = 0.008f;
         
        private readonly WallController _wCtrl;
        private readonly WallCreator _wCrt;
        private readonly MeshBuilder _mB;

        private static ItemsController _ic;

        private static GameObject _workspace;

        private static GameObject _axisRepo;
        private static GameObject _lineRepo;
        private static GameObject _pointRepo;
        private static GameObject _circleRepo;
        private static GameObject _faceRepo;

        private readonly Dictionary<Axis, Tuple<WallInfo, WallInfo>> _axisWalls;

        private List<ExPoint> _facePoints;

        private int hiddenLabelId = 1;

        private bool _allowedOnlyMainDirections = false;


        /*   E N U M S   */

        public enum DrawType
        {
            Full,
            Part
        }


        /*   C L A S S E S   */

        public class LineRef
        {
            public Line Line { get; private set; } = null;
            public bool TakeLength { get; private set; } = false;
            public bool BeParallel { get; private set; } = false;
            public bool BePerpendicular { get; private set; } = false;

            public static LineRef ToTakeLength(Line line)
            {
                if (line == null) return null;

                var lr = new LineRef();
                lr.Line = line;
                lr.TakeLength = true;
                return lr;
            }

            public static LineRef ToBeParallel(Line line)
            {
                if (line == null) return null;

                var lr = new LineRef();
                lr.Line = line;
                lr.BeParallel = true;
                return lr;
            }

            public static LineRef ToBePerpendicular(Line line)
            {
                if (line == null) return null;

                var lr = new LineRef();
                lr.Line = line;
                lr.BePerpendicular = true;
                return lr;
            }
        }


        /*   E V E N T S   */

        public event EventHandler<DrawType> DrawingCompleted;

        protected virtual void OnDrawingCompleted(
            DrawType type
            )
        {
            DrawingCompleted?.Invoke(this, type);
        }


        /*   C O N S T R U C T O R S   */

        public ItemsController(
            WallController wCtrl, 
            WallCreator wCrt, 
            MeshBuilder mB
            )
        {
            _workspace = GameObject.Find("WorkspaceExp") ?? new GameObject("WorkspaceExp");

            _axisRepo = new GameObject("AxisRepo");
            _axisRepo.transform.SetParent(_workspace.transform);

            _lineRepo = new GameObject("LineRepo");
            _lineRepo.transform.SetParent(_workspace.transform);

            _pointRepo = new GameObject("PointRepo");
            _pointRepo.transform.SetParent(_workspace.transform);

            _circleRepo = new GameObject("CircleRepo");
            _circleRepo.transform.SetParent(_workspace.transform);

            _faceRepo = new GameObject("CircleRepo");
            _faceRepo.transform.SetParent(_workspace.transform);

            _axisWalls = new Dictionary<Axis, Tuple<WallInfo, WallInfo>>();

            _facePoints = new List<ExPoint>();

            _wCtrl = wCtrl;
            _wCrt = wCrt;
            _mB = mB;

            _ic = this;
        }


        /*   P R I V A T E   M E T H O D S   */

        private static List<T> GetComponentsFromRepo<T>(
            GameObject repo
            ) where T : Component
        {
            var result = new List<T>();

            if (repo == null) return result;

            foreach (Transform child in repo.transform)
            {
                var comp = child.gameObject.GetComponent<T>();
                if (comp != null) result.Add(comp);
            }

            return result;
        }

        private Vector3 CalcProjectionOnAxis(
            Axis axis, 
            Vector3 point
            )
        {
            if (axis == null)
            {
                Debug.LogError("Brak osi odniesienia");
                return Vector3.zero;
            }
            return CalcProjectionOnAxis(axis.From, axis.To, point);
        }

        private Vector3 CalcProjectionOnAxis(
            Vector3 lineStart, 
            Vector3 lineEnd, 
            Vector3 point
            )
        {
            Vector3 A = lineEnd - lineStart;
            Vector3 B = point - lineStart;

            // B*A = |B||A|cos(a) = |B||1|cos(a) = |B|cos(a) = distance from lineStart to point projection on axis A
            float projectionLength = Vector3.Dot(B, A.normalized);

            Vector3 projectionPoint = lineStart + projectionLength * A.normalized;

            return projectionPoint;
        }

        private Axis GetAxis(
            WallInfo plane
            )
        {
            var axis = _axisWalls.FirstOrDefault(e => e.Value.Item1.Equals(plane) || e.Value.Item2.Equals(plane)).Key;
            return axis;
        }

        private Axis GetAxis(
            WallInfo planeA, 
            WallInfo planeB
            )
        {
            var axis = _axisWalls
                .FirstOrDefault(e =>
                    e.Value.Item1.Equals(planeA) && e.Value.Item2.Equals(planeB) ||
                    e.Value.Item1.Equals(planeB) && e.Value.Item2.Equals(planeA)
                    ).Key;
            return axis;
        }

        private List<Axis> GetAxesOnPlane(
            WallInfo plane
            )
        {
            var axes = _axisWalls
                .Where(e => e.Value.Item1.Equals(plane) || e.Value.Item2.Equals(plane))
                .Select(e => e.Key)
                .ToList();
            return axes;
        }

        private Axis GetAxisWithTheEarliestWall(WallInfo wall)
        {
            var axis = _axisWalls
                .Where(kvp => kvp.Value.Item1 == wall || kvp.Value.Item2 == wall)
                .OrderBy(kvp => (kvp.Value.Item1 == wall ? kvp.Value.Item2 : kvp.Value.Item1).constructionNumber)
                .Select(kvp => kvp.Key)
                .FirstOrDefault();
            return axis;
        }

        /// <summary>
        /// Zwraca oœ, w któr¹ najbardziej "wskazuje" strza³ka (lineFrom -> lineTo).
        /// Kryterium: spoœród osi, których kierunek odchylenia od najlepiej dopasowanego
        /// nie przekracza zadanej tolerancji k¹towej (5 stopni), wybierana jest oœ najbli¿sza
        /// punktowi 'to'. Uwzglêdniane s¹ wy³¹cznie osie znajduj¹ce siê w kierunku
        /// wskazywanym przez strza³kê (dodatni cosinus k¹ta).
        /// </summary>
        /// <returns>Zwraca null, jeœli nie zbli¿amy siê do ¿adnej osi.</returns>     
        private Axis FindApproachingAxis(
            Vector3 from,
            Vector3 to,
            List<Axis> axes
            )
        {
            const float angleToleranceInDeg = 5f; // deg

            var dirVector = to - from;

            if (dirVector.sqrMagnitude < DescriptiveMathLib.EPS)
                return default(Axis);

            var dirVectorNorm = dirVector.normalized;

            // ZnajdŸ najlepszy kierunek
            var maxDot = 0f;

            foreach (var axis in axes)
            {
                var axisPoint = CalcProjectionOnAxis(axis, from);
                var fromToAxisVector = axisPoint - from;
                var fromToAxisDistanceSqr = fromToAxisVector.sqrMagnitude;

                if (fromToAxisDistanceSqr < DescriptiveMathLib.EPS)
                    continue;

                // cosinus k¹ta: 1 = idealnie w stronê osi, 0 = prostopadle, <0 = „za plecami”
                var dot = Vector3.Dot(dirVectorNorm, fromToAxisVector.normalized);

                if (dot > maxDot)
                    maxDot = dot;
            }

            // Nie zbli¿amy siê do ¿adnej osi
            if (maxDot <= 0f)
                return default(Axis);

            // Wyznacz minimalny dopuszczalny dot (odpowiadaj¹cy tolerancji k¹towej 'angleToleranceInDeg')
            var minDotAccepted = Mathf.Cos(
                Mathf.Acos(Mathf.Clamp(maxDot, -1f, 1f)) +
                angleToleranceInDeg * Mathf.Deg2Rad
            );

            // Wybierz najbli¿sz¹ oœ spoœród kierunkowo dopuszczalnych
            var bestAxis = default(Axis);
            var bestDistanceSqr = float.MaxValue;

            foreach (var axis in axes)
            {
                var axisPoint = CalcProjectionOnAxis(axis, from);
                var fromToAxisVector = axisPoint - from;
                var fromToAxisDistanceSqr = fromToAxisVector.sqrMagnitude;
                var toToAxisVector = axisPoint - to;
                var toToAxisDistanceSqr = toToAxisVector.sqrMagnitude;

                if (fromToAxisDistanceSqr < DescriptiveMathLib.EPS)
                    continue;

                if (toToAxisDistanceSqr < DescriptiveMathLib.EPS)
                    continue;

                // cosinus k¹ta: 1 = idealnie w stronê osi, 0 = prostopadle, <0 = „za plecami”
                var dot = Vector3.Dot(dirVectorNorm, fromToAxisVector.normalized);

                if (toToAxisDistanceSqr < bestDistanceSqr && dot > 0f && dot >= minDotAccepted)
                {
                    bestAxis = axis;
                    bestDistanceSqr = toToAxisDistanceSqr;
                }
            }

            return bestAxis;
        }



        private WallInfo FindPlane(
            WallInfo hitPlane, 
            IRaycastable hitObject
            )
        {
            return hitPlane ?? (hitObject as IDrawable)?.Plane;
        }

        private WallInfo FindEndPlane(
            Axis axis, 
            WallInfo startPlane
            )
        {
            var planes = _axisWalls[axis];
            var plane1 = planes.Item1;
            var plane2 = planes.Item2;

            return (plane1 == startPlane) ? plane2 : plane1;
        }

        private Vector3 CalcPosition(
            WallInfo plane, 
            Vector3 hitPosition, 
            ExPoint hitPoint = null
            )
        {
            var planeNormal = plane.GetNormal();
            var planePosition = plane.gameObject.transform.position;

            var position = Vector3.zero;

            if (hitPoint != null)
            {
                position = hitPoint.Position;
            }
            else
            {
                if (Mathf.Approximately(Mathf.Abs(planeNormal.x), 1))
                    position.x = planePosition.x + Mathf.Sign(planeNormal.x) * _WALL_OFFSET;
                else
                    position.x = hitPosition.x;

                if (Mathf.Approximately(Mathf.Abs(planeNormal.y), 1))
                    position.y = planePosition.y + Mathf.Sign(planeNormal.y) * _WALL_OFFSET;
                else
                    position.y = hitPosition.y;

                if (Mathf.Approximately(Mathf.Abs(planeNormal.z), 1))
                    position.z = planePosition.z + Mathf.Sign(planeNormal.z) * _WALL_OFFSET;
                else
                    position.z = hitPosition.z;
            }

            // Debug.Log($"hitPosition=({hitPosition.x:F9}, {hitPosition.y:F9}, {hitPosition.z:F9}) " +
            //           $"position=({position.x:F9}, {position.y:F9}, {position.z:F9}) " +
            //           $"planeNormal=({planeNormal.x:F9}, {planeNormal.y:F9}, {planeNormal.z:F9}) " +
            //           $"planePosition=({planePosition.x:F9}, {planePosition.y:F9}, {planePosition.z:F9})");

            return position;
        }

        Vector3 ScaleToLength(
            Vector3 from, 
            Vector3 to, 
            float length
            )
        {
            return from + (to - from).normalized * length;
        }

        private Vector3 CastPointOnPlane(
            Vector3 pointOnPlane,
            Vector3 pointToCast,
            WallInfo plane
            )
        {
            return pointOnPlane + Vector3.ProjectOnPlane(pointToCast - pointOnPlane, plane.GetNormal());
        } 

        private Vector3 SnapToAxisDirections(
            Vector3 startPoint,
            Vector3 targetPoint,
            Vector3 axisStart,
            Vector3 axisEnd,
            Vector3 planeNormal
            )
        {
            var parallel = Vector3.ProjectOnPlane(axisEnd - axisStart, planeNormal).normalized;
            var perpendicular = Vector3.Cross(planeNormal, parallel).normalized;

            // Four bidirectional axes (8 possible directions)
            var directions = new[]
            {
                parallel,
                perpendicular,
                (parallel + perpendicular).normalized,
                (parallel - perpendicular).normalized
            };

            var targetVector = Vector3.ProjectOnPlane(targetPoint - startPoint, planeNormal);

            var bestDirection = Vector3.zero;
            var bestProjection = 0f;
            var bestDot = -1f;

            // Find the closest direction
            foreach (var direction in directions)
            {
                var projection = Vector3.Dot(targetVector, direction);
                var dot = Mathf.Abs(projection);

                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestProjection = projection;
                    bestDirection = direction;
                }
            }

            return startPoint + bestDirection * bestProjection;
        }


        private void HandleIntersectionStart(
            IRaycastable hitObject, 
            HashSet<IAnalyzable> intersectedObjs, 
            IRaycastable currentComponent
            )
        {
            if (hitObject is IAnalyzable && hitObject != currentComponent)
            {
                IAnalyzable aHitObject = (IAnalyzable)hitObject;
                intersectedObjs.Add(aHitObject.GetElement());

                if (hitObject is IColorable)
                {
                    IColorable cHitObject = (IColorable)hitObject;
                    cHitObject.Color = ReconstructionInfo.MENTIONED;
                }
            }
        }

        private void HandleIntersectionsEnd(
            WallInfo plane, 
            HashSet<IAnalyzable> intersectedObjs, 
            IAnalyzable drawableComponent
            )
        {
            foreach (var intersected in intersectedObjs)
            {
                List<Vector3> crossings = intersected.FindCrossingPoints(drawableComponent);

                if (crossings != null)
                {
                    foreach (var point in crossings)
                    {
                        DrawPoint(plane, point, null, DrawType.Part);
                    }
                }

                if (intersected is IColorable)
                {
                    IColorable cIntersected = (IColorable)intersected;
                    cIntersected.Color = ReconstructionInfo.NORMAL;
                }
            }
        }

        private void HandleIntersectionsAbort(
            HashSet<IAnalyzable> intersectedObjs
            )
        {
            foreach (var intersected in intersectedObjs)
            {
                if (intersected is IColorable)
                {
                    IColorable cIntersected = (IColorable)intersected;
                    cIntersected.Color = ReconstructionInfo.NORMAL;
                }
            }
        }

        private bool NoPointOnPosition(
            Vector3 position
            )
        {
            return GetPointOnPosition(position) == null;
        }

        private ExPoint GetPointOnPosition(
            Vector3 position
            )
        {
            foreach (Collider collider in Physics.OverlapSphere(position, DescriptiveMathLib.EPS))
            {
                IRaycastable raycastable = collider.gameObject.GetComponent<IRaycastable>();
                if (raycastable == null) continue;

                ExPoint exPoint = raycastable as ExPoint;
                if (exPoint == null) continue;

                if (Vector3.SqrMagnitude(exPoint.Position - position) >= DescriptiveMathLib.EPS) continue;

                // exists point relative to position closer than eps distance 
                return exPoint;
            }

            return null;
        }

        private ExPoint DrawOrGetPoint(
            WallInfo plane,
            Vector3 position
            )
        {
            var exPoint = GetPointOnPosition(position);

            if (exPoint == null)
            {
                var point = new GameObject("POINT");
                point.transform.SetParent(_pointRepo.transform);
                _wCtrl.LinkConstructionToWall(plane, point);

                var pointComponent = point.AddComponent<ExPoint>();
                pointComponent.Draw(plane, position);
                pointComponent.EnabledLabels = true;
                pointComponent.Color = ReconstructionInfo.NORMAL;

                return pointComponent;
            }
            else
            {
                return exPoint;
            }
        }


        /*   P U B L I C   M E T H O D S   */

        public void AddAxisBetweenPlanes(
            WallInfo planeA, 
            WallInfo planeB
            )
        {
            var axis = new GameObject("AXIS");
            axis.transform.SetParent(_axisRepo.transform); 
            
            var axisComponent = axis.AddComponent<Axis>();
            axisComponent.Width = _AXIS_LINE_WIDTH;

            if (planeB.constrPoint1 != null && planeB.constrPoint2 != null)
            {
                axisComponent.Draw(default(WallInfo), planeB.constrPoint1.Value, planeB.constrPoint2.Value);
            }
            else
            {
                Vector3 normalA = planeA.GetNormal();
                Vector3 normalB = planeB.GetNormal();
                Vector3 direction = Vector3.Cross(normalA, normalB).normalized;

                Vector3 posA = planeA.gameObject.transform.position;
                Vector3 posB = planeB.gameObject.transform.position;

                Vector3 v1 = posB - posA;
                Vector3 v2 = Vector3.ProjectOnPlane(v1, normalA);

                Vector3 contactPoint = posA + v2 + _WALL_OFFSET * (normalA + normalB);

                Vector3 from = contactPoint - direction * _WALL_HALF_LENGTH;
                Vector3 to = contactPoint + direction * _WALL_HALF_LENGTH;

                axisComponent.Draw(default(WallInfo), from, to);
            }

            var labelComponent = axis.AddComponent<IndexedLabel>();
            labelComponent.AddLabel("X", "", $"{planeA.constructionNumber}/{planeB.constructionNumber}");
            labelComponent.FontSize = 1;

            _axisWalls.Add(axisComponent, new Tuple<WallInfo, WallInfo>(planeA, planeB));
            _wCtrl.LinkConstructionToWall(planeA, axis);
            _wCtrl.LinkConstructionToWall(planeB, axis);
        }

        public void RemoveAxis(
            WallInfo wallToRem
            )
        {
            if (_axisWalls != null && _axisWalls.Count > 0 && _wCtrl != null)
            {
                var deletingWall = wallToRem;
                if (deletingWall != null)
                {
                    var keysToRemove = _axisWalls
                        .Where(pair => pair.Value.Item1 == deletingWall || pair.Value.Item2 == deletingWall)
                        .Select(pair => pair.Key)
                        .ToList();
                    foreach (var key in keysToRemove)
                    {
                        _axisWalls.Remove(key);
                    }
                }
            }
        }

        public DrawAction Add(
            ExContext context, 
            IRaycastable hitObject,
            Vector3 hitPosition, 
            WallInfo hitPlane,
            IRaycastable relativeObject = null
            )
        {
            // FIND PLANE

            var plane = FindPlane(hitPlane, hitObject);
            if (plane == null)
                return null;

            // GET POSITION INCLUDING ANTI Z-FIGHTING OFFSET

            var position = CalcPosition(plane, hitPosition);
            var positionWithPointSensitivity = CalcPosition(plane, hitPosition, hitObject as ExPoint);
            
            // CALL DRAW METHOD

            switch (context)
            {
                case ExContext.Point: return DrawPoint(plane, position);

                case ExContext.BoldLine: return DrawLine(plane, positionWithPointSensitivity, hitObject as ExPoint, null, _BOLD_LINE_WIDTH);
                
                case ExContext.HelpLine: return DrawLine(plane, positionWithPointSensitivity, hitObject as ExPoint);

                case ExContext.FixedLine: return DrawLine(plane, positionWithPointSensitivity, hitObject as ExPoint, LineRef.ToTakeLength(relativeObject as Line));

                case ExContext.PerpendicularLine: return DrawLine(plane, positionWithPointSensitivity, hitObject as ExPoint, LineRef.ToBePerpendicular(relativeObject as Line));

                case ExContext.ParallelLine: return DrawLine(plane, positionWithPointSensitivity, hitObject as ExPoint, LineRef.ToBeParallel(relativeObject as Line));

                case ExContext.Projection: return DrawProjection(plane, positionWithPointSensitivity, hitObject as ExPoint);

                case ExContext.FixedProjection: return DrawFixedProjection(plane, positionWithPointSensitivity, hitObject as ExPoint, relativeObject);

                case ExContext.Circle: return DrawCircle(plane, positionWithPointSensitivity);

                case ExContext.Wall: return DrawWall(plane, positionWithPointSensitivity, plane.GetNormal());

                case ExContext.Face: return DrawFace(hitObject as ExPoint);

                case ExContext.HelpPlane: return DrawHelpPlane(hitObject, relativeObject);

                default: return null;
            }
        }

        public DrawAction DrawPoint(
            WallInfo plane, 
            Vector3 position, 
            List<string> labels = null, 
            DrawType type = DrawType.Full
            )
        {
            var exPoint = DrawOrGetPoint(plane, position);

            labels?.ForEach(label => exPoint.AddLabel(label));

            OnDrawingCompleted(type);

            return null;
        }

        public DrawAction DrawFace(
            ExPoint chosenPoint
            )
        {
            if (chosenPoint == null)
            {
                var labels = _facePoints
                    .Select(p => p.FocusedLabel)
                    .Where(fl => !string.IsNullOrWhiteSpace(fl))
                    .Select(fl => fl.Replace("'", ""))
                    .Distinct()
                    .ToList();

                var points3DDict = _mB.GetPoints3D(); 

                var points3D = labels
                    .Where(lbl => points3DDict.ContainsKey(lbl))
                    .Select(lbl => new KeyValuePair<string, Vector3>(lbl, points3DDict[lbl]))
                    .ToList();

                DrawFace(points3D);

                _facePoints.ForEach(p => p.Color = ReconstructionInfo.NORMAL);
                _facePoints.Clear();                
            }
            else if(_facePoints.Contains(chosenPoint) || string.IsNullOrEmpty(chosenPoint.FocusedLabel))
            {
                chosenPoint.Color = ReconstructionInfo.NORMAL;
                _facePoints.Remove(chosenPoint);
            }
            else
            {
                chosenPoint.Color = ReconstructionInfo.MENTIONED;
                _facePoints.Add(chosenPoint);
            }

            return null;
        }

        public void DrawFace(
            List<KeyValuePair<string, Vector3>> points3D
            )
        {
            var face = new GameObject("FACE");
            face.transform.SetParent(_faceRepo.transform);

            var faceComponent = face.AddComponent<Face>();

            if (faceComponent.CreateFace(points3D))
            {
                _mB.AddFace(face, faceComponent);
                OnDrawingCompleted(DrawType.Full);
            }
            else
            {
                UnityEngine.Object.Destroy(face);
                Debug.LogWarning("Failed to create face.");
            }
        }

        private DrawAction DrawHelpPlane(
            IRaycastable firstItem,
            IRaycastable secondItem
            )
        {
            ExPoint point;
            Line line;

            if (firstItem is ExPoint && secondItem is Line)
            {
                point = firstItem as ExPoint;
                line = secondItem as Line;
            }
            else if (firstItem is Line && secondItem is ExPoint)
            {
                line = firstItem as Line;
                point = secondItem as ExPoint;
            }
            else
            {
                return null;
            }

            Vector3? a;
            Vector3? b;
            Vector3? p;

            p = _mB.GetPoint3DCoords(point.FocusedLabel);

            var boundPoints = line.GetLabelsOfBoundPoints();
            if (boundPoints.Count == 2)
            {
                a = _mB.GetPoint3DCoords(boundPoints[0]);
                b = _mB.GetPoint3DCoords(boundPoints[1]);
            }
            else
            {
                var ab = _mB.GetEdge3DCoords(line.FocusedLabel);
                a = ab?.Item1;
                b = ab?.Item2;
            }

            if (a == null || b == null || p == null)
                return null;

            var v = b.Value - a.Value;
            var vv = Vector3.Dot(v, v);

            if (vv < DescriptiveMathLib.EPS)
                return null;

            var t = Vector3.Dot(p.Value - a.Value, v) / vv;

            var c = p.Value + v * (1f - t);
            var d = p.Value - v * t;

            // Extend the rectangle if the point lies outside the opposite edge
            if (t < 0f)
            {
                a = a.Value + v * t;
                d = p.Value;
            }
            else if (t > 1f)
            {
                b = b.Value + v * (t - 1f);
                c = p.Value;
            }

            var points3D = new List<KeyValuePair<string, Vector3>>()
            {
                new KeyValuePair<string, Vector3>($"#{hiddenLabelId}_a", a.Value),
                new KeyValuePair<string, Vector3>($"#{hiddenLabelId}_b", b.Value),
                new KeyValuePair<string, Vector3>($"#{hiddenLabelId}_c", c),
                new KeyValuePair<string, Vector3>($"#{hiddenLabelId}_d", d)
            };

            hiddenLabelId++;

            DrawFace(points3D);

            return null;
        }

        public DrawAction DrawLine(
            WallInfo startPlane, 
            Vector3 startPosition, 
            ExPoint startPoint = null,
            LineRef lineReference = null,
            float lineWidth = _HELP_LINE_WIDTH, 
            List<string> labels = null, 
            DrawType type = DrawType.Full
            )
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
            _wCtrl.LinkConstructionToWall(startPlane, line);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = lineWidth;
            lineComponent.Draw(startPlane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;

            var intersectedObjs = new HashSet<IAnalyzable>();

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                var currentPlane = FindPlane(hitPlane, hitObject);

                if (startPlane != currentPlane)
                {
                    if (isEnd)
                    {
                        UnityEngine.Object.Destroy(line);
                        HandleIntersectionsAbort(intersectedObjs);
                    }

                    return;
                }
                    
                var cursorPosition = CalcPosition(startPlane, hitPosition, null);
                var endPosition = cursorPosition;

                if (_allowedOnlyMainDirections)
                {
                    var baseAxis = GetAxisWithTheEarliestWall(startPlane);
                    if (baseAxis == null)
                        return;

                    endPosition = SnapToAxisDirections(
                        startPosition, 
                        endPosition, 
                        baseAxis.From, 
                        baseAxis.To, 
                        startPlane.GetNormal()
                    );
                }
                else if (lineReference?.BeParallel ?? false)
                {
                    // Project the reference direction onto the drawing plane
                    var direction = Vector3.ProjectOnPlane(
                        lineReference.Line.EndPosition - lineReference.Line.StartPosition,
                        startPlane.GetNormal()
                    );

                    if (direction.sqrMagnitude < DescriptiveMathLib.EPS)
                        return;

                    direction.Normalize();

                    // Project the cursor onto the parallel direction
                    endPosition = startPosition + direction * Vector3.Dot(cursorPosition - startPosition, direction);
                }
                else if (lineReference?.BePerpendicular ?? false)
                {
                    // Calculate the perpendicular direction within the drawing plane
                    var direction = Vector3.Cross(
                        startPlane.GetNormal(),
                        lineReference.Line.EndPosition - lineReference.Line.StartPosition
                    );

                    if (direction.sqrMagnitude < DescriptiveMathLib.EPS)
                        return;

                    direction.Normalize();

                    // Project the cursor onto the perpendicular direction
                    endPosition = startPosition + direction * Vector3.Dot(cursorPosition - startPosition, direction);
                }

                if (lineReference?.TakeLength ?? false)
                {
                    var length = lineReference.Line.Length;
                    var direction = (endPosition - startPosition).normalized;
                    endPosition = startPosition + direction * length;
                }

                // add point sensitivity
                endPosition = CalcPosition(startPlane, endPosition, hitObject as ExPoint);

                lineComponent.Draw(default(WallInfo), default(Vector3), endPosition);
                HandleIntersectionStart(hitObject, intersectedObjs, line.GetComponent<IRaycastable>());
                
                if (isEnd)
                {
                    var endPoint = hitObject as ExPoint;

                    lineComponent.ColliderEnabled = true;
                    labels?.ForEach(label => lineComponent.AddLabel(label));
                    lineComponent.Color = ReconstructionInfo.NORMAL;

                    startPoint = DrawOrGetPoint(startPlane, startPosition);
                    endPoint = DrawOrGetPoint(hitPlane, endPosition);

                    if (startPoint != null && endPoint != null)
                    {
                        lineComponent.BindPoints(startPoint, endPoint);
                    }

                    HandleIntersectionsEnd(startPlane, intersectedObjs, lineComponent);

                    OnDrawingCompleted(type);
                }
            };
        }

        /// TODO Przeciecia sa niedostepne dla scian
        public DrawAction DrawWall(
            WallInfo plane, 
            Vector3 startPosition, 
            Vector3 wallParentNormal, 
            string fixedName = null, 
            DrawType type = DrawType.Full
            )
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
            _wCtrl.LinkConstructionToWall(plane, line);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = _HELP_LINE_WIDTH;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;
            //lineComponent.SetLabelVisible(true);

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                var endPositionWithPointSensitivity = CalcPosition(plane, hitPosition, hitObject as ExPoint);

                lineComponent.Draw(default(WallInfo), default(Vector3), endPositionWithPointSensitivity);
                // lineComponent.SetLabel(Vector3.Distance(startPosition, endPositionWithPointSensitivity));
                
                if (isEnd)
                {
                    UnityEngine.Object.DestroyImmediate(line);

                    var addedWall = _wCrt.WCrCreateWall(startPosition, endPositionWithPointSensitivity, wallParentNormal, plane.name, fixedName);
                    var axesCount = _axisRepo.transform.childCount;
                    addedWall.SetConstructionNumber(axesCount + 2);

                    AddAxisBetweenPlanes(plane, addedWall);

                    OnDrawingCompleted(type);
                }
            };
        }

        public DrawAction DrawCircle(
            WallInfo plane, 
            Vector3 startPosition, 
            float lineWidth = _HELP_LINE_WIDTH, 
            DrawType type = DrawType.Full
            )
        {
            var circle = new GameObject("CIRCLE");
            circle.transform.SetParent(_circleRepo.transform);
            _wCtrl.LinkConstructionToWall(plane, circle);

            var circleComponent = circle.AddComponent<Circle>();
            circleComponent.ColliderEnabled = false;
            circleComponent.Width = lineWidth;
            circleComponent.Draw(plane, startPosition, startPosition);
            
            var intersectedObjs = new HashSet<IAnalyzable>();

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                var endPositionWithPointSensitivity = CalcPosition(plane, hitPosition, hitObject as ExPoint);

                circleComponent.Draw(default(WallInfo), default(Vector3), endPositionWithPointSensitivity);

                HandleIntersectionStart(hitObject, intersectedObjs, circleComponent.GetComponent<IRaycastable>());

                if (isEnd)
                {
                    circleComponent.ColliderEnabled = true;
                    HandleIntersectionsEnd(plane, intersectedObjs, circleComponent);

                    OnDrawingCompleted(type);
                }
            };
        }

        public DrawAction DrawFixedProjection(
            WallInfo startPlane, 
            Vector3 startPosition,
            ExPoint startPoint = null,
            IRaycastable relativeObject = null
            )
        {
            var relativeLine = relativeObject as Line;
            var relativePoint = relativeObject as ExPoint;

            if (relativeLine != null)
            {
                var length = Vector3.Distance(relativeLine.StartPosition, relativeLine.EndPosition);
                return DrawProjection(startPlane, startPosition, startPoint, true, length);
            }

            if (relativePoint != null)
            {
                var pointPosition = relativePoint.Position;
                var pointPlane = relativePoint.Plane;

                var axis = GetAxis(startPlane, pointPlane);
                if (axis != null)
                {
                    var startProjectionPosition = CalcProjectionOnAxis(axis, relativePoint.Position);
                    var length = Vector3.Distance(startProjectionPosition, pointPosition);
                    return DrawProjection(startPlane, startPosition, startPoint, true, length);
                }
            }

            return null;
        }

        public DrawAction DrawProjection(
            WallInfo startPlane, 
            Vector3 startPosition,
            ExPoint startPoint = null,
            bool withFixedLength = false, 
            float fixedLength = 0.0f, 
            float lineWidth = _HELP_LINE_WIDTH
            )
        {
            // START BE A PLANE
            if (startPlane == default(WallInfo))
                return null;

            // MUST START AT POINT
            if (startPoint == null)
                return null;

            // FIRST PART
            var projection1 = new GameObject("PROJECTION");
            projection1.transform.SetParent(_lineRepo.transform);

            var projectionComponent1 = projection1.AddComponent<Line>();
            projectionComponent1.ColliderEnabled = false;
            projectionComponent1.EnabledLabels = true;
            projectionComponent1.Width = lineWidth;
            projectionComponent1.Draw(startPlane, startPosition, startPosition);

            var intersectedObjsPl1 = new HashSet<IAnalyzable>();

            // SECOND PART
            var projection2 = new GameObject("PROJECTION");
            projection2.transform.SetParent(_lineRepo.transform);

            var projectionComponent2 = projection2.AddComponent<Line>();
            projectionComponent2.ColliderEnabled = false;
            projectionComponent2.EnabledLabels = true;
            projectionComponent2.Width = lineWidth;
            projectionComponent2.Draw(startPlane, startPosition, startPosition);

            var intersectedObjsPl2 = new HashSet<IAnalyzable>();

            // AXIS
            var axis = default(Axis);

            // END PLANE
            var endPlane = default(WallInfo);

            // PROJECTION OF START POSITION ON AXIS
            var startPositionProjection = default(Vector3);

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                // CURRENT PLANE
                var currPlane = FindPlane(hitPlane, hitObject);
                if (currPlane == default(WallInfo))
                    return;

                Debug.Log($"CURR PLANE: {currPlane.constructionNumber}");

                if (startPlane == currPlane)
                {
                    // FIND AXIS
                    var axes = GetAxesOnPlane(startPlane);
                    axis = FindApproachingAxis(startPosition, hitPosition, axes);
                    if (axis == default(Axis))
                        return;

                    // THEN FIND END PLANE
                    endPlane = FindEndPlane(axis, startPlane);

                    // PROJECTION OF START POSITION ON AXIS
                    startPositionProjection = CalcProjectionOnAxis(axis, startPosition);

                    // CURRENT POSITION
                    var cursorPosition = CalcPosition(currPlane, hitPosition);

                    // PROJECTION OF CURRENT POSITION ON AXIS
                    var cursorPositionProjection = CalcProjectionOnAxis(axis, cursorPosition);

                    // DISPLACEMENT VECTOR
                    var vDisplacement = startPositionProjection - cursorPositionProjection;

                    // ACTUAL END POSITION
                    var endPosition = cursorPosition + vDisplacement;

                    // DRAWING
                    projectionComponent1.Draw(startPlane, startPosition, endPosition);
                    projectionComponent2.Draw(startPlane, startPosition, endPosition);
                    HandleIntersectionStart(hitObject, intersectedObjsPl1, projectionComponent1.GetComponent<IRaycastable>());

                    if (isEnd)
                    {
                        UnityEngine.Object.Destroy(projection1);
                        UnityEngine.Object.Destroy(projection2);
                        HandleIntersectionsAbort(intersectedObjsPl1);
                        HandleIntersectionsAbort(intersectedObjsPl2);
                    }
                }
                else if (endPlane == currPlane)
                {
                    // CURRENT POSITION
                    var cursorPosition = CalcPosition(currPlane, hitPosition);

                    // PROJECTION OF CURRENT POSITION ON AXIS
                    var cursorPositionProjection = CalcProjectionOnAxis(axis, cursorPosition);

                    // DISPLACEMENT VECTOR
                    var vDisplacement = startPositionProjection - cursorPositionProjection;

                    // ACTUAL END POSITION
                    var endPosition = cursorPosition + vDisplacement;

                    // FIXED LENGTH PROJECTION  
                    if (withFixedLength)
                    {
                        endPosition = ScaleToLength(startPositionProjection, endPosition, fixedLength);
                    }

                    // ENSURE END POSITION LIES ON END PLANE
                    endPosition = CastPointOnPlane(startPositionProjection, endPosition, currPlane);

                    // DRAWING
                    projectionComponent1.Draw(startPlane, startPosition, startPositionProjection);
                    projectionComponent2.Draw(endPlane, endPosition, startPositionProjection);
                    HandleIntersectionStart(hitObject, intersectedObjsPl2, projectionComponent2.GetComponent<IRaycastable>());

                    if (isEnd)
                    {
                        var pointLabel = startPoint?.FocusedLabel;
                        var labels = string.IsNullOrEmpty(pointLabel) ? null : new List<string>() { startPoint.FocusedLabel };

                        DrawPoint(endPlane, endPosition, labels, DrawType.Part);

                        projectionComponent1.ColliderEnabled = true;
                        _wCtrl.LinkConstructionToWall(startPlane, projection1);

                        projectionComponent2.ColliderEnabled = true;
                        _wCtrl.LinkConstructionToWall(endPlane, projection2);

                        HandleIntersectionsEnd(startPlane, intersectedObjsPl1, projectionComponent1);
                        HandleIntersectionsEnd(endPlane, intersectedObjsPl2, projectionComponent2);

                        OnDrawingCompleted(DrawType.Full);
                    }
                }
                else
                {
                    if (isEnd)
                    {
                        UnityEngine.Object.Destroy(projection1);
                        UnityEngine.Object.Destroy(projection2);
                        HandleIntersectionsAbort(intersectedObjsPl1);
                        HandleIntersectionsAbort(intersectedObjsPl2);
                    }
                }
            };
        }

        //---

        public static List<ExPoint> GetPoints() => GetComponentsFromRepo<ExPoint>(_pointRepo);

        public static List<Line> GetLines() => GetComponentsFromRepo<Line>(_lineRepo);

        public static List<Circle> GetCircles() => GetComponentsFromRepo<Circle>(_circleRepo);

        public static List<Axis> GetAxes() => GetComponentsFromRepo<Axis>(_axisRepo);

        public static List<WallInfo> GetWalls() => _ic?._wCtrl.GetWalls() ?? new List<WallInfo>();

        public static List<Face> GetFaces() => GetComponentsFromRepo<Face>(_faceRepo);

        public static void AddPoint(
            List<string> pointLabels, 
            string pointPlaneName, 
            Vector3 pointPosition
            )
        {
            _ic?.DrawPoint(_ic._wCtrl.GetWallByName(pointPlaneName), pointPosition, pointLabels, DrawType.Part);
        }

        public static void AddLine(
            List<string> lineBoundPointsByLabel, 
            Vector3 lineEndPosition, 
            List<string> lineLabels, 
            float lineLineWidth, 
            string linePlaneName, 
            Vector3 lineStartPosition
            )
        {
            if (_ic == null) return;

            var plane = _ic._wCtrl.GetWallByName(linePlaneName);

            var startPointLabel = lineBoundPointsByLabel.ElementAtOrDefault(0);
            var endPointLabel = lineBoundPointsByLabel.ElementAtOrDefault(1);

            ExPoint startPoint = null;
            ExPoint endPoint = null;

            if (startPointLabel != default(string) && endPointLabel != default(string))
            {
                foreach (Transform pointTrans in _pointRepo.transform)
                {
                    var point = pointTrans.gameObject.GetComponent<ExPoint>();
                    if (point == null)
                        continue;

                    if (point.Plane.Equals(plane) && point.Labels.Contains(startPointLabel))
                    {
                        startPoint = point;

                        while (startPoint.FocusedLabel != startPointLabel)
                            startPoint.NextLabel();
                    }

                    if (point.Plane.Equals(plane) && point.Labels.Contains(endPointLabel))
                    {
                        endPoint = point;

                        while (endPoint.FocusedLabel != endPointLabel)
                            endPoint.NextLabel();
                    }
                }
            }

            var da = _ic.DrawLine(plane, lineStartPosition, startPoint, null, lineLineWidth, lineLabels, DrawType.Part);
            da.Invoke(endPoint, lineEndPosition, plane, true);
        }

        public static void AddCircle(
            Vector3 circleEndPosition, 
            float circleLineWidth, 
            string circlePlaneName, 
            Vector3 circleStartPosition
            )
        {
            if (_ic == null) return;

            var plane = _ic._wCtrl.GetWallByName(circlePlaneName);

            var da = _ic.DrawCircle(plane, circleStartPosition, circleLineWidth, DrawType.Part);
            da.Invoke(null, circleEndPosition, plane, true);
        }

        public static void AddWall(
            Vector3? wallConstPoint1, 
            Vector3? wallConstPoint2, 
            Vector3? wallParentNormal, 
            string wallParentWallName, 
            string wallWallName
            )
        {
            if (_ic == null) return;
            if (wallConstPoint1 == null) return;
            if (wallConstPoint2 == null) return;

            var plane = _ic._wCtrl.GetWallByName(wallParentWallName);

            if (plane == null)
            {
                var addedWall = _ic._wCrt.WCrCreateWall((Vector3)wallConstPoint1, (Vector3)wallConstPoint2, (Vector3)wallParentNormal, null, wallWallName);
            }
            else
            {
                var da = _ic.DrawWall(plane, (Vector3)wallConstPoint1, (Vector3)wallParentNormal, wallWallName, DrawType.Part);
                da.Invoke(null, (Vector3)wallConstPoint2, plane, true);
            }
        }
 
        public static void AddFace(
            List<KeyValuePair<string, Vector3>> faceVertices
            )
        {
            _ic?.DrawFace(faceVertices);
        }

        //---

        public void Clear(
            bool withAxis = true
            )
        {
            if (withAxis)
            {
                _axisWalls.Clear();

                UnityEngine.Object.DestroyImmediate(_axisRepo);
                _axisRepo = new GameObject("AxisRepo");
                _axisRepo.transform.SetParent(_workspace.transform);
            }

            UnityEngine.Object.DestroyImmediate(_circleRepo);
            _circleRepo = new GameObject("CircleRepo");
            _circleRepo.transform.SetParent(_workspace.transform);

            UnityEngine.Object.DestroyImmediate(_lineRepo);
            _lineRepo = new GameObject("LineRepo");
            _lineRepo.transform.SetParent(_workspace.transform);

            UnityEngine.Object.DestroyImmediate(_pointRepo);
            _pointRepo = new GameObject("PointRepo");
            _pointRepo.transform.SetParent(_workspace.transform);

            UnityEngine.Object.DestroyImmediate(_faceRepo);
            _faceRepo = new GameObject("PointRepo");
            _faceRepo.transform.SetParent(_workspace.transform);
        }

        public void RemoveWorkspace()
        {
            UnityEngine.Object.DestroyImmediate(_workspace);
            _workspace = new GameObject("WorkspaceExp");
        }

        public void ToggleOnlyMainDirectionsDrawMode()
        {
            _allowedOnlyMainDirections = !_allowedOnlyMainDirections;
        }

        public void TurnOffOnlyMainDirectionsDrawMode()
        {
            _allowedOnlyMainDirections = false;
        }
    }
}
