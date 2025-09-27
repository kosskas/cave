using System.ComponentModel;

namespace Assets.Scripts.Experimental
{
    public enum ExContext
    {
        [Description("- - -")]
        Idle,

        [Description("Punkt")]
        Point,

        [Description("Kraw\u0119d\u017a")]
        BoldLine,

        [Description("Linia pomocznicza")]
        Line,

        [Description("Linia prostopad\u0142a")]
        PerpendicularLine,

        [Description("Linia r\u00F3wnoleg\u0142a")]
        ParallelLine,

        [Description("Okr\u0105g")]
        Circle,

        [Description("Linia odnosz\u0105ca")]
        Projection,

        [Description("Rzutnia")]
        Wall,

        [Description("\u015aciana")]
        Face
    }

}
