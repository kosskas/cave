

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Scripts.Experimental;
using Assets.Scripts.Experimental.Items;
using UnityEngine;

public static class Vector3Extensions
{
    public static bool ApproximatelyEqual(this Vector3 a, Vector3 b, float tolerance = 0.0001f)
    {
        return Mathf.Abs(a.x - b.x) < tolerance &&
               Mathf.Abs(a.y - b.y) < tolerance &&
               Mathf.Abs(a.z - b.z) < tolerance;
    }
}

namespace Assets.Scripts.Experimental
{
    
    public class ItemsController
    {
        private const float _WALL_HALF_WIDTH = 0.05f;
        private const float _WALL_HALF_LENGTH = 1.7f;
        private const float _OFFSET_FROM_WALL = 0.01f;

        private readonly GameObject _workspace;
        private readonly GameObject _axisRepo;
        private readonly GameObject _lineRepo;
        private readonly GameObject _pointRepo;

        private readonly Dictionary<Axis, Tuple<WallInfo, WallInfo>> _axisWalls;
        Vector3 CalculateProjectionOnLine(Vector3 lineStart, Vector3 lineStop, Vector3 point)
        {
            Vector3 A = lineStop - lineStart;
            Vector3 B = point - lineStart;

            // B*A = |B||A|cos(a) = |B||1|cos(a) = |B|cos(a) = distance from axis.To to point projection on axis 
            float projectionLength = Vector3.Dot(B, A.normalized);

            Vector3 projectionPoint = lineStart + projectionLength * A.normalized;

            return projectionPoint;
        }
        Axis GetAxis(WallInfo plane)
        {
            var axis = _axisWalls.FirstOrDefault(e => e.Value.Item1.Equals(plane) || e.Value.Item2.Equals(plane)).Key;
            return axis;
        }

        public ItemsController()
        {
            _workspace = GameObject.Find("Workspace") ?? new GameObject("Workspace");

            _axisRepo = new GameObject("AxisRepo");
            _axisRepo.transform.SetParent(_workspace.transform);

            _lineRepo = new GameObject("LineRepo");
            _lineRepo.transform.SetParent(_workspace.transform);

            _pointRepo = new GameObject("PointRepo");
            _pointRepo.transform.SetParent(_workspace.transform);

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
            labelComponent.Text = "X";
            labelComponent.LowerIndex = $"{planeA.number}{planeB.number}";

            _axisWalls.Add(axisComponent, new Tuple<WallInfo, WallInfo>(planeA, planeB));
        }

        public Action<WallInfo, Vector3, bool> AddLine(WallInfo fromPlane, Vector3 fromPos)
        {
            Vector3 normal = fromPlane.GetNormal();

            Vector3 offsetVector = normal * _OFFSET_FROM_WALL;

            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = 0.002f;
            lineComponent.Draw(fromPlane, fromPos + offsetVector, fromPos + offsetVector);

            var labelComponent = line.AddComponent<IndexedLabel>();
            labelComponent.UpperIndex = new string('\'', fromPlane.number);
            labelComponent.FontSize = 0.6f;

            return (toPlane, toPos, isEnd) =>
            {
                if (toPlane != fromPlane)
                {
                    return;
                }

                lineComponent.Draw(default(WallInfo), default(Vector3), toPos + offsetVector);

                if (isEnd)
                {
                    lineComponent.ColliderEnabled = true;
                }
            };
        }

        public Action<WallInfo, Vector3, bool> AddPerpendicularLine(WallInfo fromPlane, Vector3 fromPos)
        {
            Vector3 normal = fromPlane.GetNormal();

            Vector3 offsetVector = normal * _OFFSET_FROM_WALL;

            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = 0.002f;
            lineComponent.Draw(fromPlane, fromPos + offsetVector, fromPos + offsetVector);

            var labelComponent = line.AddComponent<IndexedLabel>();
            labelComponent.UpperIndex = new string('\'', fromPlane.number);
            labelComponent.FontSize = 0.6f;

            var axis = GetAxis(fromPlane);
            var fromPosProjection = CalculateProjectionOnLine(axis.From, axis.To, fromPos + offsetVector);

            return (toPlane, toPos, isEnd) =>
            {
                if (toPlane != fromPlane)
                {
                    return;
                }

                var projectionPoint = CalculateProjectionOnLine(fromPos + offsetVector, fromPosProjection, toPos + offsetVector);

                lineComponent.Draw(default(WallInfo), default(Vector3), projectionPoint);

                if (isEnd)
                {
                    lineComponent.ColliderEnabled = true;
                }
            };
        }

        public Action<WallInfo, ExPoint, Vector3, bool> AddLineBetweenPoints(WallInfo fromPlane, ExPoint fromPoint, Vector3 fromPos)
        {
            var line = new GameObject("LINE_BETWEEN_POINTS");
            line.transform.SetParent(_lineRepo.transform);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Draw(fromPlane, fromPos, fromPos);

            return (toPlane, toPoint, toPos, isEnd) =>
            {
                if (toPlane != fromPlane)
                {
                    return;
                }

                lineComponent.Draw(default(WallInfo), default(Vector3), toPos);

                if (isEnd)
                {
                    lineComponent.ColliderEnabled = true;
                    //lineComponent.AddEdgeProjTest(fromPoint.GetComponent<IndexedLabel>().Text, toPoint.GetComponent<IndexedLabel>().Text);
                }
            };
        }

        public Action<WallInfo, Vector3, bool> AddProjection(WallInfo fromPlane, Vector3 fromPos)
        {
            // FIRST PART
            var projection1 = new GameObject("PROJECTION");
            projection1.transform.SetParent(_lineRepo.transform);

            var projectionComponent1 = projection1.AddComponent<Line>();
            projectionComponent1.ColliderEnabled = false;
            projectionComponent1.Width = 0.002f;
            projectionComponent1.Draw(fromPlane, fromPos, fromPos);

            // SECOND PART
            var projection2 = new GameObject("PROJECTION");
            projection2.transform.SetParent(_lineRepo.transform);

            var projectionComponent2 = projection2.AddComponent<Line>();
            projectionComponent2.ColliderEnabled = false;
            projectionComponent2.Width = 0.002f;

            // PROJECTION ON AXIS
            var axis = GetAxis(fromPlane);
            var fromPosProjection = CalculateProjectionOnLine(axis.From, axis.To, fromPos);

            return (toPlane, toPos, isEnd) =>
            {
                if (toPlane == default(WallInfo))
                {
                    return;
                }

                if (toPlane == fromPlane)
                {
                    var projectionPoint = CalculateProjectionOnLine(fromPos, fromPosProjection, toPos);

                    projectionComponent1.Draw(default(WallInfo), default(Vector3), projectionPoint);

                    if (isEnd)
                    {
                        projectionComponent1.ColliderEnabled = true;
                    }
                }
                else
                {
                    var toPosProjection = CalculateProjectionOnLine(axis.From, axis.To, toPos);
                    var offsetToFromPosProjection = fromPosProjection - toPosProjection;
                    Vector3 offsetVector = toPlane.GetNormal() * _OFFSET_FROM_WALL;
                    var projectionPoint = toPos + offsetToFromPosProjection;

                    projectionComponent1.Draw(default(WallInfo), default(Vector3), fromPosProjection);
                    projectionComponent2.Draw(toPlane, fromPosProjection, projectionPoint + offsetVector);

                    if (isEnd)
                    {
                        projectionComponent1.ColliderEnabled = true;
                        projectionComponent2.ColliderEnabled = true;
                        AddPoint(toPlane, projectionPoint);
                    }
                }
            };
        }

        public Action<WallInfo, Vector3, bool> AddCircle(WallInfo fromPlane, Vector3 fromPos)
        {
            var circle = new GameObject("CIRCLE");
            circle.transform.SetParent(_lineRepo.transform);

            var circleComponent = circle.AddComponent<Circle>();
            circleComponent.ColliderEnabled = false;
            circleComponent.Draw(fromPlane, fromPos, fromPos);

            return (toPlane, toPos, isEnd) =>
            {
                if (toPlane != fromPlane)
                {
                    return;
                }

                circleComponent.Draw(default(WallInfo), default(Vector3), toPos);

                if (isEnd)
                {
                    circleComponent.ColliderEnabled = true;
                }
            };
        }

        public void AddPoint(WallInfo plane, Vector3 pos)
        {
            Vector3 normal = plane.GetNormal();

            Vector3 offsetVector = normal * _OFFSET_FROM_WALL;

            var point = new GameObject("POINT");
            point.transform.SetParent(_pointRepo.transform);

            var pointComponent = point.AddComponent<ExPoint>();
            pointComponent.Draw(plane, pos + offsetVector);

            var labelComponent = point.AddComponent<IndexedLabel>();
            labelComponent.UpperIndex = new string('\'', plane.number);
            labelComponent.FontSize = 0.6f;
        }

        
    }
}

