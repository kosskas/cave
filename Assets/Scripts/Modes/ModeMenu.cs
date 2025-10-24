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
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_LEFT,
            () =>
            {
                radialMenu.PreviousOption();
            }
        );

        FlystickController.SetAction(
            FlystickController.Btn.JOYSTICK,
            FlystickController.ActOn.TILT_RIGHT,
            () =>
            {
                radialMenu.NextOption();
            }
        );

        FlystickController.SetAction(
            FlystickController.Btn._1,
            FlystickController.ActOn.PRESS,
            () =>
            {
                _context.Current.Value();            
            }
        );
    }
}

/*
 *  |           ACTION          |       DEV     |               LZWP                |
 *  | _MakeActionOnWall         |       5       |   Btn.FIRE ActOn.PRESS            |
 */