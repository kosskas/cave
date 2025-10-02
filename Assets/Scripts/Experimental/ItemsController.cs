using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Experimental.Items;
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
        private readonly FacesGenerator _fGen;
        private readonly MeshBuilder _mB;

        private static ItemsController _ic;

        private readonly GameObject _workspace;

        private static GameObject _axisRepo;
        private static GameObject _lineRepo;
        private static GameObject _pointRepo;
        private static GameObject _circleRepo;

        private readonly Dictionary<Axis, Tuple<WallInfo, WallInfo>> _axisWalls;

        private List<ExPoint> _facePoints;


        /*   C O N S T R U C T O R S   */

        public ItemsController(WallController wCtrl, WallCreator wCrt, FacesGenerator fGen, MeshBuilder mB)
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

            _facePoints = new List<ExPoint>();

            _wCtrl = wCtrl;
            _wCrt = wCrt;
            _fGen = fGen;
            _mB = mB;

            _ic = this;
        }


        /*   P R I V A T E   M E T H O D S   */

        private static List<T> GetComponentsFromRepo<T>(GameObject repo) where T : Component
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

            if (hitPoint != null)
                position = hitPoint.Position;

            // Debug.Log($"hitPosition=({hitPosition.x:F9}, {hitPosition.y:F9}, {hitPosition.z:F9}) " +
            //           $"position=({position.x:F9}, {position.y:F9}, {position.z:F9}) " +
            //           $"planeNormal=({planeNormal.x:F9}, {planeNormal.y:F9}, {planeNormal.z:F9}) " +
            //           $"planePosition=({planePosition.x:F9}, {planePosition.y:F9}, {planePosition.z:F9})");

            return position;
        }


        /*   P U B L I C   M E T H O D S   */

        public void AddAxisBetweenPlanes(WallInfo planeA, WallInfo planeB)
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


            var axis = new GameObject("AXIS");
            axis.transform.SetParent(_axisRepo.transform);

            var axisComponent = axis.AddComponent<Axis>();
            axisComponent.Width = _AXIS_LINE_WIDTH;
            axisComponent.Draw(default(WallInfo), from, to);

            var labelComponent = axis.AddComponent<IndexedLabel>();
            labelComponent.AddLabel("X", "", $"{planeA.numberExp}/{planeB.numberExp}");
            labelComponent.FontSize = 1;

            _axisWalls.Add(axisComponent, new Tuple<WallInfo, WallInfo>(planeA, planeB));
            _wCtrl.LinkConstructionToWall(planeA, axis);
            _wCtrl.LinkConstructionToWall(planeB, axis);
        }

        public void RemoveLastAxis()
        {
            if (_axisWalls != null && _axisWalls.Count > 0 && _wCtrl != null)
            {
                var deletingWall = _wCtrl.GetLastAddedWall();
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
            IRaycastable relativeObject = null)
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

                case ExContext.PerpendicularLine: return DrawLinePerpendicularToLine(plane, positionWithPointSensitivity, relativeObject);

                case ExContext.ParallelLine: return DrawLineParallelToLine(plane, positionWithPointSensitivity, relativeObject);

                case ExContext.Circle: return DrawCircle(plane, positionWithPointSensitivity);

                case ExContext.Projection: return DrawProjection(plane, positionWithPointSensitivity);

                case ExContext.Wall: return DrawWall(plane, positionWithPointSensitivity);

                case ExContext.Face: return DrawFace(hitObject as ExPoint);

                default: return null;
            }
        }

        public DrawAction DrawPoint(WallInfo plane, Vector3 position, List<string> labels = null)
        {
            var point = new GameObject("POINT");
            point.transform.SetParent(_pointRepo.transform);
            _wCtrl.LinkConstructionToWall(plane, point);

            var pointComponent = point.AddComponent<ExPoint>();
            pointComponent.Draw(plane, position);
            pointComponent.EnabledLabels = true;

            labels?.ForEach(label => pointComponent.AddLabel(label));

            return null;
        }

        public DrawAction DrawFace(ExPoint chosenPoint)
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

                _fGen.GenerateFace(points3D);

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

        public DrawAction DrawLine(WallInfo plane, Vector3 startPosition, ExPoint startPoint = null, float lineWidth = _HELP_LINE_WIDTH)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
            _wCtrl.LinkConstructionToWall(plane, line);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = lineWidth;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;
            lineComponent.SetLabelVisible(true);

            var intersectedObjs = new HashSet<IAnalyzable>();

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                var endPositionWithPointSensitivity = CalcPosition(plane, hitPosition, hitObject as ExPoint);

                lineComponent.Draw(default(WallInfo), default(Vector3), endPositionWithPointSensitivity);

                lineComponent.SetLabel(Vector3.Distance(startPosition, endPositionWithPointSensitivity));

                if (hitObject is IAnalyzable && hitObject != line.GetComponent<IRaycastable>())
                {
                    IAnalyzable aHitObject = (IAnalyzable)hitObject;
                    intersectedObjs.Add(aHitObject.GetElement());

                    if (hitObject is IColorable)
                    {
                        IColorable cHitObject = (IColorable)hitObject;
                        cHitObject.Color = ReconstructionInfo.MENTIONED;
                    }
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

                    foreach (var intersected in intersectedObjs)
                    {
                        List<Vector3> crossings = intersected.FindCrossingPoints(lineComponent);
                        if (crossings != null)
                        {
                            foreach (var point in crossings)
                            {
                                if(Vector3.SqrMagnitude(endPositionWithPointSensitivity - point) < 1e-5f) continue;
                                DrawPoint(plane, point);
                            }
                        }
                        if (intersected is IColorable)
                        {
                            IColorable cIntersected = (IColorable)intersected;
                            cIntersected.Color = ReconstructionInfo.NORMAL;
                        }
                    }
                }
            };
        }

        /// TODO Przeciecia sa niedostepne dla scian
        public DrawAction DrawWall(WallInfo plane, Vector3 startPosition, string fixedName = null)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
            _wCtrl.LinkConstructionToWall(plane, line);

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

                    var addedWall = _wCrt.WCrCreateWall(startPosition, endPositionWithPointSensitivity, plane, fixedName);
                    AddAxisBetweenPlanes(addedWall, plane);
                }
            };
        }

        public DrawAction DrawCircle(WallInfo plane, Vector3 startPosition, float lineWidth = _HELP_LINE_WIDTH)
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

                if (hitObject is IAnalyzable && hitObject != circleComponent.GetComponent<IRaycastable>())
                {
                    IAnalyzable aHitObject = (IAnalyzable)hitObject;
                    intersectedObjs.Add(aHitObject.GetElement());

                    if (hitObject is IColorable)
                    {
                        IColorable cHitObject = (IColorable)hitObject;
                        cHitObject.Color = ReconstructionInfo.MENTIONED;
                    }
                }

                if (isEnd)
                {
                    circleComponent.ColliderEnabled = true;
                    foreach (var intersected in intersectedObjs)
                    {
                        List<Vector3> crossings = intersected.FindCrossingPoints(circleComponent);
                        if (crossings != null)
                        {
                            foreach (var point in crossings)
                            {
                                if (Vector3.SqrMagnitude(endPositionWithPointSensitivity - point) < 1e-5f) continue;
                                DrawPoint(plane, point);
                            }
                        }
                        if (intersected is IColorable)
                        {
                            IColorable cIntersected = (IColorable)intersected;
                            cIntersected.Color = ReconstructionInfo.NORMAL;
                        }
                    }
                }
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

            var intersectedObjsPl1 = new HashSet<IAnalyzable>();

            // SECOND PART
            var projection2 = new GameObject("PROJECTION");
            projection2.transform.SetParent(_lineRepo.transform);

            var projectionComponent2 = projection2.AddComponent<Line>();
            projectionComponent2.ColliderEnabled = false;
            projectionComponent2.EnabledLabels = true;
            projectionComponent2.Width = lineWidth;
            projectionComponent2.Draw(startPlane, startPosition, startPosition);
            projectionComponent1.SetLabelVisible(false);

            var intersectedObjsPl2 = new HashSet<IAnalyzable>();

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (startPlane == default(WallInfo))
                    return;

                // CHOOSE AXIS
                var axes = GetAllAxes(startPlane);
                var axis = FindApproachingAxis(startPosition, hitPosition, axes);
                if (axis == default(Axis))
                    return;

                // END PLANE
                var endPlane = FindEndPlane(axis, startPlane);

                // CURRENT PLANE
                var currPlane = FindPlane(hitPlane, hitObject);
                if (currPlane == default(WallInfo))
                    return;

                // PROJECTION OF START POSITION ON AXIS
                var startPositionProjection = CalcProjectionOnAxis(axis, startPosition);

                // CURRENT POSITION
                var cursorPosition = CalcPosition(currPlane, hitPosition);

                // PROJECTION OF CURRENT POSITION ON AXIS
                var cursorPositionProjection = CalcProjectionOnAxis(axis, cursorPosition);

                // DISPLACEMENT VECTOR
                var vDisplacement = startPositionProjection - cursorPositionProjection;

                // ACTUAL END POSITION
                var endPosition = cursorPosition + vDisplacement;

                if (startPlane == currPlane)
                {
                    projectionComponent1.Draw(startPlane, startPosition, endPosition);
                    projectionComponent1.SetLabel(Vector3.Distance(startPosition, endPosition));
                    projectionComponent1.SetLabelVisible(true);

                    projectionComponent2.Draw(startPlane, startPosition, endPosition);
                    projectionComponent2.SetLabelVisible(false);

                    if (hitObject is IAnalyzable && hitObject != projectionComponent1.GetComponent<IRaycastable>())
                    {
                        IAnalyzable aHitObject = (IAnalyzable)hitObject;
                        intersectedObjsPl1.Add(aHitObject.GetElement());

                        if (hitObject is IColorable)
                        {
                            IColorable cHitObject = (IColorable)hitObject;
                            cHitObject.Color = ReconstructionInfo.MENTIONED;
                        }
                    }

                    if (isEnd)
                    {
                        UnityEngine.Object.Destroy(projection1);
                        UnityEngine.Object.Destroy(projection2);
                    }
                }
                else
                {
                    projectionComponent1.Draw(startPlane, startPosition, startPositionProjection);
                    projectionComponent1.SetLabelVisible(false);

                    projectionComponent2.Draw(endPlane, startPositionProjection, endPosition);
                    projectionComponent2.SetLabel(Vector3.Distance(startPositionProjection, endPosition));
                    projectionComponent2.SetLabelVisible(true);

                    if (hitObject is IAnalyzable && hitObject != projectionComponent2.GetComponent<IRaycastable>())
                    {
                        IAnalyzable aHitObject = (IAnalyzable)hitObject;
                        intersectedObjsPl2.Add(aHitObject.GetElement());

                        if (hitObject is IColorable)
                        {
                            IColorable cHitObject = (IColorable)hitObject;
                            cHitObject.Color = ReconstructionInfo.MENTIONED;
                        }
                    }

                    if (isEnd)
                    {
                        DrawPoint(endPlane, endPosition);

                        projectionComponent1.ColliderEnabled = true;
                        projectionComponent1.SetLabelVisible(false);
                        _wCtrl.LinkConstructionToWall(startPlane, projection1);

                        projectionComponent2.ColliderEnabled = true;
                        projectionComponent2.SetLabelVisible(false);
                        _wCtrl.LinkConstructionToWall(endPlane, projection2);

                        foreach (var intersected in intersectedObjsPl1)
                        {
                            List<Vector3> crossings = intersected.FindCrossingPoints(projectionComponent1);
                            if (crossings != null)
                            {
                                foreach (var point in crossings)
                                {
                                    DrawPoint(startPlane, point);
                                }
                            }
                            if (intersected is IColorable)
                            {
                                IColorable cIntersected = (IColorable)intersected;
                                cIntersected.Color = ReconstructionInfo.NORMAL;
                            }
                        }

                        foreach (var intersected in intersectedObjsPl2)
                        {
                            List<Vector3> crossings = intersected.FindCrossingPoints(projectionComponent2);
                            if (crossings != null)
                            {
                                foreach (var point in crossings)
                                {
                                    DrawPoint(endPlane, point);
                                }
                            }
                            if (intersected is IColorable)
                            {
                                IColorable cIntersected = (IColorable)intersected;
                                cIntersected.Color = ReconstructionInfo.NORMAL;
                            }
                        }
                    }
                }
            };
        }

        public DrawAction DrawLineParallelToLine(WallInfo plane, Vector3 startPosition, IRaycastable relativeObject = null)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
            _wCtrl.LinkConstructionToWall(plane, line);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = _HELP_LINE_WIDTH;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;
            lineComponent.SetLabelVisible(true);

            var relativeLine = relativeObject as Line;
            var relativeAxis = relativeObject as Axis;
            var fromPosition = Vector3.zero;
            var toPosition = Vector3.zero;

            if (relativeLine != null)
            {
                fromPosition = relativeLine.StartPosition;
                toPosition = relativeLine.EndPosition;
            }

            if (relativeAxis != null)
            {
                fromPosition = relativeAxis.From;
                toPosition = relativeAxis.To;
            }

            var intersectedObjs = new HashSet<IAnalyzable>();

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                if (fromPosition == Vector3.zero && toPosition == Vector3.zero)
                    return;

                var startPositionProjection = CalcProjectionOnAxis(fromPosition, toPosition, startPosition);
                var startPositionOffsetFromAxis = startPosition - startPositionProjection;

                var cursorPosition = CalcPosition(plane, hitPosition);

                var cursorPositionProjection = CalcProjectionOnAxis(fromPosition, toPosition, cursorPosition);

                var endPosition = cursorPositionProjection + startPositionOffsetFromAxis;

                lineComponent.Draw(default(WallInfo), default(Vector3), endPosition);
                lineComponent.SetLabel(Vector3.Distance(startPosition, endPosition));

                if (hitObject is IAnalyzable && hitObject != line.GetComponent<IRaycastable>())
                {
                    IAnalyzable aHitObject = (IAnalyzable)hitObject;
                    intersectedObjs.Add(aHitObject.GetElement());

                    if (hitObject is IColorable)
                    {
                        IColorable cHitObject = (IColorable)hitObject;
                        cHitObject.Color = ReconstructionInfo.MENTIONED;
                    }
                }

                if (isEnd)
                {
                    lineComponent.ColliderEnabled = true;
                    lineComponent.SetLabelVisible(false);

                    foreach (var intersected in intersectedObjs)
                    {
                        List<Vector3> crossings = intersected.FindCrossingPoints(lineComponent);
                        if (crossings != null)
                        {
                            foreach (var point in crossings)
                            {
                                //if (Vector3.SqrMagnitude(endPositionWithPointSensitivity - point) < 1e-5f) continue;
                                DrawPoint(plane, point);
                            }
                        }
                        if (intersected is IColorable)
                        {
                            IColorable cIntersected = (IColorable)intersected;
                            cIntersected.Color = ReconstructionInfo.NORMAL;
                        }
                    }
                }
            };
        }

        public DrawAction DrawLinePerpendicularToLine(WallInfo plane, Vector3 startPosition, IRaycastable relativeObject = null)
        {
            var line = new GameObject("LINE");
            line.transform.SetParent(_lineRepo.transform);
            _wCtrl.LinkConstructionToWall(plane, line);

            var lineComponent = line.AddComponent<Line>();
            lineComponent.ColliderEnabled = false;
            lineComponent.Width = _HELP_LINE_WIDTH;
            lineComponent.Draw(plane, startPosition, startPosition);
            lineComponent.EnabledLabels = true;
            lineComponent.SetLabelVisible(true);

            var relativeLine = relativeObject as Line;
            var relativeAxis = relativeObject as Axis;
            var vRelativeLine = Vector3.zero;

            if (relativeLine != null)
                vRelativeLine = relativeLine.EndPosition - relativeLine.StartPosition;

            if (relativeAxis != null)
                vRelativeLine = relativeAxis.To - relativeAxis.From;

            var intersectedObjs = new HashSet<IAnalyzable>();

            return (hitObject, hitPosition, hitPlane, isEnd) =>
            {
                if (plane != FindPlane(hitPlane, hitObject))
                    return;

                if (vRelativeLine == Vector3.zero)
                    return;

                var cursorPosition = CalcPosition(plane, hitPosition);

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

                if (hitObject is IAnalyzable && hitObject != line.GetComponent<IRaycastable>())
                {
                    IAnalyzable aHitObject = (IAnalyzable)hitObject;
                    intersectedObjs.Add(aHitObject.GetElement());

                    if (hitObject is IColorable)
                    {
                        IColorable cHitObject = (IColorable)hitObject;
                        cHitObject.Color = ReconstructionInfo.MENTIONED;
                    }
                }

                if (isEnd)
                {
                    lineComponent.ColliderEnabled = true;
                    lineComponent.SetLabelVisible(false);

                    foreach (var intersected in intersectedObjs)
                    {
                        List<Vector3> crossings = intersected.FindCrossingPoints(lineComponent);
                        if (crossings != null)
                        {
                            foreach (var point in crossings)
                            {
                                //if (Vector3.SqrMagnitude(endPositionWithPointSensitivity - point) < 1e-5f) continue;
                                DrawPoint(plane, point);
                            }
                        }
                        if (intersected is IColorable)
                        {
                            IColorable cIntersected = (IColorable)intersected;
                            cIntersected.Color = ReconstructionInfo.NORMAL;
                        }
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

        public static List<FaceInfo> GetFaces() => FacesGenerator.faceInfoList;

        public static void AddPoint(List<string> pointLabels, string pointPlaneName, Vector3 pointPosition)
        {
            _ic?.DrawPoint(_ic._wCtrl.GetWallByName(pointPlaneName), pointPosition, pointLabels);
        }

        public static void AddLine(List<string> lineBoundPointsByLabel, Vector3 lineEndPosition, List<string> lineLabels, float lineLineWidth, string linePlaneName, Vector3 lineStartPosition)
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
                        startPoint.FocusedLabel = startPointLabel;
                    }

                    if (point.Plane.Equals(plane) && point.Labels.Contains(endPointLabel))
                    {
                        endPoint = point;
                        endPoint.FocusedLabel = endPointLabel;
                    }
                }
            }

            var da = _ic.DrawLine(plane, lineStartPosition, startPoint, lineLineWidth);
            da.Invoke(endPoint, lineEndPosition, plane, true);
        }

        public static void AddCircle(Vector3 circleEndPosition, float circleLineWidth, string circlePlaneName, Vector3 circleStartPosition)
        {
            if (_ic == null) return;

            var plane = _ic._wCtrl.GetWallByName(circlePlaneName);

            var da = _ic.DrawCircle(plane, circleStartPosition, circleLineWidth);
            da.Invoke(null, circleEndPosition, plane, true);
        }

        public static void AddWall(Vector3? wallConstPoint1, Vector3? wallConstPoint2, string wallParentWallName, string wallWallName)
        {
            if (_ic == null) return;
            if (wallParentWallName == null) return;
            if (wallConstPoint1 == null) return;
            if (wallConstPoint2 == null) return;

            var plane = _ic._wCtrl.GetWallByName(wallParentWallName);

            var da = _ic.DrawWall(plane, (Vector3)wallConstPoint1, wallWallName);
            da.Invoke(null, (Vector3)wallConstPoint2, plane, true);
        }

        public static void AddFace(List<KeyValuePair<string, Vector3>> faceVertices)
        {
            _ic?._fGen.GenerateFace(faceVertices);
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

    }
}
