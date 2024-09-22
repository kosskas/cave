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
            PCref.ChangeMode(PlayerController.Mode.Mode3Dto2D);
        }
        
        if (Input.GetKeyDown("2"))
        {
            PCref.ChangeMode(PlayerController.Mode.Mode2Dto3D);
        }

        if (Input.GetKeyDown("3"))
        {
            PCref.ChangeMode(PlayerController.Mode.ModeExperimental);
        }
    }
}