using Assets.Scripts.Experimental.Utils;
using Assets.Scripts.Experimental;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ModeMenu : IMode
{
    public PlayerController PCref { get; private set; }
    private CircularIterator<KeyValuePair<ExContext, Action>> _context;
    private static float Z_RADIAL_MENU_OFFSET = (GameObject.Find("TrackedObject") != null ? 0.0f : 0.55f);
    private static float Y_RADIAL_MENU_OFFSET = (GameObject.Find("TrackedObject") != null ? 0.0f : -0.30f);
    private static float RADIAL_1ST_MENU_RADIUS = 15f;
    private static float RADIAL_2ND_MENU_RADIUS = 25f;
    private RadialMenu radialMenu;

    public ModeMenu(PlayerController pc)
    {
        PCref = pc;
        _context = new CircularIterator<KeyValuePair<ExContext, Action>>(
            new List<KeyValuePair<ExContext, Action>>()
            {
                new KeyValuePair<ExContext, Action>(ExContext.Const, _LoadReconstruction),
                new KeyValuePair<ExContext, Action>(ExContext.LoadVisual, _LoadVisualization),
                
            });
        AddRadialMenu();
        Debug.Log($"<color=blue> MODE menu ON </color>");

        SetUpFlystick();
    }

    public void AddRadialMenu()
    {
        GameObject flystick = GameObject.Find("TrackedObject");
        if (flystick == null)
        {
            flystick = GameObject.Find("Main Camera");
            if (flystick == null)
            {
                flystick = new GameObject("FlystickPlaceholder");
            }
        }

        GameObject canvasPrefab = Resources.Load<GameObject>("Canvas");
        if (canvasPrefab != null)
        {
            GameObject canvas = GameObject.Instantiate(canvasPrefab, flystick.transform);

            Vector3 localPos = canvas.transform.localPosition;
            localPos.z = Z_RADIAL_MENU_OFFSET;
            localPos.y = Y_RADIAL_MENU_OFFSET;
            canvas.transform.localPosition = localPos;
            canvas.transform.rotation = flystick.transform.rotation;

            Transform radialMenuRoot = canvas.transform.Find("RadialMenuRoot");
            if (radialMenuRoot != null)
            {
                GameObject itemPrefab = Resources.Load<GameObject>("RadialMenuItem");
                if (itemPrefab != null)
                {
                    this.radialMenu = RadialMenu.Create(
                        radialMenuRoot,
                        itemPrefab
                    );
                    radialMenu.Generate(_context, RADIAL_1ST_MENU_RADIUS);
                }
                else
                {
                    Debug.LogError("Nie mo¿na za³adowaæ prefabrykatu MenuItem z Resources/RadialMenuItem");
                }
            }
            else
            {
                Debug.LogError("Nie znaleziono RadialMenuRoot w Canvas");
            }
        }
        else
        {
            Debug.LogError("Nie znaleziono fabrykatu z Resources/Canvas");
        }
    }

    public void HandleInput()
    {

        if (Input.GetKeyDown("1"))
        {
            _context.Current.Value();
        }

        //if (Input.GetKeyDown("2"))
        //{
        //    _LoadReconstruction();
        //}

    }

    private void _LoadVisualization()
    {
        radialMenu.RemoveFromScene();
        radialMenu = null;
        PCref.ChangeMode(PlayerController.Mode.Mode3Dto2D);
    }

    private void _LoadReconstruction()
    {
        radialMenu.RemoveFromScene();
        radialMenu = null;
        PCref.ChangeMode(PlayerController.Mode.ModeExperimental);
    }

    public void SetUpFlystick()
    {
        FlystickController.ClearActions();

        FlystickController.SetAction(
            FlystickController.Btn._1,
            FlystickController.ActOn.PRESS,
            _LoadVisualization
        );

        FlystickController.SetAction(
            FlystickController.Btn._2,
            FlystickController.ActOn.PRESS,
            _LoadReconstruction
        );
    }
}

/*
 *  |           ACTION          |       DEV     |               LZWP                |
 *  | _MakeActionOnWall         |       5       |   Btn.FIRE ActOn.PRESS            |
 */