using UnityEngine;

public class ModeExperimental : IMode
{
    public PlayerController PCref { get; private set; }

    private WallController _wc;
    private ItemsController _items;
    //private Drawer _drawer;

    private void _MakeActionOnWall()
    {
        // if (_drawer.IsDrawing) {
        //     _drawer.StopDrawing(PCref.Hit.point);
        // } else {
        //     _drawer.StartDrawing(PCref.Hit.point);
        // }
    }

    public ModeExperimental(PlayerController pc)
    {
        PCref = pc;
        _wc = GameObject.Find("Walls").GetComponent<WallController>();

        _items = new ItemsController();
        

        Debug.Log($"<color=blue> MODE experimental ON </color>");
    }

    public void HandleInput()
    {
        //_drawer.Draw(PCref.Hit.point);

        if (Input.GetKeyDown("1"))
        {
            //_drawer.SetDrawingItem(Drawer.DrawItem.NOTHING);
        }

        if (Input.GetKeyDown("2"))
        {
            //_drawer.SetDrawingItem(Drawer.DrawItem.LINE_SEGMENT);
        }

        if (Input.GetKeyDown("5"))
        {
            _MakeActionOnWall();
        }
    }
}