

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


}