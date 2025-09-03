using Assets.Scripts.Experimental;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts;
using Assets.Scripts.Experimental.Items;
using Assets.Scripts.Experimental.Utils;
using UnityEngine;
using Assets.Scripts.Walls;

public class ModeExperimental : IMode
{
    public PlayerController PCref { get; private set; }

    private WallController _wc;

    private MeshBuilder _mb;

    private FacesGenerator _fc;

    private WallCreator _wcrt;

    private ItemsController _items;

    private CircularIterator<KeyValuePair<ExContext, Action>> _context;

    private ExContextMenuView _contextMenuView;
    private ExControlMenuView _controlMenuView;

    private IRaycastable _hitObject;

    private DrawAction _drawAction;

    private RadialMenu radialMenu;
    
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

    public void _ChangeDrawContextNext()
    {
        _context.Next();
        _contextMenuView.SetCurrentContext(_context.Current.Key);
    }

    public void _ChangeDrawContextPrev()
    {
        _context.Previous();
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
            _fc.points.Add(PointsList.AddPointToVerticesList(PCref.Hit.collider.gameObject));
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
                _fc.GenerateFace(_fc.points);
                break;

            case "SwitchButton":
                _SaveSolidAndSwitchToMode3Dto2D();
                break;

            case "NextContext":
                _context.Next();
                _contextMenuView.SetCurrentContext(_context.Current.Key);
                break;

            case "PrevContext":
                _context.Previous();
                _contextMenuView.SetCurrentContext(_context.Current.Key);
                break;

            case "ExportSolidToVisualButton":
                _SaveSolidAndSwitchToMode3Dto2D();
                break;

            case "BackToMenuButton":
                _BackToMenu();
                break;
            
            case "SaveStateButton":
                _items.Save();
                break;

            case "LoadStateButton":
                _items.Restore();
                break;

        }
    }

    private void _BackToMenu()
    {
        //czyszcenie œcian obiektu
        _fc.Clear();
        GameObject.Destroy(_fc);

        //Grid Clear powoduje usuniecie siatki i wszystkich rzutow punktow
        _items.Clear();

        //clear meshBuilder usuwa pkty 3D,krawedzie 3d,linie rzutujace,odnoszace
        _mb.ClearAndDisable();
        GameObject.Destroy(_mb);

        //Hide point list
        PointsList.HideListAndLogs();

        _wc.SetBasicWalls();
        //delete menu
        GameObject.Destroy(radialMenu.gameObject); 
        radialMenu = null;

        //Hide context view
        ExContextMenuView.Hide();
        UIWall.ExportSolidToVisualButton.Hide();
        UIWall.BackToMenuButton.Hide();

        ///Zaladuj grupowy
        PCref.ChangeMode(PlayerController.Mode.ModeMenu);
    }

    private void _SaveSolidAndSwitchToMode3Dto2D()
    {
        GameObject mainObject = GameObject.Find("MainObject");

        //export solid
        string solid = SolidExporter.ExportSolid(
            _mb.GetPoints3D(), 
            _mb.GetEdges3D(), 
            FacesGenerator.GetFaces());

        if (solid == null)
        {
            Debug.LogError("Error - save failed");
        }

        //czyszcenie œcian obiektu
        _fc.Clear();
        GameObject.Destroy(_fc);

        //Grid Clear powoduje usuniecie siatki i wszystkich rzutow punktow
        _items.Clear();

        //clear meshBuilder usuwa pkty 3D,krawedzie 3d,linie rzutujace,odnoszace
        _mb.ClearAndDisable();
        GameObject.Destroy(_mb);

        //Hide point list
        PointsList.HideListAndLogs();

        //Hide context view
        ExContextMenuView.Hide();
        UIWall.ExportSolidToVisualButton.Hide();
        UIWall.BackToMenuButton.Hide();
        UIWall.SaveLoadStateButtons.Hide();


        //delete menu
        GameObject.Destroy(radialMenu.gameObject);
        radialMenu = null;

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
        //Debug.Log("Objekt " + hitGameObject.name);
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

        _wcrt = PCref.gameObject.GetComponent<WallCreator>();

        GameObject mainObject = GameObject.Find("MainObject");
        _mb = mainObject.AddComponent<MeshBuilder>();
        _mb.Init(true);
        _fc = mainObject.AddComponent<FacesGenerator>();

        _items = new ItemsController(_wc, _wcrt, _fc);
        _items.AddAxisBetweenPlanes(_wc.GetWallByName("Wall4"), _wc.GetWallByName("Wall3"));
        _items.AddAxisBetweenPlanes(_wc.GetWallByName("Wall6"), _wc.GetWallByName("Wall3"));
        _items.AddAxisBetweenPlanes(_wc.GetWallByName("Wall4"), _wc.GetWallByName("Wall6"));

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
                new KeyValuePair<ExContext, Action>(ExContext.Projection, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Wall, Act)
            });

        _contextMenuView = new ExContextMenuView();
        _contextMenuView.SetCurrentContext(_context.Current.Key);

        UIWall.ExportSolidToVisualButton.Show();
        UIWall.BackToMenuButton.Show();
        UIWall.SaveLoadStateButtons.Show();

        //_controlMenuView = new ExControlMenuView();

        PointsList.ShowListAndLogs();


        GameObject canvas = GameObject.Find("FlystickPlaceholder/Canvas");
        if (canvas != null)
        {
            Transform radialMenuRoot = canvas.transform.Find("RadialMenuRoot");
            if (radialMenuRoot != null)
            {
                GameObject itemPrefab = Resources.Load<GameObject>("RadialMenuItem");
                if (itemPrefab != null)
                {
                    List<string> descriptions = Enum.GetValues(typeof(ExContext)).Cast<ExContext>().Select(e => e.GetDescription()).ToList();
                    this.radialMenu = RadialMenu.Create(
                        radialMenuRoot,
                        descriptions.Count, // liczba elementów
                        20f, // promieñ
                        descriptions, // etykiety
                        itemPrefab,
                        this
                    );
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
            Debug.LogError("Nie znaleziono Canvas w scenie");
        }

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

        if (Input.GetKeyDown("l"))
        {
            _wc.RemoveWall(_wc.FindWallInfoByGameObject(PCref.Hit.collider.gameObject));
        }

        if (Input.GetKeyDown("p"))
        {
            _wc.PopBackWall();
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