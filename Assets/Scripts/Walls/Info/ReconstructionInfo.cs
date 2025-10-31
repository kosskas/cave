using UnityEngine;

public static class ReconstructionInfo
{
    // -- COLOR
    public static Color NORMAL = Color.black;
    public static Color MENTIONED = new Color(1f, 0.5f, 0f);
    public static Color FOCUSED = Color.white;
    // -- FACE
    public static Color FACE_COLOR = Color.white;
    public static float SOLID_FACE_TRANSPARENCY = 0.3f;
    // -- PROJECTION LINE
    public static Color PROJECTION_LINE_COLOR = Color.blue;
    public static Color PROJECTION_LINE_ERROR_COLOR = Color.red;
    public static float PROJECTION_LINE_WIDTH = 0.002f;
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
    // -- 2D CIRCLE
    public static float CIRCLE_2D_WIDTH = 0.003f;
    public static Color CIRCLE_2D_COLOR = Color.black;
    // -- 2D LINE
    public static float LINE_2D_WIDTH = 0.005f;
    public static Color LINE_2D_COLOR = Color.black;
    // -- 2D POINT
    public static float POINT_2D_SIZE = 0.025f;
    public static Color POINT_2D_COLOR = Color.black;
    // -- 2D LABEl
    public static float LABEL_2D_FONT_SIZE = 0.6f;
    public static float LABEL_2D_OFFSET = 0.08f;
    public static Color LABEL_2D_COLOR = Color.black;
}