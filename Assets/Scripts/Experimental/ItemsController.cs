

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


    public ItemsController()
    {
        _workspace = GameObject.Find("Workspace") ?? new GameObject("Workspace");

        _axisRepo = new GameObject("AxisRepo");
        _axisRepo.transform.SetParent(_workspace.transform);
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
        labelComponent.Draw(from);
    }
}