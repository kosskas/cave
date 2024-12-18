using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using Assets.Scripts.Experimental;
using Assets.Scripts.Experimental.Items;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public class ItemsController
    {
        private const float _WALL_HALF_WIDTH = 0.05f;
        private const float _WALL_HALF_LENGTH = 1.7f;
        private const float _OFFSET_FROM_WALL = 0.01f;
            
        private const float _HELP_LINE_WIDTH = 0.002f;
        private const float _BOLD_LINE_WIDTH = 0.005f;

        private readonly GameObject _workspace;
        private GameObject _axisRepo;
        private GameObject _lineRepo;
        private GameObject _pointRepo;
        private GameObject _circleRepo;

        private readonly Dictionary<Axis, Tuple<WallInfo, WallInfo>> _axisWalls;


        /* PRIVATE METHODS */

        private Vector3 CalcProjectionOnAxis(Axis axis, Vector3 point)
        {
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

        private WallInfo FindPlane(WallInfo hitPlane, IRaycastable hitObject)
        {
            return hitPlane ?? (hitObject as IDrawable)?.Plane;
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

        public ItemsController()
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
            labelComponent.AddLabel("X", "", $"{planeA.number}{planeB.number}");
            labelComponent.FontSize = 1;

            _axisWalls.Add(axisComponent, new Tuple<WallInfo, WallInfo>(planeA, planeB));
        }

        public DrawAction Add(
            ExContext context, 
            IRaycastable hitObject,
            Vector3 hitPosition, 
            WallInfo hitPlane)
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

                case ExContext.PerpendicularLine: return DrawLinePerpendicularToAxis(plane, positionWithPointSensitivity);

                case ExContext.ParallelLine: return DrawLineParallelToAxis(plane, positionWithPointSensitivity);

                case ExContext.Circle: return DrawCircle(plane, positionWithPointSensitivity);

                case ExContext.Projection: return DrawProjection(plane, positionWithPointSensitivity);

                default: return null;
            }
        }

        public DrawAction DrawPoint(WallInfo plane, Vector3 position)
        {
            var point = new GameObject("POINT");
            point.transform.SetParent(_pointRepo.transform);

            var pointComponent = point.AddComponent<ExPoint>();
            pointComponent.Draw(plane, position);
            pointComponent.EnabledLabels = true;

            return null;
        }

        public DrawAction DrawLine(WallInfo plane, Vector3 startPosition, ExPoint startPoint = null, float lineWidth = _HELP_LINE_WIDTH)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = lineWidth;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                var endPositionWithPointSensitivity = CalcPosition(plane, hitPosition, hitObject as ExPoint);

                lineComponent.Draw(default(WallInfo), default(Vector3), endPositionWithPointSensitivity);

                if (isEnd)
                {
                    lineComponent.ColliderEnabled = true;

                    var endPoint = hitObject as ExPoint;

                    if (startPoint != null && endPoint != null)
                        lineComponent.BindPoints(startPoint, endPoint);
                }
            };
        }

        public DrawAction DrawCircle(WallInfo plane, Vector3 startPosition, float lineWidth = _HELP_LINE_WIDTH)
        {
            var circle = new GameObject("CIRCLE");
            circle.transform.SetParent(_circleRepo.transform);

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
            // START POINT PROJECTION ON AXIS
            var axis = GetAxis(startPlane);
            var startPositionProjection = CalcProjectionOnAxis(axis, startPosition);

            // FIRST PART
            var projection1 = new GameObject("PROJECTION");
            projection1.transform.SetParent(_lineRepo.transform);

            var projectionComponent1 = projection1.AddComponent<Line>();
            projectionComponent1.ColliderEnabled = false;
            projectionComponent1.Width = 0.002f;
            projectionComponent1.Draw(startPlane, startPosition, startPosition);

            // SECOND PART
            var projection2 = new GameObject("PROJECTION");
            projection2.transform.SetParent(_lineRepo.transform);

            var projectionComponent2 = projection2.AddComponent<Line>();
            projectionComponent2.ColliderEnabled = false;
            projectionComponent2.Width = 0.002f;
            projectionComponent2.Draw(startPlane, startPosition, startPosition);

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (startPlane == default(WallInfo))
                    return;

                var endPlane = FindPlane(hitPlane, hitObject);
                var cursorPosition = CalcPosition(endPlane, hitPosition);

                if (startPlane == endPlane)
                {
                    var endPosition = CalcProjectionOnAxis(startPosition, startPositionProjection, cursorPosition);

                    projectionComponent1.Draw(default(WallInfo), default(Vector3), endPosition);
                    projectionComponent2.Draw(default(WallInfo), startPosition, endPosition);

                    if (isEnd)
                    {
                        projectionComponent1.Draw(default(WallInfo), default(Vector3), startPositionProjection);
                        projectionComponent1.ColliderEnabled = true;

                        UnityEngine.Object.Destroy(projection2);
                    }
                }
                else
                {
                    var cursorPositionProjection = CalcProjectionOnAxis(axis, cursorPosition);
                    var alignment = startPositionProjection - cursorPositionProjection;
                    var endPosition = cursorPosition + alignment;
                    
                    projectionComponent2.Draw(endPlane, endPosition, default(Vector3));

                    if (isEnd)
                    {
                        projectionComponent1.Draw(default(WallInfo), default(Vector3), startPositionProjection);
                        projectionComponent2.Draw(default(WallInfo), default(Vector3), startPositionProjection);

                        DrawPoint(endPlane, endPosition);

                        projectionComponent1.ColliderEnabled = true;
                        projectionComponent2.ColliderEnabled = true;
                    }
                }
            };
        }

        public DrawAction DrawLineParallelToAxis(WallInfo plane, Vector3 startPosition, float lineWidth = _HELP_LINE_WIDTH)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
        
            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = 0.002f;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;

            var axis = GetAxis(plane);
            var startPositionProjection = CalcProjectionOnAxis(axis, startPosition);
            var startPositionOffsetFromAxis = startPosition - startPositionProjection;
        
            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                var cursorPosition = CalcPosition(plane, hitPosition);

                var cursorPositionProjection = CalcProjectionOnAxis(axis, cursorPosition);

                var endPosition = cursorPositionProjection + startPositionOffsetFromAxis;

                lineComponent.Draw(default(WallInfo), default(Vector3), endPosition);
        
                if (isEnd)
                    lineComponent.ColliderEnabled = true;
            };
        }

        public DrawAction DrawLinePerpendicularToAxis(WallInfo plane, Vector3 startPosition, float lineWidth = _HELP_LINE_WIDTH)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
        
            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = 0.002f;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;

            var axis = GetAxis(plane);
            var startPositionProjection = CalcProjectionOnAxis(axis, startPosition);

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                var cursorPosition = CalcPosition(plane, hitPosition);

                var endPosition = CalcProjectionOnAxis(startPosition, startPositionProjection, cursorPosition);
        
                lineComponent.Draw(default(WallInfo), default(Vector3), endPosition);
        
                if (isEnd)
                    lineComponent.ColliderEnabled = true;
            };
        }

        //---

        public void Save()
        {
            StorePoints();
            StoreLines();
            StoreCircles();

            StateManager.Exp.Save();
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

            RestorePoints();
            RestoreLines();
            RestoreCircles();
        }

        private void RestorePoints()
        {
            StateManager.Exp.RestorePoints((plane, position, labels) =>
            {
                var point = new GameObject("POINT");
                point.transform.SetParent(_pointRepo.transform);

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
            
                var lineComponent = line.AddComponent<Line>();
                lineComponent.Width = lineWidth;
                lineComponent.Draw(plane, startPosition, endPosition);
                lineComponent.EnabledLabels = true;
                lineComponent.ColliderEnabled = true;
            
                labels.ForEach(label => lineComponent.AddLabel(label));

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

                var circleComponent = circle.AddComponent<Circle>();
                circleComponent.Draw(plane, startPosition, endPosition);
                circleComponent.ColliderEnabled = true;
            });
        }
    }
}
