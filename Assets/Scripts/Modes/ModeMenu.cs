using UnityEngine;

public class ModeMenu : IMode
{
    public PlayerController PCref { get; private set; }

    public ModeMenu(PlayerController pc)
    {
        PCref = pc;
        Debug.Log($"<color=blue> MODE menu ON </color>");
    }

    public void HandleInput()
    {

        if (Input.GetKeyDown("1"))
        {
            _LoadVisualization();
        }

        if (Input.GetKeyDown("2"))
        {
            _LoadReconstruction();
        }

    }

    private void _LoadVisualization()
    {
        PCref.ChangeMode(PlayerController.Mode.Mode3Dto2D);
    }

    private void _LoadReconstruction()
    {
        PCref.ChangeMode(PlayerController.Mode.ModeExperimental);
    }
}

/*
 *  |           ACTION          |       DEV     |               LZWP                |
 *  | _MakeActionOnWall         |       5       |   Btn.FIRE ActOn.PRESS            |
 */