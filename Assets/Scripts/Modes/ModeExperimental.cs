using UnityEngine;

public class ModeExperimental : IMode
{
    public PlayerController PCref { get; private set; }

    private void _MakeActionOnWall()
    {

    }

    public ModeExperimental(PlayerController pc)
    {
        PCref = pc;

        Debug.Log($"<color=blue> MODE experimental ON </color>");
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown("5"))
        {
            _MakeActionOnWall();
        }
    }
}