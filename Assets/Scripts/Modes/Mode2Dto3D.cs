
using UnityEngine;

public class Mode2Dto3D : IMode
{
    private WallController _wc;
    private PointPlacer _pp;
    private GridCreator _gc;
    private ConstrDrawer _cd;
    private MeshBuilder _mb;
    private WallGenerator _wg;
    public PlayerController PCref { get; private set; }

    private void _AddPointProjection()
    {
        PointINFO pointInfo = _pp.HandleAddingPoint();
        if (pointInfo != PointINFO.Empty) {
            Debug.Log(pointInfo.ToString());
            //PointsList.infoList.Add(pointInfo);
        }
    }

    private void _RemovePointProjection()
    {
        PointINFO pointInfo = _pp.HandleRemovingPoint();
        if (pointInfo != PointINFO.Empty) {
            Debug.Log(pointInfo.ToString());
            //PointsList.infoList.Remove(pointInfo);
        }
    }

    private void _AddEdgeProjection()
    {
        EdgeINFO edgeInfo = _pp.HandleAddingEdge();
        if (edgeInfo != EdgeINFO.Empty) {
            Debug.Log(edgeInfo.ToString());
        }
    }

    private void _RemoveEdgeProjection()
    {
        EdgeINFO edgeInfo = _pp.HandleRemovingEdge();
        if (edgeInfo != EdgeINFO.Empty) {
            Debug.Log(edgeInfo.ToString());
        }
    }

    private void _MakeActionOnWall()
    {
        if (PCref.Hit.collider != null)
        {
            Debug.Log($"[CLICK] on object named: {PCref.Hit.collider.gameObject.name}");
            if (PCref.Hit.collider.tag == "PointButton")
            {
                _wg.points.Add(PointsList.AddPointToVerticesList(PCref.Hit.collider.gameObject));
                //PointINFO pointInfo = PointsList.RemovePointOnClick(PCref.Hit.collider.gameObject);
                //_pp.RemovePoint(pointInfo);
            }
            else if (PCref.Hit.collider.gameObject.name == "UpButton")
            {
                PointsList.PointListGoUp();
            }
            else if (PCref.Hit.collider.gameObject.name == "DownButton")
            {
                PointsList.PointListGoDown();
            }
            else if (PCref.Hit.collider.gameObject.name == "GenerateButton")
            {
                _wg.GenerateWall(_wg.points);
            }
        }
    }

    private void _SaveSolidAndSwitchToMode3Dto2D()
    {
        GameObject mainObject = GameObject.Find("MainObject");
        //export solid
        string solid = SolidExporter.ExportSolid(_mb.GetPoints3D(), _mb.GetEdges3D(),WallGenerator.GetFaces());
        if(solid == null)
        {
            Debug.LogError("Error - save failed");
        }
        //Grid Clear powoduje usuniecie siatki i wszystkich rzutow punktow
        _gc.Clear();
        GameObject.Destroy(_gc);

        //clear meshBuilder usuwa pkty 3D,krawedzie 3d,linie rzutujace,odnoszace
        _mb.ClearAndDisable();
        GameObject.Destroy(_mb);
        //clear PointPlacer usuwa krawedzie 2d oraz kursor
        _pp.Clear();
        GameObject.Destroy(_pp);

        _cd.Clear();
        GameObject.Destroy(_cd);
        //Hide point list
        PointsList.HideListAndLogs();
        ///Zaladuj grupowy
        PCref.ChangeMode(PlayerController.Mode.Mode3Dto2D);
        
        SolidImporter si = mainObject.AddComponent<SolidImporter>();
        si.Init();
        if(si == null)
        {
            Debug.LogError("Error - cannot find SolidImporter");
            return;
        }
        _wc.SetBasicWalls();
        _wc.SetDefaultShowRules();
        si.SetUpDirection();
        si.ImportSolid(solid);
    }

    private void _ChoosePreviousLabel()
    {
        _pp.PreviousLabel();
    }

    private void _ChooseNextLabel()
    {
        _pp.NextLabel();
    }

    public Mode2Dto3D(PlayerController pc)
    {
        PCref = pc;
        GameObject wallsObject = GameObject.Find("Walls");
        _wc = wallsObject.GetComponent<WallController>();

        GameObject mainObject = GameObject.Find("MainObject");
        _mb = mainObject.AddComponent<MeshBuilder>();
        _mb.Init(true, true);

        _gc = wallsObject.AddComponent<GridCreator>();
        _gc.Init();

        _cd = wallsObject.AddComponent<ConstrDrawer>();
        _cd.Init();

        _wg = mainObject.AddComponent<WallGenerator>();

        _pp = PCref.gameObject.AddComponent<PointPlacer>();
        _pp.Init(_mb, _cd);
        _pp.CreateCursor();
        _pp.MoveCursor(PCref.Hit);
        
        SetUpFlystick();

        //if (PointsList.ceilingWall != null) //nie dziala, dalej sie psuje
        //{
        //    Debug.Log("ceiling found");
        //    PointsList.ceilingWall.SetActive(true);
        //}

        PointsList.ShowListAndLogs();

        Debug.Log($"<color=blue> MODE inzynierka ON </color>");
    }

    public void SetUpFlystick()
    {
        return;

        FlystickController.ClearActions();

        FlystickController.SetAction(
            FlystickController.Btn._1, 
            FlystickController.ActOn.PRESS, 
            () => _AddPointProjection()
        );

        FlystickController.SetAction(
            FlystickController.Btn._2, 
            FlystickController.ActOn.PRESS, 
            () => _RemovePointProjection()
        );

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK, 
            FlystickController.ActOn.TILT_LEFT, 
            () => _ChoosePreviousLabel()
        );

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK, 
            FlystickController.ActOn.TILT_RIGHT, 
            () => _ChooseNextLabel()
        );
    }

    public void HandleInput()
    {
        _pp.MoveCursor(PCref.Hit);

        if (Input.GetKeyDown("1"))
        {
            _AddPointProjection();
        }

        if (Input.GetKeyDown("2"))
        {
            _RemovePointProjection();
        }

        if (Input.GetKeyDown("3"))
        {
            _AddEdgeProjection();
        }

        if (Input.GetKeyDown("4"))
        {
            _RemoveEdgeProjection();
        }

        if (Input.GetKeyDown("5"))
        {
            _MakeActionOnWall();
        }

        if (Input.GetKeyDown("6"))
        {
            _SaveSolidAndSwitchToMode3Dto2D();
        }

        if (Input.GetKeyDown("7"))
        {
            _ChoosePreviousLabel();
        }

        if (Input.GetKeyDown("8"))
        {
            _ChooseNextLabel();
        }

        if (Input.GetKeyDown("9"))
        {
            _pp.HandleAddingCircle();
        }

        if (Input.GetKeyDown("0"))
        {
            _pp.HandleAddingLine();
        }

        if (Input.GetKeyDown("g"))
        {
            _wg.GenerateWall(_wg.points);
        }
    }

}
