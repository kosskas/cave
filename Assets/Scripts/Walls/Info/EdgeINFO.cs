using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EdgeINFO
{
    public PointINFO P1 { get; }
    public PointINFO P2 { get; }
    public GameObject EdgeObj { get; }
    public LineSegment Edge { get; }

    public static readonly EdgeINFO Empty = new EdgeINFO(null, null, PointINFO.Empty, PointINFO.Empty);

    public EdgeINFO(GameObject edgeObj, LineSegment edge, PointINFO p1, PointINFO p2)
    {
        P1 = p1;
        P2 = p2;
        EdgeObj = edgeObj;
        Edge = edge;
    }

    public override string ToString() => $"|{P1.FullLabel}{P2.FullLabel}|";

    public static bool operator ==(EdgeINFO left, EdgeINFO right) => left.Equals(right);

    public static bool operator !=(EdgeINFO left, EdgeINFO right) => !left.Equals(right);
}
