using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct PointINFO
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }
    public string Label { get; }
    public string FullLabel { get; }
    public WallInfo WallInfo { get; }
    public GameObject GridPoint { get; }

    public static readonly PointINFO Empty = new PointINFO(null, null, "<?>", "<?>");

    public PointINFO(GameObject gridPoint, WallInfo wallInfo, string label, string fullLabel)
    {
        X = 0.0f;
        Y = 0.0f;
        Z = 0.0f;
        Label = label;
        FullLabel = fullLabel;
        WallInfo = wallInfo;
        GridPoint = gridPoint;

        if (gridPoint != null)
        {
            string name = gridPoint.name;
            int startIndex = name.IndexOf('(') + 1;
            int endIndex = name.IndexOf(')') - 1;
            int length = endIndex - startIndex + 1;

            char[] coordAxis = { name[5], name[6] };
            string[] coordValuesStr = name.Substring(startIndex, length).Split(',');
            float[] coordValues = { float.Parse(coordValuesStr[0], CultureInfo.InvariantCulture), float.Parse(coordValuesStr[1], CultureInfo.InvariantCulture) };

            switch (coordAxis[0])
            {
                case 'X':
                    X = coordValues[0];
                    break;
                case 'Y':
                    Y = coordValues[0];
                    break;
                case 'Z':
                    Z = coordValues[0];
                    break;
                default:
                    break;
            }

            switch (coordAxis[1])
            {
                case 'X':
                    X = coordValues[1];
                    break;
                case 'Y':
                    Y = coordValues[1];
                    break;
                case 'Z':
                    Z = coordValues[1];
                    break;
                default:
                    break;
            }
        }
    }

    public override string ToString() => $"{FullLabel} (X={X}, Y={Y}, Z={Z})";

    public static bool operator ==(PointINFO left, PointINFO right) => left.Equals(right);

    public static bool operator !=(PointINFO left, PointINFO right) => !left.Equals(right);
}