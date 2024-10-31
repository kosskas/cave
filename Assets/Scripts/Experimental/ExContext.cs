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

        [Description("Okr\u0105g")]
        Circle,

        [Description("Rzut")]
        Projection
    }

}
