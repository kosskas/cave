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

    private MeshBuilder _mc;

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

    private void _MakeAction()
    {
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
        else
        {
            _context.Current.Value();
        }
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

        SetUpFlystick();

        _drawAction = null;
        _hitObject = null;

        _items = new ItemsController();
        _items.AddAxisBetweenPlanes(_wc.GetWallByName("Wall4"), _wc.GetWallByName("Wall3"));
        GameObject mainObject = GameObject.Find("MainObject");
        _mc = mainObject.AddComponent<MeshBuilder>();
        _mc.Init(true,false);
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

        //_controlMenuView = new ExControlMenuView();

        //_wallBuilderView = new ExWallBuilderView();

        PointsList.ShowListAndLogs();

        Debug.Log($"<color=blue> MODE experimental ON </color>");
    }

    public void HandleInput()
    {
        _MoveCursor();

            if (Input.GetKeyDown("1"))
            {
                _ChangeDrawContext();
            }

            if (Input.GetKeyDown("2"))
            {
                _MakeAction();
            }

            if (Input.GetKeyDown("3"))
            {
                _DeleteHoveredObject();
            }

            if (Input.GetKeyDown("5"))
            {
                _TryAddLabel();
            }

            if (Input.GetKeyDown("6"))
            {
                _TryRemoveFocusedLabel();
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

    public void SetUpFlystick()
    {
        FlystickController.ClearActions();

        FlystickController.SetAction(
            FlystickController.Btn._1,
            FlystickController.ActOn.PRESS,
            () => _MakeAction()
        );

        FlystickController.SetAction(
            FlystickController.Btn._2,
            FlystickController.ActOn.PRESS,
            () => _DeleteHoveredObject()
        );

        FlystickController.SetAction(
            FlystickController.Btn._3,
            FlystickController.ActOn.PRESS,
            () => _TryAddLabel()
        );

        FlystickController.SetAction(
            FlystickController.Btn._4,
            FlystickController.ActOn.PRESS,
            () => _TryRemoveFocusedLabel()
        );


        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_LEFT,
            () => _TryGetPrevLabelText()
        );

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_RIGHT,
            () => _TryGetNextLabelText()
        );

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_UP,
            () => _TryGetNextLabel()
        );

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_DOWN,
            () => _ChangeDrawContext()
        );

    }
}