using UnityEngine;

public class Mode3Dto2D : IMode
{
    private WallController _wc;
    private WallCreator _wcrt;
    private SolidImporter _si;

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

        SetUpFlystick();

        Debug.Log($"<color=blue> MODE grupowy ON </color>");
    }

    private void _BackToMenu()
    {
        _wc.SetBasicWalls();
        GameObject.Destroy(_si);
        PCref.ChangeMode(PlayerController.Mode.ModeMenu);
    }
    ////
    /// NOTE: jedyny Input nie będący tutaj jest w pliku ObjectRotator!
    ///
    //from solidimporter
    public void HandleInput()
    {
        if (Input.GetKeyDown("m"))
        {
            _BackToMenu();
        }

        if (Input.GetKeyDown("p"))
        {
            _DisplayNextSolid();
        }

        if (Input.GetKeyDown("u"))
        {
            _DisplayPreviousSolid();
        }

        if (Input.GetKeyDown("o"))
        {
            _ShowProjectionLines();
        }
        
        if(Input.GetKeyDown("i"))
        {
            _ShowReferenceLines();
        }

        if(Input.GetKeyDown("l"))
        {
            _RemoveWall();
        }

        if (Input.GetKeyDown("v"))
        {
            _SetShowingProjection();
        }

        if (Input.GetKeyDown("c"))
        {
            _AddPointToCreateFace();
        }
    }

    public void SetUpFlystick()
    {
        FlystickController.ClearActions();

        FlystickController.SetAction(
            FlystickController.Btn._1,
            FlystickController.ActOn.PRESS,
            _ShowProjectionLines
        );

        FlystickController.SetAction(
            FlystickController.Btn._2,
            FlystickController.ActOn.PRESS,
            _ShowReferenceLines
        );

        FlystickController.SetAction(
            FlystickController.Btn._3,
            FlystickController.ActOn.PRESS,
            _SetShowingProjection
        );

        FlystickController.SetAction(
            FlystickController.Btn._4,
            FlystickController.ActOn.PRESS,
            _AddPointToCreateFace
        );

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.PRESS,
            _RemoveWall
        );

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_LEFT,
            _DisplayPreviousSolid
        );

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_RIGHT,
            _DisplayNextSolid
        );
    }
}