
using Assets.Scripts.Walls;
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
        if (PCref.Hit.collider == null) 
            return;

        //Debug.Log($"[CLICK] on object named: {PCref.Hit.collider.gameObject.name}");

        if (PCref.Hit.collider.tag == "PointButton")
        {
            _wg.points.Add(PointsList.AddPointToVerticesList(PCref.Hit.collider.gameObject));
        }
        else switch (PCref.Hit.collider.gameObject.name)
        {
            case "UpButton":
                PointsList.PointListGoUp();
                break;

            case "DownButton":
                PointsList.PointListGoDown();
                break;

            case "GenerateButton":
                _wg.GenerateWall(_wg.points);
                break;

            case "ExportSolidToVisualButton":
                _SaveSolidAndSwitchToMode3Dto2D();
                break;
        }
    }
    
    private void _SaveSolidAndSwitchToMode3Dto2D()
    {
        GameObject mainObject = GameObject.Find("MainObject");

        //export solid
        string solid = SolidExporter.ExportSolid(
            _mb.GetPoints3D(), 
            _mb.GetEdges3D(),
            WallGenerator.GetFaces());

        if(solid == null)
        {
            Debug.LogError("Error - save failed");
        }

        //czyszcenie œcian obiektu
        _wg.Clear();
        GameObject.Destroy(_wg);

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
        UIWall.ExportSolidToVisualButton.Hide();

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

        PointsList.ShowListAndLogs();

        UIWall.ExportSolidToVisualButton.Show();

        Debug.Log($"<color=blue> MODE inzynierka ON </color>");
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
            _ChoosePreviousLabel();
        }

        if (Input.GetKeyDown("7"))
        {
            _ChooseNextLabel();
        }

        if (Input.GetKeyDown("c"))
        {
            _pp.HandleAddingCircle();
        }

        if (Input.GetKeyDown("l"))
        {
            _pp.HandleAddingLine();
        }

    }

}

/*
 *  |           ACTION          |       DEV     |               LZWP                |
 *  | _AddPointProjection       |       1       |   Btn._1 ActOn.PRESS              |     
 *  | _RemovePointProjection    |       2       |   Btn._2 ActOn.PRESS              |
 *  | _AddEdgeProjection        |       3       |   Btn._3 ActOn.PRESS              |
 *  | _RemoveEdgeProjection     |       4       |   Btn._4 ActOn.PRESS              |
 *  | _MakeActionOnWall         |       5       |   Btn.FIRE ActOn.PRESS            |
 *  | _ChoosePreviousLabel      |       6       |   Btn.JOYSTICK ActOn.TILT_LEFT    |
 *  | _ChooseNextLabel          |       7       |   Btn.JOYSTICK ActOn.TILT_RIGHT   |
 *  | _pp.HandleAddingCircle    |       c       |   ?                               |
 *  | _pp.HandleAddingLine      |       l       |   ?                               |
 */
