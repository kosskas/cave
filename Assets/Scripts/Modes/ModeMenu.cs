using Assets.Scripts.Experimental;
using Assets.Scripts.Walls;
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

        ExContextMenuView.Hide();
        UIWall.ExportSolidToVisualButton.Hide();
        UIWall.SaveLoadStateButtons.Hide();
        UIWall.BackToMenuButton.Hide();

        UIWall.MenuButtons.Show();
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
                //_HideModesButtons();
                //PCref.ChangeMode(PlayerController.Mode.Mode2Dto3D);
                Debug.Log("Wycofany");
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
        UIWall.MenuButtons.Hide();
    }

    public void HandleInput()
    {

        if (Input.GetKeyDown("1"))
        {
            _HideModesButtons();
            PCref.ChangeMode(PlayerController.Mode.Mode3Dto2D);
        }
        if (Input.GetKeyDown("2"))
        {
            _HideModesButtons();
            PCref.ChangeMode(PlayerController.Mode.ModeExperimental);
        }

    }
}

/*
 *  |           ACTION          |       DEV     |               LZWP                |
 *  | _MakeActionOnWall         |       5       |   Btn.FIRE ActOn.PRESS            |
 */