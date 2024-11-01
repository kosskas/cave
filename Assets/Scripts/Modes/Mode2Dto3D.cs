
using UnityEngine;

public class Mode2Dto3D : IMode
{
    private WallController _wc;
    private PointPlacer _pp;
    private GridCreator _gc;

    public PlayerController PCref { get; private set; }

    private void _AddPointProjection()
    {
        PointINFO pointInfo = _pp.HandleAddingPoint();
        if (pointInfo != PointINFO.Empty) {
            Debug.Log(pointInfo.ToString());
            PointsList.infoList.Add(pointInfo);
        }
    }

    private void _RemovePointProjection()
    {
        PointINFO pointInfo = _pp.HandleRemovingPoint();
        if (pointInfo != PointINFO.Empty) {
            Debug.Log(pointInfo.ToString());
            PointsList.infoList.Remove(pointInfo);
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
                PointINFO pointInfo = PointsList.RemovePointOnClick(PCref.Hit.collider.gameObject);
                _pp.RemovePoint(pointInfo);
            }
            else if (PCref.Hit.collider.gameObject.name == "UpButton")
            {
                PointsList.PointListGoUp();
            }
            else if (PCref.Hit.collider.gameObject.name == "DownButton")
            {
                PointsList.PointListGoDown();
            }
        }
    }

    private void _SaveSolidAndSwitchToMode3Dto2D()
    {   
        MeshBuilder mb = (MeshBuilder)GameObject.FindObjectOfType<MeshBuilder>();
        //export solid
        string solid = SolidExporter.ExportSolid(mb.GetPoints3D(), mb.GetEdges3D());
        if(solid == null)
        {
            Debug.LogError("Error - save failed");
        }
        //Grid Clear powoduje usuniecie siatki i wszystkich rzutow punktow
        _gc.Clear();
        //clear meshBuilder usuwa pkty 3D,krawedzie 3d,linie rzutujace,odnoszace
        mb.ClearAndDisable();
        //clear PointPlacer usuwa krawedzie 2d oraz kursor
        _pp.Clear();
        //Hide point list
        PointsList.HideListAndLogs();
        ///Zaladuj grupowy
        PCref.ChangeMode(PlayerController.Mode.Mode3Dto2D);
        
        SolidImporter si = (SolidImporter)GameObject.FindObjectOfType<SolidImporter>();
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

        

        _gc = wallsObject.GetComponent<GridCreator>();
        _gc.Init();

        
        

        _pp = PCref.gameObject.GetComponent<PointPlacer>();
        _pp.CreateCursor();
        _pp.MoveCursor(PCref.Hit);

        if (PointsList.ceilingWall != null) //nie dziala, dalej sie psuje
        {
            Debug.Log("ceiling found");
            PointsList.ceilingWall.SetActive(true);
        }

        PointsList.ShowListAndLogs();

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
    }

}
