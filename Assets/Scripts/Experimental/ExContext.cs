using System.ComponentModel;

namespace Assets.Scripts.Experimental
{
    public enum ExContext
    {
        [Description("Idle")]
        Idle,

        [Description("Point")]
        Point,

        [Description("LineBetweenPoints")]
        LineBetweenPoints,

        [Description("Line")]
        Line,

        [Description("Circle")]
        Circle,

        [Description("Projection")]
        Projection
    }

}
