using UnityEngine;

public struct GridINFO
{
    public float Height { get; }
    public float Length { get; }
    public int RowsNumber { get; }
    public int ColsNumber { get; }
    public string RowsAxis { get; }
    public string ColsAxis { get; }
    public float LineWidth { get; }
    public int Scale { get; }
    public Vector3 Position { get; }
    public Vector3 EulerAngles { get; }

    public GridINFO(
        float height, 
        float length, 
        int rowsNumber, 
        int colsNumber, 
        string rowsAxis, 
        string colsAxis, 
        float lineWidth, 
        int scale, 
        Vector3 position,
        Vector3 eulerAngles
    )
    {
        Height = height;
        Length = length;
        RowsNumber = rowsNumber;
        ColsNumber = colsNumber;
        RowsAxis = rowsAxis;
        ColsAxis = colsAxis;
        LineWidth = lineWidth;
        Scale = scale;
        Position = position;
        EulerAngles = eulerAngles;
    }

    public override string ToString() => $"Grid {RowsAxis}{ColsAxis}";
}