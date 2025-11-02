using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrosshairGUI : MonoBehaviour {

    public Color color = new Color(1, 1, 1, 0.5f);
    public float size = 10f;

    void OnGUI()
    {
        float x = Screen.width / 2f;
        float y = Screen.height / 2f;
        Texture2D tex = Texture2D.whiteTexture;

        GUI.color = color;
        GUI.DrawTexture(new Rect(x - size / 2, y - 1, size, 2), tex);
        GUI.DrawTexture(new Rect(x - 1, y - size / 2, 2, size), tex);
    }
}
