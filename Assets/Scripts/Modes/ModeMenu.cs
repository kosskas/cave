using UnityEngine;

public class ModeMenu : IMode
{
    public PlayerController PCref { get; private set; }

    public ModeMenu(PlayerController pc)
    {
        PCref = pc;
        Debug.Log($"<color=blue> MODE menu ON </color>");

        SetUpFlystick();
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

    public void SetUpFlystick()
    {
        FlystickController.ClearActions();

        FlystickController.SetAction(
            FlystickController.Btn._1,
            FlystickController.ActOn.PRESS,
            _LoadVisualization
        );

        FlystickController.SetAction(
            FlystickController.Btn._2,
            FlystickController.ActOn.PRESS,
            _LoadReconstruction
        );
    }
}

/*
 *  |           ACTION          |       DEV     |               LZWP                |
 *  | _MakeActionOnWall         |       5       |   Btn.FIRE ActOn.PRESS            |
 */