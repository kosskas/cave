using Assets.Scripts.Experimental;
using System;
using Assets.Scripts.Experimental.Items;
using UnityEngine;

public class ModeExperimental : IMode
{
    public PlayerController PCref { get; private set; }

    private WallController _wc;
    private ItemsController _items;
    //private Drawer _drawer;

    private string _ctx = string.Empty;
    private Action<WallInfo, Vector3> _drawAction;

    private IRaycastable _hitObject;

    private void _MakeActionOnWall()
    {
        if (_ctx == "LINE")
        {
            WallInfo hitWall = _wc.GetWallByName(PCref.Hit.collider.gameObject.name);
            if (hitWall == null)
            {
                return;
            }

            if (_drawAction == null)
            {
                Vector3 from = PCref.Hit.point;
                _drawAction = _items.AddLine(hitWall, from);
            }
            else
            {
                Vector3 to = PCref.Hit.point;
                _drawAction(hitWall, to);
            }
        }
    }

    public ModeExperimental(PlayerController pc)
    {
        PCref = pc;
        _wc = GameObject.Find("Walls").GetComponent<WallController>();

        _drawAction = null;
        _hitObject = null;

        _items = new ItemsController();
        _items.AddAxisBetweenPlanes(_wc.GetWallByName("Wall4"), _wc.GetWallByName("Wall3"));

        Debug.Log($"<color=blue> MODE experimental ON </color>");
    }

    public void HandleInput()
    {
        if (_ctx == "LABEL")
        {
            var hitObject = PCref.Hit.collider.GetComponent<IRaycastable>();

            if (_hitObject != hitObject)
            {
                _hitObject?.OnHoverExit();
                _hitObject = hitObject;
                _hitObject?.OnHoverEnter();
            }
        }
        else
        {
            _hitObject?.OnHoverExit();
            _hitObject = null;
        }


        if (Input.GetKeyDown("1"))
        {
            _ctx = string.Empty;
            _drawAction = null;
            Debug.Log("MODE ---");
        }

        if (Input.GetKeyDown("2"))
        {
            _ctx = "LINE";
            _drawAction = null;
            Debug.Log("MODE LINE");
        }

        if (Input.GetKeyDown("3"))
        {
            _ctx = "LABEL";
            _drawAction = null;
            Debug.Log("MODE LABEL");
        }

        if (Input.GetKey("5"))
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