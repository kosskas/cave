
using UnityEngine;

public class Mode2Dto3D : IMode
{
    private WallController _wc;
    private PointPlacer _pp;
    private GridCreator _gc;

    public PlayerController PCref { get; private set; }

    private void _AddPointProjection()
    {}

    private void _RemovePointProjection()
    {}

    private void _AddEdgeProjection()
    {}

    private void _RemoveEdgeProjection()
    {}

    private void _MakeActionOnWall()
    {}

    private void _SaveSolidAndSwitchToMode3Dto2D()
    {}

    private void _ChoosePreviousLabel()
    {}

    private void _ChooseNextLabel()
    {}

    public Mode2Dto3D(PlayerController pc)
    {
        PCref = pc;
        GameObject wallsObject = GameObject.Find("Walls");
        _wc = wallsObject.GetComponent<WallController>();

        _gc = new GridCreator();

        _pp = PCref.gameObject.GetComponent<PointPlacer>();
        _pp.CreatePoint();
        _pp.MovePointPrototype(PCref.Hit);

        Debug.Log($"<color=blue> MODE inzynierka ON </color>");
    }

    public void HandleInput()
    {
        _pp.MovePointPrototype(PCref.Hit);

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
    }

}

        // if (Input.GetKeyDown("o"))
        // {
        //     pp.CreateLabel(hit, $"{alpha[labelIdx]}");
        // }
        // if (Input.GetKeyDown("p"))
        // {
        //     pp.RemoveLabel(hit, $"{alpha[labelIdx]}");
        // }
        // if (Input.GetKeyDown("4"))
        // {
        //     labelIdx = (labelIdx - 1 < 0 ? alpha.Length - 1 : labelIdx - 1);
        //     Debug.Log($"Current label {alpha[labelIdx]}");
        // }
        // if (Input.GetKeyDown("5"))
        // {
        //     labelIdx = (labelIdx+1)% alpha.Length;
        //     Debug.Log($"Current label {alpha[labelIdx]}");
        // }