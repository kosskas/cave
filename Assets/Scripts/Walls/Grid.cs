using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Globalization;

public class Grid : MonoBehaviour {

    private enum Line {
        ROW, COL
    }

    private GameObject _gridObj;
    private Quaternion _gridObjRotation;
    private Vector3 _gridObjPosition;

    private float _lineWidth;
    private float _scale;

    public Grid(
        float height, 
        float length, 
        int rowsNumber, 
        int colsNumber, 
        string rowsAxis, 
        string colsAxis, 
        float lineWidth, 
        int scale, 
        GameObject parentObj,
        Vector3 position,
        Vector3 eulerAngles
    )
    {
        Quaternion rotation = Quaternion.identity;
        rotation.eulerAngles = eulerAngles;

        _gridObj = new GameObject($"grid_{rowsAxis}{colsAxis}");
        _gridObj.transform.SetParent(parentObj.transform);

        _gridObj.transform.rotation = rotation;
        _gridObjRotation = _gridObj.transform.rotation;

        _gridObj.transform.position = position;
        _gridObjPosition = _gridObj.transform.position;

        _lineWidth = lineWidth;
        _scale = scale;

        _CreateLines(rowsNumber, height, Line.ROW, rowsAxis);
        _CreateLines(colsNumber, length, Line.COL, colsAxis);

        _CreatePoints($"{rowsAxis}{colsAxis}", rowsNumber, height, colsNumber, length);

        //Point pp = gridObj.AddComponent<Point>();
        //pp.SetStyle(Color.red, 0.05f);     
    }

    private void _CreateLines(
        int linesNumber,
        float maxOffset,
        Line lineType,
        string axis
    ) 
    {
        for (int ithLine = 0; ithLine < linesNumber; ithLine++)
        {
            float lineOffset = ((float)ithLine / (float)(linesNumber-1)) * maxOffset;
            float lineValue = ithLine * _scale;

            Vector3 from = Vector3.zero;
            Vector3 to = Vector3.zero;
            switch (lineType)
            {
                case Line.ROW:
                    from = new Vector3(0f, lineOffset, 0f);
                    to = new Vector3(maxOffset, lineOffset, 0f);
                    break;
                case Line.COL:
                    from = new Vector3(lineOffset, 0f, 0f);
                    to = new Vector3(lineOffset, maxOffset, 0f);
                    break;
            }

            GameObject lineObj = new GameObject($"line_{axis}={lineValue}");
            lineObj.transform.SetParent(_gridObj.transform);
            lineObj.transform.rotation = _gridObjRotation;
            lineObj.transform.position = _gridObjPosition + (_gridObjRotation * from);
            lineObj.tag = "GridLine";

            LineSegment line = lineObj.AddComponent<LineSegment>();
            line.SetCoordinates(
                _gridObjPosition + (_gridObjRotation * from), 
                _gridObjPosition + (_gridObjRotation * to)
            );
            if (ithLine == 0)
            {
                line.SetStyle(Color.black, _lineWidth*2);
            }
            else
            {
                line.SetStyle(Color.grey, _lineWidth);
            }

            GameObject axisObj = new GameObject();
            axisObj.transform.SetParent(lineObj.transform);
            axisObj.transform.position = lineObj.transform.position;
            Point p = axisObj.AddComponent<Point>();
            p.SetStyle(Color.black, _lineWidth*2);
            if (ithLine == linesNumber-1)
            {
                axisObj.name = $"axis_name_{axis}";
                p.SetLabel(axis, 0.06f, Color.black);
            }
            else
            {
                axisObj.name = $"axis_point_{axis}={lineValue}";
                //p.SetLabel($"{lineValue}", 0.03f, Color.white);
            }
        }
    }

    private void _CreatePoints(string planeRowColName, int rowsNumber, float rowsMaxOffset, int colsNumber, float colsMaxOffset)
    {
        Vector3 pointRadius = new Vector3(0.1f, 0.1f, 0.1f);

        float rowsPointSize = 0.8f * _GetOffsetBetweenPoints(rowsNumber, rowsMaxOffset);
        float colsPointSize = 0.8f * _GetOffsetBetweenPoints(colsNumber, colsMaxOffset);

        for (int ithRow = 1; ithRow < rowsNumber; ithRow++)
        {
            float rowOffset = ((float)ithRow / (float)(rowsNumber-1)) * rowsMaxOffset;
            float rowValue = ithRow * _scale;

            for (int ithCol = 1; ithCol < colsNumber; ithCol++)
            {
                float colOffset = ((float)ithCol / (float)(colsNumber-1)) * colsMaxOffset;
                float colValue = ithCol * _scale;

                {
                    Vector3 pointPos = new Vector3(colOffset, rowOffset, 0f);

                    GameObject pointObj = new GameObject($"point{planeRowColName}=({rowValue},{colValue})");
                    pointObj.transform.SetParent(_gridObj.transform);
                    pointObj.transform.rotation = _gridObjRotation;
                    pointObj.transform.position = _gridObjPosition + (_gridObjRotation * pointPos);
                    pointObj.tag = "GridPoint";

                    BoxCollider boxCollider = pointObj.AddComponent<BoxCollider>();
                    boxCollider.size = new Vector3(colsPointSize, rowsPointSize, 0f);
                    boxCollider.isTrigger = true;

                    pointObj.layer = LayerMask.NameToLayer("GridPoint");
                }
            }
        }
    }

    private float _GetOffsetBetweenPoints(int pointsNumber, float maxOffset)
    {
        return maxOffset / (float)pointsNumber;
    }
}
