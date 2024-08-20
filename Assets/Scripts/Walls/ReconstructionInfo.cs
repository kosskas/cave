using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ReconstructionInfo
{
    // -- CURSOR
    public static float _CURSOR_SIZE = 0.05f;
    public static Color _CURSOR_COLOR = new Color(1, 1, 1, 0.3f);
    public static Color _CURSOR_COLOR_FOCUSED = new Color(1, 0, 0, 1f);
    // -- POINT ON WALL
    public static float POINT_SIZE = 0.025f;
    public static Color POINT_COLOR = Color.black;
    // -- EDGE ON WALL
    public static float EDGE_LINE_WIDTH = 0.01f;
    public static Color EDGE_COLOR = Color.white;
    // -- LABEL ON WALL
    public static float LABEL_SIZE_PLACED = 0.04f;
    public static float LABEL_SIZE_PICKED = 0.06f;
    public static float LABEL_OFFSET_FROM_POINT = 0.03f;
    public static Color LABEL_COLOR_PLACED = Color.white;
    public static Color LABEL_COLOR_CHOSEN = Color.red;
    public static Color LABEL_COLOR_PICKED_FOCUSED = Color.green;
    public static Color LABEL_COLOR_PICKED_UNFOCUSED = new Color(0, 0.8f, 0);
    // -- PROJECTION LINE
    public static Color PROJECTION_LINE_COLOR = Color.blue;
    public static float PROJECTION_LINE_WIDTH = 0.002f;
    // -- REFERENCE LINE
    public static Color REFERENCE_LINE_COLOR = Color.green;
    public static float REFERENCE_LINE_WIDTH = 0.005f;
    // -- 3D POINT
    public static float POINT_3D_DIAMETER = 0.015f;
    public static Color POINT_3D_COLOR = Color.black;
    // -- 3D EDGE
    public static float EDGE_3D_LINE_WIDTH = 0.01f;
    public static Color EDGE_3D_COLOR = Color.black;
    // -- 3D LABEl
    public static float LABEL_3D_SIZE = 0.04f;
    public static Color LABEL_3D_COLOR = Color.white;
    public static Color LABEL_3D_ERR_COLOR = Color.red;
}