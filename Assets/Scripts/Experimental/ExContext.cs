using System.ComponentModel;

namespace Assets.Scripts.Experimental
{
    public enum ExContext
    {
        [Description("- - -")]
        Idle,

        [Description("Punkt")]
        Point,

        [Description("Gruba linia")]
        BoldLine,

        [Description("Linia")]
        Line,

        [Description("Linia prostopad\u0142a")]
        PerpendicularLine,

        [Description("Linia r\u00F3wnoleg\u0142a")]
        ParallelLine,

        [Description("Okr\u0105g")]
        Circle,

        [Description("Rzut")]
        Projection,

        [Description("\u015bciana")]
        Wall
    }

}
