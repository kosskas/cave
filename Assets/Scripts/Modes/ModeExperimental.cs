using Assets.Scripts.Experimental;
using System;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Experimental.Items;
using Assets.Scripts.Experimental.Utils;
using Assets.Scripts.FileManagers;
using UnityEngine;
public class ModeExperimental : IMode
{
    private static float Z_RADIAL_MENU_OFFSET = ( GameObject.Find("TrackedObject") != null ? 0.0f : 0.55f );
    private static float Y_RADIAL_MENU_OFFSET = ( GameObject.Find("TrackedObject") != null ? 0.0f : -0.30f );
    private static float RADIAL_1ST_MENU_RADIUS = 15f;
    private static float RADIAL_2ND_MENU_RADIUS = 25f;
    public PlayerController PCref { get; private set; }

    private WallController _wc;

    private MeshBuilder _mb;

    private FacesGenerator _fc;

    private WallCreator _wcrt;

    private ItemsController _items;

    private CircularIterator<KeyValuePair<ExContext, Action>> _context;
    private CircularIterator<KeyValuePair<ExContext, Action>> _creationCtx;
    private CircularIterator<KeyValuePair<ExContext, Action>> _optCtx;

    private IRaycastable _hitObject;

    private DrawAction _drawAction;

    private RadialMenu radialMenu;

    private IRaycastable _relativeObject;
    
    private Line _relativeLine;

    private bool _showProjLines = true;
    /* * * * CONTEXT ACTIONS begin * * * */

    private void TryColorObject(IRaycastable obj, Color color)
    {
        var line = obj as Line;
        var axis = obj as Axis;

        if (!ReferenceEquals(line, null))
        {
            line.Color = color;
            Debug.Log("FOO: LINE LINE LINE");
        }

        if (!ReferenceEquals(axis, null))
        {
            axis.Color = color;
            Debug.Log("FOO: AXIS AXIS AXIS");
        }
    }

    private void Act()
    {
        var hitObject = _hitObject;
        var hitPosition = PCref.Hit.point;
        var hitWall = _wc.GetWallByName(PCref.Hit.collider.gameObject.name);

        if (_drawAction == null)
        {
            _drawAction = _items.Add(_context.Current.Key, hitObject, hitPosition, hitWall, _relativeObject);
        }
        else
        {
            _drawAction(hitObject, hitPosition, hitWall, true);
            _drawAction = null;

            if (_relativeObject != null)
            {
                TryColorObject(_relativeObject, ReconstructionInfo.NORMAL);

                _relativeObject = null;
            }
        }
    }

    private void ActRelativeToObject()
    {
        if (_relativeObject == null)
        {
            _relativeObject = _hitObject;
            Debug.Log($"ActRelativeToObject: {_relativeObject}");

            TryColorObject(_relativeObject, ReconstructionInfo.MENTIONED);

            return;
        }
        
        Act();
    }

    /* * * * CONTEXT ACTIONS end * * * */

    /* * * * INPUT HANDLERS begin * * * */
    private void _DrawAction()
    {
        _context.Current.Value();
    }

    private void _SaveState()
    {
        StateManager.Exp.Save();
    }

    private void _LoadState()
    {
        //czyszcenie œcian obiektu
        _fc.Clear();

        //Grid Clear powoduje usuniecie siatki i wszystkich rzutow punktow
        _items.Clear();

        //clear meshBuilder usuwa pkty 3D,krawedzie 3d,linie rzutujace,odnoszace
        _mb.ClearAndDisable();
        _mb.Init(true);
        _wc.SetBasicWalls();
        //dodanie bazowej osi rzutuj¹cej
        _AddBaseAxis();

        //wczytanie pliku
        StateManager.Exp.Load();
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

        _wc.SetBasicWalls();
        //delete menu
        GameObject.Destroy(radialMenu.gameObject); 
        radialMenu = null;

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
        _items.RemoveWorkspace();

        //clear meshBuilder usuwa pkty 3D,krawedzie 3d,linie rzutujace,odnoszace
        _mb.ClearAndDisable();
        GameObject.Destroy(_mb);

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
        if (_context.Current.Key == ExContext.Wall)
        {
            _RemoveWall();
            return;
        }

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

    private void _RemoveWall()
    {
        WallInfo wallToRem = _wc.FindWallInfoByGameObject(PCref.Hit.collider.gameObject);
        _wc.RemoveWall(wallToRem);
        _items.RemoveAxis(wallToRem);
    }
    /* * * * INPUT HANDLERS end * * * */


    private void _MoveCursor()
    {
        Debug.Log($"_MoveCursor: {PCref.Hit.collider.gameObject.transform.parent.gameObject} --> {PCref.Hit.collider.gameObject}");
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

    private void _AddBaseAxis()
    {
        var wallConnections = new Dictionary<string, List<string>>
        {
            ["Wall1"] = new List<string> { "Wall2", "Wall3", "Wall4", "Wall5" },
            ["Wall2"] = new List<string> { "Wall3", "Wall5", "Wall6" },
            ["Wall3"] = new List<string> { "Wall4", "Wall6" },
            ["Wall4"] = new List<string> { "Wall5", "Wall6" },
            ["Wall5"] = new List<string> { "Wall6" }
        };

        foreach (var kvp in wallConnections)
        {
            string wall = kvp.Key;
            foreach (var neighbor in kvp.Value)
            {
                _items.AddAxisBetweenPlanes(_wc.GetWallByName(wall),
                    _wc.GetWallByName(neighbor));
            }
        }
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
        _mb.Init(_showProjLines);
        _fc = mainObject.AddComponent<FacesGenerator>();

        _items = new ItemsController(_wc, _wcrt, _fc, _mb);

        //dodanie bazowej osi rzutuj¹cej
        _AddBaseAxis();

        _creationCtx = new CircularIterator<KeyValuePair<ExContext, Action>>(
            new List<KeyValuePair<ExContext, Action>>()
            {
                new KeyValuePair<ExContext, Action>(ExContext.BackToOpt, _BackToBasicCtx),
                new KeyValuePair<ExContext, Action>(ExContext.Point, Act),
                new KeyValuePair<ExContext, Action>(ExContext.BoldLine, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Line, Act),
                new KeyValuePair<ExContext, Action>(ExContext.PerpendicularLine, ActRelativeToObject),
                new KeyValuePair<ExContext, Action>(ExContext.ParallelLine, ActRelativeToObject),
                new KeyValuePair<ExContext, Action>(ExContext.Circle, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Projection, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Wall, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Face, Act),
                new KeyValuePair<ExContext, Action>(ExContext.ProjLine, _SwitchRuleProjectionLine)
            });

        _optCtx = new CircularIterator<KeyValuePair<ExContext, Action>>(
            new List<KeyValuePair<ExContext, Action>>()
            {
                new KeyValuePair<ExContext, Action>(ExContext.Save, _SaveState),
                new KeyValuePair<ExContext, Action>(ExContext.Load, _LoadState),
                new KeyValuePair<ExContext, Action>(ExContext.LoadVisual, _SaveSolidAndSwitchToMode3Dto2D),
                new KeyValuePair<ExContext, Action>(ExContext.BackToMenu, _BackToMenu),
                new KeyValuePair<ExContext, Action>(ExContext.Const, _ChangeToConstrCtx),
            });

        _context = _optCtx;
        
        AddRadialMenu();

        Debug.Log($"<color=blue> MODE experimental ON </color>");
    }

    public CircularIterator<KeyValuePair<ExContext, Action>> GetCtx()
    {
        return _context;
    }

    private void _SwitchRuleProjectionLine()
    {
        _showProjLines = !_showProjLines;
        _mb.SetShowRulesProjectionLine(_showProjLines);
    }
    private void _BackToBasicCtx()
    {
        _context = _optCtx;
        radialMenu.Generate(_context, RADIAL_1ST_MENU_RADIUS);
    }
    private void _ChangeToConstrCtx()
    {
        _context = _creationCtx;
        radialMenu.Generate(_context, RADIAL_2ND_MENU_RADIUS);
    }
    public void AddRadialMenu()
    {
        GameObject flystick = GameObject.Find("TrackedObject");
        GameObject.Find("NextContext")?.SetActive(false);
        GameObject.Find("PrevContext")?.SetActive(false);
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
                    _BackToBasicCtx();
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
            _RemoveWall();
        }

        if (Input.GetKeyDown("m"))
        {
            radialMenu.ToggleRadialMenuActive();
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