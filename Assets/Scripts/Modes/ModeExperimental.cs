using Assets.Scripts.Experimental;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Experimental.Items;
using UnityEngine;

public class ModeExperimental : IMode
{
    public PlayerController PCref { get; private set; }

    private WallController _wc;

    private ItemsController _items;

    private enum Ctx
    {
        Idle,
        Point,
        LineBetweenPoints,
        Line
    }

    private Ctx _ctx = Ctx.Idle;

    private Action<WallInfo, Vector3> _drawLineAction;
    private Action<Vector3, bool> _drawLineBetweenPointsAction;

    private IRaycastable _hitObject;

    private void _MakeActionOnWall()
    {
        switch (_ctx)
        {
            case Ctx.Point:
            {
                WallInfo hitWall = _wc.GetWallByName(PCref.Hit.collider.gameObject.name);
                Vector3 pos = PCref.Hit.point;
                if (hitWall != null)
                {
                    _items.AddPoint(hitWall, pos);
                }
            }
                break;

            case Ctx.LineBetweenPoints:
            {
                var hitPoint = _hitObject as Assets.Scripts.Experimental.Items.Point;
                if (hitPoint != null)
                {
                    if (_drawLineBetweenPointsAction == null)
                    {
                        Vector3 from = hitPoint.Position;
                        _drawLineBetweenPointsAction = _items.AddLineBetweenPoints(from);
                    }
                    else
                    {
                        Vector3 to = hitPoint.Position;
                        _drawLineBetweenPointsAction(to, true);
                        _drawLineBetweenPointsAction = null;
                    }
                }
            }
                break;

            case Ctx.Line:
            {
                WallInfo hitWall = _wc.GetWallByName(PCref.Hit.collider.gameObject.name);
                if (hitWall != null)
                {
                    if (_drawLineAction == null)
                    {
                        Vector3 from = PCref.Hit.point;
                        _drawLineAction = _items.AddLine(hitWall, from);
                    }
                    else
                    {
                        Vector3 to = PCref.Hit.point;
                        _drawLineAction(hitWall, to);
                        _drawLineAction = null;
                    }
                }
            }
                break;

            default:
                break;
        }
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

        _drawLineBetweenPointsAction?.Invoke(hitPoint, false);
        _drawLineAction?.Invoke(hitWall, hitPoint);
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
            _ctx = Ctx.Idle;
            //_drawAction = null;
            // _ctx = string.Empty;
            // _drawAction = null;
            // Debug.Log("MODE ---");
        }

        if (Input.GetKeyDown("2"))
        {
            _ctx = Ctx.Point; 
            //_drawAction = null;
            // _ctx = "LINE";
            // Debug.Log("MODE LINE");
        }

        if (Input.GetKeyDown("3"))
        {
            _ctx = Ctx.LineBetweenPoints;
            //_drawAction = null;
            // _ctx = "LABEL";
            // _drawAction = null;
            // Debug.Log("MODE LABEL");
        }

        if (Input.GetKeyDown("4"))
        {
            _ctx = Ctx.Line;
            // _ctx = "POINT";
            // _drawAction = null;
            // Debug.Log("MODE POINT");
        }


        if (Input.GetKeyDown("5"))
        {
            _MakeActionOnWall();
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