using System.ComponentModel;

namespace Assets.Scripts.Experimental
{
    public enum ExContext
    {
        [Description("- - -")]
        Idle,

        [Description("Punkt")]
        Point,

        [Description("Odcinek")]
        LineBetweenPoints,

        [Description("Linia")]
        Line,

        [Description("Linia prostopad\u0142a")]
        PerpendicularLine,

        [Description("Okr\u0105g")]
        Circle,

        [Description("Rzut")]
        Projection
    }

}
