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

    private ItemsController _items;

    private CircularIterator<KeyValuePair<ExContext, Action>> _context;

    private ExContextMenuView _contextMenuView;
    private ExControlMenuView _controlMenuView;

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

    private void _DeleteHoveredObject()
    {
        GameObject hitGameObject = null;
        _hitObject?.OnHoverAction(gameObject => hitGameObject = gameObject);
        if (hitGameObject == null) return;

        UnityEngine.Object.Destroy(hitGameObject);

        _hitObject = null;
    }

    private void _MakeAction()
    {
        _context.Current.Value();
    }

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
        var mc = mainObject.AddComponent<MeshBuilder>();
        mc.Init(true,false);

        _context = new CircularIterator<KeyValuePair<ExContext, Action>>(
            new List<KeyValuePair<ExContext, Action>>()
            {
                new KeyValuePair<ExContext, Action>(ExContext.Point, Act),
                new KeyValuePair<ExContext, Action>(ExContext.BoldLine, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Line, Act),
                new KeyValuePair<ExContext, Action>(ExContext.PerpendicularLine, Act),
                new KeyValuePair<ExContext, Action>(ExContext.ParallelLine, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Circle, Act),
                new KeyValuePair<ExContext, Action>(ExContext.Projection, Act)
            },
            new KeyValuePair<ExContext, Action>(ExContext.Idle, () => {}));

        _contextMenuView = new ExContextMenuView();
        _contextMenuView.SetCurrentContext(_context.Current.Key);

        _controlMenuView = new ExControlMenuView();

        Debug.Log($"<color=blue> MODE experimental ON </color>");
    }

    public void HandleInput()
    {
        _MoveCursor();

        if (Input.GetKeyDown("1"))
        {
            _context.Next();
            _contextMenuView.SetCurrentContext(_context.Current.Key);
        }

        if (Input.GetKeyDown("2"))
        {
            _MakeAction();
        }

        if (Input.GetKeyDown("3"))
        {
            _DeleteHoveredObject();
        }

        if (Input.GetKeyDown("8"))
        {
            _hitObject?.OnHoverAction((gameObject) =>
            {
                gameObject.GetComponent<ILabelable>()?.PrevLabel();
            });
        }

        if (Input.GetKeyDown("9"))
        {
            _hitObject?.OnHoverAction((gameObject) =>
            {
                gameObject.GetComponent<ILabelable>()?.NextLabel();
            });
        }
    }
}