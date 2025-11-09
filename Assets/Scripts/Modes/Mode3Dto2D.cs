using Assets.Scripts.Experimental.Utils;
using Assets.Scripts.Experimental;
using System.Collections.Generic;
using System;
using Assets.Scripts.FileManagers;
using UnityEngine;

public class Mode3Dto2D : IMode
{
    private WallController _wc;
    private WallCreator _wcrt;
    private SolidImporter _si;
    private CircularIterator<KeyValuePair<ExContext, Action>> _context;
    private static float Z_RADIAL_MENU_OFFSET = (GameObject.Find("TrackedObject") != null ? 0.0f : 0.55f);
    private static float Y_RADIAL_MENU_OFFSET = (GameObject.Find("TrackedObject") != null ? 0.0f : -0.30f);
    private static float RADIAL_1ST_MENU_RADIUS = 15f;
    private static float RADIAL_2ND_MENU_RADIUS = 25f;
    private RadialMenu radialMenu;

    public PlayerController PCref { get; private set; }

    private void _DisplayNextSolid()
    {
        _wc.SetBasicWalls();
        _wc.SetDefaultShowRules();
        _si.SetUpDirection();
        _si.ImportSolid();
    }

    private void _DisplayPreviousSolid()
    {
        _wc.SetBasicWalls();
        _wc.SetDefaultShowRules();
        _si.SetDownDirection();
        _si.ImportSolid();
    }

    private void _ShowProjectionLines()
    {
        ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
        op.SetShowingProjectionLines();
    }

    private void _ShowReferenceLines()
    {
        ObjectProjecter op = (ObjectProjecter)GameObject.FindObjectOfType(typeof(ObjectProjecter));
        op.SetShowingReferenceLines();
    }

    private void _RemoveWall()
    {
        _wc.PopBackWall();
    }

    private void _AddPointToCreateFace()
    {
        _wcrt.AddAnchorPoint(PCref.Hit);
    }

    private void _SetShowingProjection()
    {
        if (PCref.Hit.collider == null) {
            return;
        }
        if (PCref.Hit.collider.tag != "Wall") {
            return;
        }

        WallInfo info = _wc.FindWallInfoByGameObject(PCref.Hit.collider.gameObject);

        if (info == null) {
            return;
        }
        else if (info.showLines) {
            _wc.SetWallInfo(PCref.Hit.collider.gameObject, false, false, false);
        }
        else {
            _wc.SetWallInfo(PCref.Hit.collider.gameObject, true, true, true);
        }
    }

    public Mode3Dto2D(PlayerController pc)
    {
        PCref = pc;

        GameObject mainObject = GameObject.Find("MainObject");
        _si = GameObject.FindObjectOfType<SolidImporter>(); //z trybu rek. zostal juz zainicjalizowany
        if (_si == null)
        {
            _si = mainObject.AddComponent<SolidImporter>();
            _si.Init();
        }

        GameObject wallsObject = GameObject.Find("Walls");
        _wc = wallsObject.GetComponent<WallController>();
        _wcrt = PCref.gameObject.GetComponent<WallCreator>();

        //PointsList.ceilingWall.SetActive(true);
        _context = new CircularIterator<KeyValuePair<ExContext, Action>>(
            new List<KeyValuePair<ExContext, Action>>()
            {
                new KeyValuePair<ExContext, Action>(ExContext.BackToMenu, _BackToMenu),
                new KeyValuePair<ExContext, Action>(ExContext.NextSolid, _DisplayNextSolid),
                new KeyValuePair<ExContext, Action>(ExContext.PrevSolid, _DisplayPreviousSolid),
                new KeyValuePair<ExContext, Action>(ExContext.ProjLine, _ShowProjectionLines),
                new KeyValuePair<ExContext, Action>(ExContext.Projection, _ShowReferenceLines),
                new KeyValuePair<ExContext, Action>(ExContext.RemoveWall, _RemoveWall),
                new KeyValuePair<ExContext, Action>(ExContext.AddWall, _AddPointToCreateFace),
                new KeyValuePair<ExContext, Action>(ExContext.ShowProj, _SetShowingProjection),

            });
        AddRadialMenu();

        SetUpFlystick();

        Debug.Log($"<color=blue> MODE grupowy ON </color>");
    }

    private void _BackToMenu()
    {
        _wc.SetBasicWalls();
        GameObject mainObject = GameObject.Find("MainObject");
        GameObject.Destroy(mainObject);
        mainObject = new GameObject("MainObject");
        GameObject.Destroy(_si);
        radialMenu.RemoveFromScene();
        radialMenu = null;
        PCref.ChangeMode(PlayerController.Mode.ModeMenu);
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
                    radialMenu.Generate(_context, RADIAL_2ND_MENU_RADIUS);
                }
                else
                {
                    Debug.LogError("Nie można załadować prefabrykatu MenuItem z Resources/RadialMenuItem");
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
    ////
    /// NOTE: jedyny Input nie będący tutaj jest w pliku ObjectRotator!
    ///
    //from solidimporter
    public void HandleInput()
    {
        //if (Input.GetKeyDown("m"))
        //{
        //    _BackToMenu();
        //}

        //if (Input.GetKeyDown("p"))
        //{
        //    _DisplayNextSolid();
        //}

        //if (Input.GetKeyDown("u"))
        //{
        //    _DisplayPreviousSolid();
        //}

        //if (Input.GetKeyDown("o"))
        //{
        //    _ShowProjectionLines();
        //}

        //if(Input.GetKeyDown("i"))
        //{
        //    _ShowReferenceLines();
        //}

        //if(Input.GetKeyDown("l"))
        //{
        //    _RemoveWall();
        //}

        //if (Input.GetKeyDown("v"))
        //{
        //    _SetShowingProjection();
        //}

        //if (Input.GetKeyDown("c"))
        //{
        //    _AddPointToCreateFace();
        //}
        if (Input.GetKeyDown("1"))
        {
            _context.Current.Value();
        }
    }

    public void SetUpFlystick()
    {
        FlystickController.ClearActions();

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_LEFT,
            () =>
            {
                radialMenu.PreviousOption();
            }
        );

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_RIGHT,
            () =>
            {
                radialMenu.NextOption();
            }
        );

        FlystickController.SetAction(
            FlystickController.Btn._1,
            FlystickController.ActOn.PRESS,
            () =>
            {
                _context.Current.Value();            
            }
        );
    }
}