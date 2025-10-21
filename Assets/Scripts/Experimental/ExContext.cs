using System.ComponentModel;

namespace Assets.Scripts.Experimental
{
    public enum ExContext
    {
        [Description("Zapisz stan")]
        Save,

        [Description("Wczytaj stan")]
        Load,

        [Description("Wizualizuj")]
        LoadVisual,

        [Description("Konstruuj")]
        Const,

        [Description("<Wr\u00F3\u0107>")]
        BackToOpt,

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
        Face,

        [Description("Linie rzutuj\u0105ce")]
        ProjLine,

        [Description("Cofnij")]
        Undo,

        [Description("Pon\u00F3w")]
        Redo,

        //---------WIZUALIZACJA-------------------

        [Description("Nastepna bryla")]
        NextSolid,

        [Description("Poprzednia bryla")]
        PrevSolid,

        [Description("Usun sciane")]
        RemoveWall,

        [Description("Dodaj sciane")]
        AddWall,

        [Description("Pokaz rzut")]
        ShowProj,
    }
}
