

using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Experimental;
using Assets.Scripts.Experimental.Items;
using UnityEngine;

public class ItemsController
{
    private const float _WALL_HALF_WIDTH = 0.05f;
    private const float _WALL_HALF_LENGTH = 1.7f;
    private const float _OFFSET_FROM_WALL = 0.01f;

    private readonly GameObject _workspace;
    private readonly GameObject _axisRepo;
    private readonly GameObject _lineRepo;
    private readonly GameObject _pointRepo;


    public ItemsController()
    {
        _workspace = GameObject.Find("Workspace") ?? new GameObject("Workspace");

        _axisRepo = new GameObject("AxisRepo");
        _axisRepo.transform.SetParent(_workspace.transform);

        _lineRepo = new GameObject("LineRepo");
        _lineRepo.transform.SetParent(_workspace.transform);

        _pointRepo = new GameObject("PointRepo");
        _pointRepo.transform.SetParent(_workspace.transform);
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
        axisComponent.Draw(from, to);

        var labelComponent = axis.AddComponent<IndexedLabel>();
        labelComponent.Text = "X";
        labelComponent.LowerIndex = $"{planeA.number}{planeB.number}";
    }

    public Action<WallInfo, Vector3> AddLine(WallInfo plane, Vector3 from)
    {
        Vector3 normal = plane.GetNormal();

        Vector3 offsetVector = normal * _OFFSET_FROM_WALL;

        var line = new GameObject("LINE");
        line.transform.SetParent(_lineRepo.transform);

        var lineComponent = line.AddComponent<Line>();
        lineComponent.Draw(from + offsetVector, from + offsetVector);

        var labelComponent = line.AddComponent<IndexedLabel>();
        labelComponent.UpperIndex = new string('\'', plane.number);
        labelComponent.FontSize = 0.6f;

        return (toPlane, to) =>
        {
            if (toPlane != plane)
            {
                return;
            }

            lineComponent.Draw(default(Vector3), to + offsetVector);
        };
    }

    public Action<Vector3, bool> AddLineBetweenPoints(Vector3 fromPoint)
    {
        var line = new GameObject("LINE_BETWEEN_POINTS");
        line.transform.SetParent(_lineRepo.transform);

        var lineComponent = line.AddComponent<Line>();
        lineComponent.ColliderEnabled = false;
        lineComponent.Draw(fromPoint, fromPoint);

        return (toPoint, isEnd) =>
        {
            lineComponent.Draw(default(Vector3), toPoint);

            if (isEnd)
            {
                lineComponent.ColliderEnabled = true;
            }
        };
    }

    public void AddPoint(WallInfo plane, Vector3 pos)
    {
        Vector3 normal = plane.GetNormal();

        Vector3 offsetVector = normal * _OFFSET_FROM_WALL;

        var point = new GameObject("POINT");
        point.transform.SetParent(_pointRepo.transform);

        var pointComponent = point.AddComponent<Assets.Scripts.Experimental.Items.Point>();
        pointComponent.Draw(pos + offsetVector);

        var labelComponent = point.AddComponent<IndexedLabel>();
        labelComponent.UpperIndex = new string('\'', plane.number);
        labelComponent.FontSize = 0.6f;
    }
}