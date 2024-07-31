using UnityEngine;

public class ModeMenu : IMode
{
    public PlayerController PCref { get; private set; }

    private void _GoToMode3Dto2D()
    {
        PCref.ChangeMode(PlayerController.Mode.Mode3Dto2D);
    }

    private void _GoToMode2Dto3D()
    {
        PCref.ChangeMode(PlayerController.Mode.Mode2Dto3D);
    }

    public ModeMenu(PlayerController pc)
    {
        PCref = pc;

        SetUpFlystick();

        Debug.Log($"<color=blue> MODE menu ON </color>");
    }

    public void SetUpFlystick()
    {
        FlystickController.ClearActions();

        FlystickController.SetAction(
            FlystickController.Btn._1, 
            FlystickController.ActOn.PRESS, 
            () => _GoToMode3Dto2D()
        );

        FlystickController.SetAction(
            FlystickController.Btn._2, 
            FlystickController.ActOn.PRESS, 
            () => _GoToMode2Dto3D()
        );
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown("1"))
        {
            _GoToMode3Dto2D();
        }
        
        if (Input.GetKeyDown("2"))
        {
            _GoToMode2Dto3D();
        }
    }
}