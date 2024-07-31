using UnityEngine;

public class ModeMenu : IMode
{
    public PlayerController PCref { get; private set; }

    

    public ModeMenu(PlayerController pc)
    {
        PCref = pc;

        SetUpFlystick();

        Debug.Log($"<color=blue> MODE menu ON </color>");

        PointsList.HideListAndLogs();
        //PointsList.ceilingWall.SetActive(false);
    }


    private void _MakeActionOnWall()
    {
        if (PCref.Hit.collider != null)
        {
            Debug.Log($"[CLICK] on object named: {PCref.Hit.collider.gameObject.name}");
            if (PCref.Hit.collider.gameObject.name == "WizButton")
            {
                _HideModesButtons();
                PCref.ChangeMode(PlayerController.Mode.Mode3Dto2D);
            }
            else if (PCref.Hit.collider.gameObject.name == "KreaButton")
            {
                _HideModesButtons();
                PCref.ChangeMode(PlayerController.Mode.Mode2Dto3D);
            }
            
        }
    }

    private void _HideModesButtons()
    {
        GameObject wizButton = GameObject.Find("WizButton");
        GameObject kreaButton = GameObject.Find("KreaButton");

        if (wizButton != null)
        {
            wizButton.SetActive(false);
        }
        if (kreaButton != null)
        {
            kreaButton.SetActive(false);
        }
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
        //if (Input.GetKeyDown("1"))
        //{
        //    PCref.ChangeMode(PlayerController.Mode.Mode3Dto2D);
        //}

        //if (Input.GetKeyDown("2"))
        //{
        //    PCref.ChangeMode(PlayerController.Mode.Mode2Dto3D);
        //}
        if (Input.GetKeyDown("5"))
        {
            _MakeActionOnWall();
        }

        if (Input.GetKeyDown("3"))
        {
            _HideModesButtons();
            PCref.ChangeMode(PlayerController.Mode.ModeExperimental);
        }
    }
}