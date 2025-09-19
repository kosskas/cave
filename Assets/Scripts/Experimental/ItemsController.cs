using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Assets.Scripts.Experimental;
using Assets.Scripts.Experimental.Items;
using UnityEngine;
using UnityEngine.Tizen;

namespace Assets.Scripts.Experimental
{
    public class ItemsController
    {
        private WallController _wc;
        private WallCreator _wcrt;
        private FacesGenerator _fc;
        
        private const float _WALL_HALF_WIDTH = 0.05f;
        private const float _WALL_HALF_LENGTH = 1.7f;
        private const float _OFFSET_FROM_WALL = 0.01f;
            
        private const float _HELP_LINE_WIDTH = 0.0035f;
        private const float _BOLD_LINE_WIDTH = 0.008f;

        private readonly GameObject _workspace;
        private GameObject _axisRepo;
        private GameObject _lineRepo;
        private GameObject _pointRepo;
        private GameObject _circleRepo;

        private Dictionary<Axis, Tuple<WallInfo, WallInfo>> _axisWalls;


        /* PRIVATE METHODS */

        private Vector3 CalcProjectionOnAxis(Axis axis, Vector3 point)
        {
            if (axis == null)
            {
                Debug.LogError("Brak osi odniesienia");
                return Vector3.zero;
            }
            return CalcProjectionOnAxis(axis.From, axis.To, point);
        }

        private Vector3 CalcProjectionOnAxis(Vector3 startPoint, Vector3 endPoint, Vector3 point)
        {
            Vector3 A = endPoint - startPoint;
            Vector3 B = point - startPoint;

            // B*A = |B||A|cos(a) = |B||1|cos(a) = |B|cos(a) = distance from axis.To to point projection on axis 
            float projectionLength = Vector3.Dot(B, A.normalized);

            Vector3 projectionPoint = startPoint + projectionLength * A.normalized;

            return projectionPoint;
        }

        private Axis GetAxis(WallInfo plane)
        {
            var axis = _axisWalls.FirstOrDefault(e => e.Value.Item1.Equals(plane) || e.Value.Item2.Equals(plane)).Key;
            return axis;
        }

        private List<Axis> GetAllAxes(WallInfo plane)
        {
            var axes = _axisWalls
                .Where(e => e.Value.Item1.Equals(plane) || e.Value.Item2.Equals(plane))
                .Select(e => e.Key)
                .ToList();
            return axes;
        }

        /// <summary>
        /// Zwraca oœ, w któr¹ najbardziej "wskazuje" strza³ka (lineFrom -> lineTo).
        /// Kryterium: najwiêkszy dodatni cos(k¹ta) pomiêdzy kierunkiem line,
        /// a wektorem od lineFrom do najbli¿szego punktu na osi.
        /// </summary>
        private Axis FindApproachingAxis(Vector3 lineFrom, Vector3 lineTo, List<Axis> axes)
        {
            var dirLine = (lineTo - lineFrom).normalized;

            var bestAxis = default(Axis);
            var bestDot = 0f;

            foreach (var axis in axes)
            {
                var axisPoint = CalcProjectionOnAxis(axis, lineFrom);
                var toAxis = (axisPoint - lineFrom).normalized;

                // cosinus k¹ta: 1 = idealnie w tê stronê, 0 = prostopadle, <0 = „za plecami”
                var dot = Vector3.Dot(dirLine, toAxis);

                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestAxis = axis;
                }
            }

            // mo¿e byæ null, jeœli wszystkie s¹ „za plecami” (dot <= minDot)
            return bestAxis;
        }

        private WallInfo FindPlane(WallInfo hitPlane, IRaycastable hitObject)
        {
            return hitPlane ?? (hitObject as IDrawable)?.Plane;
        }

        private WallInfo FindEndPlane(Axis axis, WallInfo startPlane)
        {
            var planes = _axisWalls[axis];
            var plane1 = planes.Item1;
            var plane2 = planes.Item2;

            return (plane1 == startPlane) ? plane2 : plane1;
        }

        private Vector3 CalcPosition(WallInfo plane, Vector3 hitPosition, ExPoint hitPoint = null)
        {
            var planeNormal = plane.GetNormal();
            var planePosition = plane.gameObject.transform.position;

            var position = Vector3.zero;

            if (Mathf.Approximately(Mathf.Abs(planeNormal.x), 1))
                position.x = planePosition.x + planeNormal.x * (_WALL_HALF_WIDTH + _OFFSET_FROM_WALL);
            else
                position.x = hitPosition.x;

            if (Mathf.Approximately(Mathf.Abs(planeNormal.y), 1))
                position.y = planePosition.y + planeNormal.y * (_WALL_HALF_WIDTH + _OFFSET_FROM_WALL);
            else
                position.y = hitPosition.y;

            if (Mathf.Approximately(Mathf.Abs(planeNormal.z), 1))
                position.z = planePosition.z + planeNormal.z * (_WALL_HALF_WIDTH + _OFFSET_FROM_WALL);
            else
                position.z = hitPosition.z;

            if (hitPoint != null)
                position = hitPoint.Position;

            // Debug.Log($"hitPosition=({hitPosition.x:F9}, {hitPosition.y:F9}, {hitPosition.z:F9}) " +
            //           $"position=({position.x:F9}, {position.y:F9}, {position.z:F9}) " +
            //           $"planeNormal=({planeNormal.x:F9}, {planeNormal.y:F9}, {planeNormal.z:F9}) " +
            //           $"planePosition=({planePosition.x:F9}, {planePosition.y:F9}, {planePosition.z:F9})");

            return position;
        }

        /* PUBLIC METHODS */

        public ItemsController(WallController wc, WallCreator wcrt, FacesGenerator fc)
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

            _axisWalls = new Dictionary<Axis, Tuple<WallInfo, WallInfo>>();

            _wc = wc;
            _wcrt = wcrt;
            _fc = fc;
        }

        public void AddAxisBetweenPlanes(WallInfo planeA, WallInfo planeB)
        {
            Vector3 normalA = planeA.GetNormal();
            Vector3 normalB = planeB.GetNormal();

            Vector3 positionA = planeA.gameObject.transform.position;
            Vector3 positionB = planeB.gameObject.transform.position;

            Vector3 offsetVector = (normalA + normalB) * (_WALL_HALF_WIDTH + _OFFSET_FROM_WALL);

            Vector3 direction = Vector3.Cross(normalA, normalB);

            Vector3 intersectionMiddlePoint = positionA - _WALL_HALF_LENGTH * normalB;

            Vector3 from = (intersectionMiddlePoint - direction) + offsetVector;
            Vector3 to = (intersectionMiddlePoint + direction) + offsetVector;

            var axis = new GameObject("AXIS");
            axis.transform.SetParent(_axisRepo.transform);

            var axisComponent = axis.AddComponent<Axis>();
            axisComponent.Draw(default(WallInfo), from, to);

            var labelComponent = axis.AddComponent<IndexedLabel>();
            labelComponent.AddLabel("X", "", $"{planeA.numberExp}/{planeB.numberExp}");
            labelComponent.FontSize = 1;

            _axisWalls.Add(axisComponent, new Tuple<WallInfo, WallInfo>(planeA, planeB));
            _wc.LinkConstructionToWall(planeA, axis);
            _wc.LinkConstructionToWall(planeB, axis);
        }

        public void AddAxisBetweenPlanes2(WallInfo planeA, WallInfo planeB)
        {
            Vector3 normalA = planeA.GetNormal();
            Vector3 normalB = planeB.GetNormal();

            Vector3 positionA = planeA.gameObject.transform.position;
            Vector3 positionB = planeB.gameObject.transform.position;

            // Calculate the intersection point of the two planes
            Vector3 direction = Vector3.Cross(normalA, normalB);

            // Solve for the intersection point using plane equations
            float determinant = Vector3.Dot(direction, direction);
            if (Mathf.Approximately(determinant, 0))
            {
                Debug.LogError("Planes are parallel and do not intersect.");
                return;
            }

            Vector3 intersectionMiddlePoint = positionA + Vector3.Project(positionB - positionA, direction);

            Vector3 from = (intersectionMiddlePoint - direction.normalized * _WALL_HALF_LENGTH);
            Vector3 to = (intersectionMiddlePoint + direction.normalized * _WALL_HALF_LENGTH);

            var axis = new GameObject("AXIS");
            axis.transform.SetParent(_axisRepo.transform);

            var axisComponent = axis.AddComponent<Axis>();
            axisComponent.Draw(default(WallInfo), from, to);

            var labelComponent = axis.AddComponent<IndexedLabel>();
            labelComponent.AddLabel("X", "", $"{planeA.numberExp}/{planeB.numberExp}");
            labelComponent.FontSize = 1;

            _axisWalls.Add(axisComponent, new Tuple<WallInfo, WallInfo>(planeA, planeB));
            _wc.LinkConstructionToWall(planeA, axis);
            _wc.LinkConstructionToWall(planeB, axis);
        }
        public void RemoveLastAxis()
        {
            if (_axisWalls != null && _axisWalls.Count > 0 && _wc != null)
            {
                var deletingWall = _wc.GetLastAddedWall();
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
            Line relativeLine = null)
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
                case ExContext.Idle: return null;

                case ExContext.Point: return DrawPoint(plane, position);

                case ExContext.BoldLine: return DrawLine(plane, positionWithPointSensitivity, hitObject as ExPoint, _BOLD_LINE_WIDTH);
                
                case ExContext.Line: return DrawLine(plane, positionWithPointSensitivity);

                case ExContext.PerpendicularLine: return DrawLinePerpendicularToLine(plane, positionWithPointSensitivity, relativeLine);

                case ExContext.ParallelLine: return DrawLineParallelToLine(plane, positionWithPointSensitivity, relativeLine);

                case ExContext.Circle: return DrawCircle(plane, positionWithPointSensitivity);

                case ExContext.Projection: return DrawProjection(plane, positionWithPointSensitivity);

                case ExContext.Wall: return DrawWall(plane, positionWithPointSensitivity);

                default: return null;
            }
        }

        public DrawAction DrawPoint(WallInfo plane, Vector3 position)
        {
            var point = new GameObject("POINT");
            point.transform.SetParent(_pointRepo.transform);
            _wc.LinkConstructionToWall(plane, point);

            var pointComponent = point.AddComponent<ExPoint>();
            pointComponent.Draw(plane, position);
            pointComponent.EnabledLabels = true;
            
            return null;
        }

        public DrawAction DrawLine(WallInfo plane, Vector3 startPosition, ExPoint startPoint = null, float lineWidth = _HELP_LINE_WIDTH)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
            _wc.LinkConstructionToWall(plane, line);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = lineWidth;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;
            lineComponent.SetLabelVisible(true);

            var collidedLines = new HashSet<IAnalyzable>();

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                var endPositionWithPointSensitivity = CalcPosition(plane, hitPosition, hitObject as ExPoint);

                lineComponent.Draw(default(WallInfo), default(Vector3), endPositionWithPointSensitivity);

                lineComponent.SetLabel(Vector3.Distance(startPosition, endPositionWithPointSensitivity));

                // Check for collision with other lines or circles
                if (hitObject is IAnalyzable)
                {
                    collidedLines.Add((IAnalyzable)hitObject);
                }
                
                if (isEnd)
                {
                    lineComponent.ColliderEnabled = true;
                    lineComponent.SetLabelVisible(false);
                    var endPoint = hitObject as ExPoint;

                    if (startPoint != null && endPoint != null)
                    {
                        lineComponent.BindPoints(startPoint, endPoint);
                    }

                    foreach (var vaAnalyzable in collidedLines)
                    {
                        List<Vector3> found = vaAnalyzable.FindCrossingPoints(lineComponent);
                        if (found != null)
                        {
                            DrawPoint(plane, found[0]);
                        }
                    }
                }
            };
        }

        public DrawAction DrawWall(WallInfo plane, Vector3 startPosition)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
            _wc.LinkConstructionToWall(plane, line);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = _HELP_LINE_WIDTH;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;
            lineComponent.SetLabelVisible(true);

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                var endPositionWithPointSensitivity = CalcPosition(plane, hitPosition, hitObject as ExPoint);

                lineComponent.Draw(default(WallInfo), default(Vector3), endPositionWithPointSensitivity);
                lineComponent.SetLabel(Vector3.Distance(startPosition, endPositionWithPointSensitivity));

                if (isEnd)
                {
                    UnityEngine.Object.Destroy(line);

                    var addedWall = _wcrt.WCrCreateWall(startPosition, endPositionWithPointSensitivity, plane);
                    AddAxisBetweenPlanes2(addedWall, plane);
                }
            };
        }

        public DrawAction DrawCircle(WallInfo plane, Vector3 startPosition, float lineWidth = _HELP_LINE_WIDTH)
        {
            var circle = new GameObject("CIRCLE");
            circle.transform.SetParent(_circleRepo.transform);
            _wc.LinkConstructionToWall(plane, circle);

            var circleComponent = circle.AddComponent<Circle>();
            circleComponent.ColliderEnabled = false;
            circleComponent.Width = lineWidth;
            circleComponent.Draw(plane, startPosition, startPosition);

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                var endPositionWithPointSensitivity = CalcPosition(plane, hitPosition, hitObject as ExPoint);

                circleComponent.Draw(default(WallInfo), default(Vector3), endPositionWithPointSensitivity);

                if (isEnd)
                    circleComponent.ColliderEnabled = true;
            };
        }

        public DrawAction DrawProjection(WallInfo startPlane, Vector3 startPosition, float lineWidth = _HELP_LINE_WIDTH)
        {
            // FIRST PART
            var projection1 = new GameObject("PROJECTION");
            projection1.transform.SetParent(_lineRepo.transform);

            var projectionComponent1 = projection1.AddComponent<Line>();
            projectionComponent1.ColliderEnabled = false;
            projectionComponent1.EnabledLabels = true;
            projectionComponent1.Width = lineWidth;
            projectionComponent1.Draw(startPlane, startPosition, startPosition);
            projectionComponent1.SetLabelVisible(true);

            // SECOND PART
            var projection2 = new GameObject("PROJECTION");
            projection2.transform.SetParent(_lineRepo.transform);

            var projectionComponent2 = projection2.AddComponent<Line>();
            projectionComponent2.ColliderEnabled = false;
            projectionComponent2.EnabledLabels = true;
            projectionComponent2.Width = lineWidth;
            projectionComponent2.Draw(startPlane, startPosition, startPosition);

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (startPlane == default(WallInfo))
                    return;

                // CHOOSE AXIS
                var axes = GetAllAxes(startPlane);
                var axis = FindApproachingAxis(startPosition, hitPosition, axes);
                if (axis == default(Axis))
                    return;

                var currPlane = FindPlane(hitPlane, hitObject);
                var endPlane = FindEndPlane(axis, startPlane);

                var startPositionProjection = CalcProjectionOnAxis(axis, startPosition);
                var cursorPosition = CalcPosition(endPlane, hitPosition);

                if (startPlane == currPlane)
                {
                    var endPosition = CalcProjectionOnAxis(startPosition, startPositionProjection, cursorPosition);

                    projectionComponent1.Draw(default(WallInfo), default(Vector3), endPosition);
                    projectionComponent2.Draw(default(WallInfo), startPosition, endPosition);

                    projectionComponent1.SetLabel(Vector3.Distance(startPosition, endPosition));
                    projectionComponent1.SetLabelVisible(true);
                    projectionComponent2.SetLabelVisible(false);

                    if (isEnd)
                    {
                        UnityEngine.Object.Destroy(projection1);
                        UnityEngine.Object.Destroy(projection2);
                    }
                }
                else
                {
                    var cursorPositionProjection = CalcProjectionOnAxis(axis, cursorPosition);
                    var alignment = startPositionProjection - cursorPositionProjection;
                    var endPosition = cursorPosition + alignment;
                    
                    projectionComponent2.Draw(endPlane, endPosition, default(Vector3));

                    projectionComponent2.SetLabel(Vector3.Distance(startPositionProjection, endPosition));
                    projectionComponent1.SetLabelVisible(false);
                    projectionComponent2.SetLabelVisible(true);

                    if (isEnd)
                    {
                        projectionComponent1.Draw(default(WallInfo), default(Vector3), startPositionProjection);
                        projectionComponent2.Draw(default(WallInfo), default(Vector3), startPositionProjection);

                        DrawPoint(endPlane, endPosition);

                        projectionComponent1.ColliderEnabled = true;
                        projectionComponent2.ColliderEnabled = true;
                        projectionComponent1.SetLabelVisible(false);
                        projectionComponent2.SetLabelVisible(false);
                        _wc.LinkConstructionToWall(startPlane, projection1);
                        _wc.LinkConstructionToWall(endPlane, projection2);
                    }
                }
            };
        }

        public DrawAction DrawLineParallelToLine(WallInfo plane, Vector3 startPosition, Line relativeLine = null)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
            _wc.LinkConstructionToWall(plane, line);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = _HELP_LINE_WIDTH;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;
            lineComponent.SetLabelVisible(true);

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                if (relativeLine == null)
                    return;

                var startPositionProjection = CalcProjectionOnAxis(relativeLine.StartPosition, relativeLine.EndPosition, startPosition);
                var startPositionOffsetFromAxis = startPosition - startPositionProjection;

                var cursorPosition = CalcPosition(plane, hitPosition);

                var cursorPositionProjection = CalcProjectionOnAxis(relativeLine.StartPosition, relativeLine.EndPosition, cursorPosition);

                var endPosition = cursorPositionProjection + startPositionOffsetFromAxis;

                lineComponent.Draw(default(WallInfo), default(Vector3), endPosition);
                lineComponent.SetLabel(Vector3.Distance(startPosition, endPosition));

                if (isEnd)
                {
                    lineComponent.ColliderEnabled = true;
                    lineComponent.SetLabelVisible(false);
                }
            };
        }

        public DrawAction DrawLinePerpendicularToLine(WallInfo plane, Vector3 startPosition, Line relativeLine = null)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
            _wc.LinkConstructionToWall(plane, line);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = _HELP_LINE_WIDTH;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;
            lineComponent.SetLabelVisible(true);

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                if (relativeLine == null)
                    return;

                var cursorPosition = CalcPosition(plane, hitPosition);

                var vRelativeLine = relativeLine.EndPosition - relativeLine.StartPosition;
                var vDrawnLine = cursorPosition - startPosition;

                if (vDrawnLine.magnitude < 1e-8f)
                    return; // vDrawnLine zdegenerowany

                if (vRelativeLine.sqrMagnitude < 1e-12f) 
                    return; // vRelativeLine zdegenerowany

                var dirRelativeLine = vRelativeLine.normalized;

                // Usuñ sk³adow¹ vDrawnLine wzd³u¿ vRelativeLine (rzut na p³aszczyznê prostopad³¹ do vRelativeLine)
                var vDrawnLinePerpendicular = vDrawnLine - Vector3.Dot(vDrawnLine, dirRelativeLine) * dirRelativeLine;

                if (vDrawnLinePerpendicular.sqrMagnitude < 1e-12f)
                    return; // gdy vDrawnLine by³ równoleg³y do vRelativeLine

                vDrawnLinePerpendicular = vDrawnLinePerpendicular.normalized * vDrawnLine.magnitude;

                var endPosition = startPosition + vDrawnLinePerpendicular;

                lineComponent.Draw(default(WallInfo), default(Vector3), endPosition);
                lineComponent.SetLabel(Vector3.Distance(startPosition, endPosition));

                if (isEnd)
                {
                    lineComponent.ColliderEnabled = true;
                    lineComponent.SetLabelVisible(false);
                }
            };
        }

        //---

        public void Save()
        {
            //walls i faces sa juz zapisane
            StorePoints();
            StoreLines();
            StoreCircles();

            StateManager.Exp.Save();
        }
        private void StoreWalls()
        {
            foreach (Transform pointTrans in _pointRepo.transform)
            {
                var point = pointTrans.gameObject.GetComponent<ExPoint>();
                if (point == null)
                    continue;

                var planeName = point.Plane.name;
                var position = point.Position;
                var labels = point.Labels;

                StateManager.Exp.StorePoint(planeName, position, labels);
            }
        }
        private void StorePoints()
        {
            foreach (Transform pointTrans in _pointRepo.transform)
            {
                var point = pointTrans.gameObject.GetComponent<ExPoint>();
                if (point == null)
                    continue;

                var planeName = point.Plane.name;
                var position = point.Position;
                var labels = point.Labels;

                StateManager.Exp.StorePoint(planeName, position, labels);
            }
        }

        private void StoreLines()
        {
            foreach (Transform lineTrans in _lineRepo.transform)
            {
                var line = lineTrans.gameObject.GetComponent<Line>();
                if (line == null)
                    continue;

                var planeName = line.Plane.name;
                var startPosition = line.StartPosition;
                var endPosition = line.EndPosition;
                var boundPoints = line.GetLabelsOfBoundPoints();
                var labels = line.Labels;
                var lineWidth = line.Width;

                StateManager.Exp.StoreLine(planeName, startPosition, endPosition, boundPoints, labels, lineWidth);
            }
        }

        private void StoreCircles()
        {
            foreach (Transform circleTrans in _circleRepo.transform)
            {
                var circle = circleTrans.gameObject.GetComponent<Circle>();
                if (circle == null)
                    continue;

                var planeName = circle.Plane.name;
                var startPosition = circle.StartPosition;
                var endPosition = circle.EndPosition;
                var lineWidth = circle.Width;

                StateManager.Exp.StoreCircle(planeName, startPosition, endPosition, lineWidth);
            }
        }

        //---

        public void Clear(bool withAxis = true)
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
        }

        //---

        public void Restore()
        {
            Clear(withAxis: false);
            StateManager.Exp.Load();
            StateManager.Exp.RestoreWalls(_wcrt);
            RestorePoints();
            RestoreLines();
            RestoreCircles();
            StateManager.Exp.RestoreFaces(_fc);
        }

        private void RestorePoints()
        {
            StateManager.Exp.RestorePoints((plane, position, labels) =>
            {
                var point = new GameObject("POINT");
                point.transform.SetParent(_pointRepo.transform);
                _wc.LinkConstructionToWall(plane, point);
                var pointComponent = point.AddComponent<ExPoint>();
                pointComponent.Draw(plane, position);
                pointComponent.EnabledLabels = true;

                labels.ForEach(label => pointComponent.AddLabel(label));
            });
        }

        private void RestoreLines()
        {
            StateManager.Exp.RestoreLines((plane, startPosition, endPosition, boundPointsByLabel, labels, lineWidth) =>
            {
                var line = new GameObject("LINE");
                line.transform.SetParent(_lineRepo.transform);
                _wc.LinkConstructionToWall(plane, line);
                var lineComponent = line.AddComponent<Line>();
                lineComponent.Width = lineWidth;
                lineComponent.Draw(plane, startPosition, endPosition);
                lineComponent.EnabledLabels = true;
                lineComponent.ColliderEnabled = true;
            
                labels.ForEach(label =>
                {
                    float tmp;
                    if (float.TryParse(label, out tmp))
                        return;

                    lineComponent.AddLabel(label);
                });

                var startPointLabel = boundPointsByLabel.ElementAtOrDefault(0);
                var endPointLabel = boundPointsByLabel.ElementAtOrDefault(1);

                if (startPointLabel == default(string) || endPointLabel == default(string))
                    return;

                var startPoint = default(ExPoint);
                var endPoint = default(ExPoint);
                foreach (Transform pointTrans in _pointRepo.transform)
                {
                    var point = pointTrans.gameObject.GetComponent<ExPoint>();
                    if (point == null)
                        continue;

                    if (point.Plane.Equals(plane) && point.Labels.Contains(startPointLabel))
                        startPoint = point;

                    if (point.Plane.Equals(plane) && point.Labels.Contains(endPointLabel))
                        endPoint = point;
                }

                if (startPoint == default(ExPoint) || endPoint == default(ExPoint))
                    return;

                lineComponent.BindPoints(startPoint, startPointLabel, endPoint, endPointLabel);
            });
        }

        private void RestoreCircles()
        {
            StateManager.Exp.RestoreCircles((plane, startPosition, endPosition, lineWidth) =>
            {
                var circle = new GameObject("CIRCLE");
                circle.transform.SetParent(_circleRepo.transform);
                _wc.LinkConstructionToWall(plane, circle);
                var circleComponent = circle.AddComponent<Circle>();
                circleComponent.Draw(plane, startPosition, endPosition);
                circleComponent.ColliderEnabled = true;
            });
        }
    }
}
