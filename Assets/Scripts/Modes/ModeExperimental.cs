using Assets.Scripts.Experimental;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Experimental.Items;
using Assets.Scripts.Experimental.Utils;
using UnityEngine;

public class ModeExperimental : IMode
{
    public PlayerController PCref { get; private set; }

    private WallController _wc;

    private MeshBuilder _mb;

    private WallGenerator _wg;

    private ItemsController _items;

    private CircularIterator<KeyValuePair<ExContext, Action>> _context;

    private ExContextMenuView _contextMenuView;
    private ExControlMenuView _controlMenuView;
    private ExWallBuilderView _wallBuilderView;

    private IRaycastable _hitObject;

    private DrawAction _drawAction;
    
    /* * * * CONTEXT ACTIONS begin * * * */

    private void Act()
    {
        var hitObject = _hitObject;
        var hitPosition = PCref.Hit.point;
        var hitWall = _wc.GetWallByName(PCref.Hit.collider.gameObject.name);

        if (_drawAction == null)
        {
            _drawAction = _items.Add(_context.Current.Key, hitObject, hitPosition, hitWall);
        }
        else
        {
            _drawAction(hitObject, hitPosition, hitWall, true);
            _drawAction = null;
        }
    }

    /* * * * CONTEXT ACTIONS end * * * */

    /* * * * INPUT HANDLERS begin * * * */

    private void _ChangeDrawContext()
    {
        _context.Next();
        _contextMenuView.SetCurrentContext(_context.Current.Key);
    }

    private void _DrawAction()
    {
        _context.Current.Value();
    }

    private void _MakeActionOnWall()
    {
        if (PCref.Hit.collider == null)
            return;

        if (PCref.Hit.collider.tag == "PointButton")
        {
            _wg.points.Add(PointsList.AddPointToVerticesList(PCref.Hit.collider.gameObject));
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
        else if (PCref.Hit.collider.gameObject.name == "SwitchButton")
        {
            _SaveSolidAndSwitchToMode3Dto2D();
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

        if (solid == null)
        {
            Debug.LogError("Error - save failed");
        }

        //czyszcenie œcian obiektu
        _wg.Clear();
        GameObject.Destroy(_wg);

        //Grid Clear powoduje usuniecie siatki i wszystkich rzutow punktow
        _items.Clear();

        //clear meshBuilder usuwa pkty 3D,krawedzie 3d,linie rzutujace,odnoszace
        _mb.ClearAndDisable();
        GameObject.Destroy(_mb);

        //Hide point list
        PointsList.HideListAndLogs();

        ///Zaladuj grupowy
        PCref.ChangeMode(PlayerController.Mode.Mode3Dto2D);

        SolidImporter si = mainObject.AddComponent<SolidImporter>();
        si.Init();
        if (si == null)
        {
            Debug.LogError("Error - cannot find SolidImporter");
            return;
        }
        _wc.SetBasicWalls();
        _wc.SetDefaultShowRules();
        si.SetUpDirection();
        si.ImportSolid(solid);
    }

    private void _DeleteHoveredObject()
    {
        GameObject hitGameObject = null;
        _hitObject?.OnHoverAction(gameObject => hitGameObject = gameObject);
        if (hitGameObject == null) return;

        UnityEngine.Object.Destroy(hitGameObject);

        _hitObject = null;
    }

    private void _TryGetNextLabelText()
    {
        _hitObject?.OnHoverAction((gameObject) =>
        {
            gameObject.GetComponent<ILabelable>()?.NextText();
        });
    }

    private void _TryGetPrevLabelText()
    {
        _hitObject?.OnHoverAction((gameObject) =>
        {
            gameObject.GetComponent<ILabelable>()?.PrevText();
        });
    }

    private void _TryGetNextLabel()
    {
        _hitObject?.OnHoverAction((gameObject) =>
        {
            gameObject.GetComponent<ILabelable>()?.NextLabel();
        });
    }

    private void _TryGetPrevLabel()
    {
        _hitObject?.OnHoverAction((gameObject) =>
        {
            gameObject.GetComponent<ILabelable>()?.PrevLabel();
        });
    }

    private void _TryRemoveFocusedLabel()
    {
        _hitObject?.OnHoverAction((gameObject) =>
        {
            gameObject.GetComponent<ILabelable>()?.RemoveFocusedLabel();
        });
    }

    private void _TryAddLabel()
    {
        _hitObject?.OnHoverAction((gameObject) =>
        {
            gameObject.GetComponent<ILabelable>()?.AddLabel();
        });
    }

    /* * * * INPUT HANDLERS end * * * */


    private void _MoveCursor()
    {
        var hitObject = PCref.Hit.collider.gameObject.GetComponent<IRaycastable>();
        var hitPosition = PCref.Hit.point;
        var hitWall = _wc.GetWallByName(PCref.Hit.collider.gameObject.name);
        
        if (_hitObject != hitObject)
        {
            _hitObject?.OnHoverExit();
            _hitObject = hitObject;
            _hitObject?.OnHoverEnter();
        }

        _drawAction?.Invoke(hitObject, hitPosition, hitWall, false);
    }


    public ModeExperimental(PlayerController pc)
    {
        PCref = pc;
        _wc = GameObject.Find("Walls").GetComponent<WallController>();

        _drawAction = null;
        _hitObject = null;

        _items = new ItemsController();
        _items.AddAxisBetweenPlanes(_wc.GetWallByName("Wall4"), _wc.GetWallByName("Wall3"));
        GameObject mainObject = GameObject.Find("MainObject");
        _mb = mainObject.AddComponent<MeshBuilder>();
        _mb.Init(true,false);
        _wg = mainObject.AddComponent<WallGenerator>();

        _context = new CircularIterator<KeyValuePair<ExContext, Action>>(
            new List<KeyValuePair<ExContext, Action>>()
            {
                new KeyValuePair<ExContext, Action>(ExContext.Idle, () => {}),
                new KeyValuePair<ExContext, Action>(ExContext.Point, Act),
                new KeyValuePair<ExContext, Action>(ExContext.BoldLine, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Line, Act),
                new KeyValuePair<ExContext, Action>(ExContext.PerpendicularLine, Act),
                new KeyValuePair<ExContext, Action>(ExContext.ParallelLine, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Circle, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Projection, Act)
            });

        _contextMenuView = new ExContextMenuView();
        _contextMenuView.SetCurrentContext(_context.Current.Key);

        _controlMenuView = new ExControlMenuView();

        _wallBuilderView = new ExWallBuilderView();

        PointsList.ShowListAndLogs();

        Debug.Log($"<color=blue> MODE experimental ON </color>");
    }

    public void HandleInput()
    {
        _MoveCursor();

        if (Input.GetKeyDown("1"))
        {
            _DrawAction();
        }

        if (Input.GetKeyDown("2"))
        {
            _DeleteHoveredObject();
        }

        if (Input.GetKeyDown("3"))
        {
            _TryAddLabel();
        }

        if (Input.GetKeyDown("4"))
        {
            _TryRemoveFocusedLabel();
        }
        
        if (Input.GetKeyDown("5"))
        {
            _MakeActionOnWall();
        }

        if (Input.GetKeyDown("6"))
        {
            _TryGetPrevLabel();
        }

        if (Input.GetKeyDown("7"))
        {
            _TryGetNextLabel();
        }

        if (Input.GetKeyDown("8"))
        {
            _TryGetPrevLabelText();
        }

        if (Input.GetKeyDown("9"))
        {
            _TryGetNextLabelText();
        }
    }
}

/*
 *  |           ACTION          |       DEV     |               LZWP                |
 *  | _DrawAction               |       1       |   Btn._1 ActOn.PRESS              |
 *  | _DeleteHoveredObject      |       2       |   Btn._2 ActOn.PRESS              |
 *  | _TryAddLabel              |       3       |   Btn._3 ActOn.PRESS              |
 *  | _TryRemoveFocusedLabel    |       4       |   Btn._4 ActOn.PRESS              |
 *  | _MakeActionOnWall         |       5       |   Btn.FIRE ActOn.PRESS            |
 *  | _TryGetPrevLabel          |       6       |   Btn.JOYSTICK ActOn.TILT_LEFT    |
 *  | _TryGetNextLabel          |       7       |   Btn.JOYSTICK ActOn.TILT_RIGHT   |
 *  | _TryGetPrevLabelText      |       8       |   Btn.JOYSTICK ActOn.TILT_DOWN    |
 *  | _TryGetNextLabelText      |       9       |   Btn.JOYSTICK ActOn.TILT_UP      |
 */