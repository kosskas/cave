using UnityEngine;

public class ModeMenu : IMode
{
    public PlayerController PCref { get; private set; }

    

    public ModeMenu(PlayerController pc)
    {
        PCref = pc;
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
            else if (PCref.Hit.collider.gameObject.name == "ExpButton")
            {
                _HideModesButtons();
                PCref.ChangeMode(PlayerController.Mode.ModeExperimental);
            }

        }
    }

    private void _HideModesButtons()
    {
        GameObject wizButton = GameObject.Find("WizButton");
        GameObject kreaButton = GameObject.Find("KreaButton");
        GameObject expButton = GameObject.Find("ExpButton");

        if (wizButton != null)
        {
            wizButton.SetActive(false);
        }
        if (kreaButton != null)
        {
            kreaButton.SetActive(false);
        }
        if (expButton != null)
        {
            expButton.SetActive(false);
        }
    }

    public void HandleInput()
    {

        if (Input.GetKeyDown("5"))
        {
            _MakeActionOnWall();
        }

    }
}

/*
 *  |           ACTION          |       DEV     |               LZWP                |
 *  | _MakeActionOnWall         |       5       |   Btn.FIRE ActOn.PRESS            |
 */