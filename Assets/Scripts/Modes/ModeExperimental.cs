using Assets.Scripts.Experimental;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Experimental.Items;
using Assets.Scripts.Experimental.Utils;
using UnityEngine;

using ExPoint = Assets.Scripts.Experimental.Items.Point;

public class ModeExperimental : IMode
{
    public PlayerController PCref { get; private set; }

    private WallController _wc;

    private ItemsController _items;

    private CircularIterator<KeyValuePair<ExContext, Action>> _context;
    private ExContextMenuView _contextMenuView;
    private Action<WallInfo, Vector3, bool> _drawLineAction;
    private Action<WallInfo, ExPoint, Vector3, bool> _drawLineBetweenPointsAction;
    private Action<WallInfo, Vector3, bool> _drawCircleAction;
    private Action<WallInfo, Vector3, bool> _drawProjectionAction;

    private IRaycastable _hitObject;


    /* * * * CONTEXT ACTIONS begin * * * */

    private void ActPoint()
    {
        WallInfo hitWall = _wc.GetWallByName(PCref.Hit.collider.gameObject.name);
        Vector3 pos = PCref.Hit.point;
        if (hitWall != null)
        {
            _items.AddPoint(hitWall, pos);
        }
    }

    private void ActLineBetweenPoints()
    {
        var hitPoint = _hitObject as ExPoint;
        if (hitPoint == null) return;
        
        if (_drawLineBetweenPointsAction == null)
        {
            _drawLineBetweenPointsAction = _items.AddLineBetweenPoints(hitPoint.Plane, hitPoint, hitPoint.Position);
        }
        else
        {
            _drawLineBetweenPointsAction(hitPoint.Plane, hitPoint, hitPoint.Position, true);
            _drawLineBetweenPointsAction = null;
        }
    }

    private void ActCircle()
    {
        var hitPoint = _hitObject as ExPoint;
        if (hitPoint == null) return;
        
        if (_drawCircleAction == null)
        {
            _drawCircleAction = _items.AddCircle(hitPoint.Plane, hitPoint.Position);
        }
        else
        {
            _drawCircleAction(hitPoint.Plane, hitPoint.Position, true);
            _drawCircleAction = null;
        }
    }

    private void ActLine()
    {
        WallInfo hitWall = _wc.GetWallByName(PCref.Hit.collider.gameObject.name);
        if (hitWall == null) return;
        
        if (_drawLineAction == null)
        {
            Vector3 from = PCref.Hit.point;
            _drawLineAction = _items.AddLine(hitWall, from);
        }
        else
        {
            Vector3 to = PCref.Hit.point;
            _drawLineAction(hitWall, to, true);
            _drawLineAction = null;
        }
    }

    private void ActProjection()
    {
        var hitPoint = _hitObject as ExPoint;
        WallInfo hitWall = _wc.GetWallByName(PCref.Hit.collider.gameObject.name);

        if (hitPoint != null && _drawProjectionAction == null)
        {
            _drawProjectionAction = _items.AddProjection(hitPoint.Plane, hitPoint.Position);
        }
        else if (hitWall != null && _drawProjectionAction != null)
        {
            _drawProjectionAction(hitWall, PCref.Hit.point, true);
            _drawProjectionAction = null;
        }
    }

    /* * * * CONTEXT ACTIONS end * * * */


    private void _MakeAction()
    {
        _context.Current.Value();
    }

    private void _MoveCursor()
    {
        var hitObject = PCref.Hit.collider.gameObject.GetComponent<IRaycastable>();
        var hitWall = _wc.GetWallByName(PCref.Hit.collider.gameObject.name);
        var hitPoint = PCref.Hit.point;

        if (_hitObject != hitObject)
        {
            _hitObject?.OnHoverExit();
            _hitObject = hitObject;
            _hitObject?.OnHoverEnter();
        }

        _drawLineBetweenPointsAction?.Invoke(hitWall, default(ExPoint), hitPoint, false);
        _drawCircleAction?.Invoke(hitWall, hitPoint, false);
        _drawLineAction?.Invoke(hitWall, hitPoint, false);
        _drawProjectionAction?.Invoke(hitWall, hitPoint, false);
    }

    public ModeExperimental(PlayerController pc)
    {
        PCref = pc;
        _wc = GameObject.Find("Walls").GetComponent<WallController>();

        _drawLineAction = null;
        _drawLineBetweenPointsAction = null;
        _hitObject = null;

        _items = new ItemsController();
        _items.AddAxisBetweenPlanes(_wc.GetWallByName("Wall4"), _wc.GetWallByName("Wall3"));

        var mc = (MeshBuilder)UnityEngine.Object.FindObjectOfType(typeof(MeshBuilder));
        mc.SetGenerateRulesReferenceLine(false);

        _context = new CircularIterator<KeyValuePair<ExContext, Action>>(
            new List<KeyValuePair<ExContext, Action>>()
            {
                new KeyValuePair<ExContext, Action>(ExContext.Point, ActPoint),
                new KeyValuePair<ExContext, Action>(ExContext.LineBetweenPoints, ActLineBetweenPoints),
                new KeyValuePair<ExContext, Action>(ExContext.Line, ActLine),
                new KeyValuePair<ExContext, Action>(ExContext.Circle, ActCircle),
                new KeyValuePair<ExContext, Action>(ExContext.Projection, ActProjection)
            },
            new KeyValuePair<ExContext, Action>(ExContext.Idle, () => {}));

        _contextMenuView = new ExContextMenuView();
        _contextMenuView.SetCurrentContext(_context.Current.Key);

        Debug.Log($"<color=blue> MODE experimental ON </color>");
    }

    public void HandleInput()
    {
        _MoveCursor();

        // List<Collider> overlappingColliders = Physics.OverlapSphere(PCref.Hit.point, 0.007f).ToList();
        // if (overlappingColliders.Select(o => o.gameObject.name == "LINE").Count() > 1)
        // {
        //     Debug.Log("Multiple colliders intersect.");
        // }


        if (Input.GetKeyDown("1"))
        {
            _context.Next();
            _contextMenuView.SetCurrentContext(_context.Current.Key);
        }

        if (Input.GetKeyDown("2"))
        {
            _MakeAction();
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