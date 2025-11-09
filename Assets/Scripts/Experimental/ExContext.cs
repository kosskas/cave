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

        [Description("Wro\u0107 do menu")]
        BackToMenu,

        [Description("Konstruuj")]
        Const,

        [Description("<Wr\u00F3\u0107>")]
        BackToOpt,

        [Description("Punkt")]
        Point,

        [Description("Kraw\u0119d\u017a")]
        BoldLine,

        [Description("Linia")]
        Line,

        [Description("Linia pomocznicza")]
        HelpLine,

        [Description("Linia prostopad\u0142a")]
        PerpendicularLine,

        [Description("Linia r\u00F3wnoleg\u0142a")]
        ParallelLine,

        [Description("Linia odnosz\u0105ca")]
        Projection,

        [Description("Linia odnosz\u0105ca wskazanej d\u0142ugo\u015bci")]
        FixedProjection,

        [Description("Okr\u0105g")]
        Circle,

        [Description("Rzutnia")]
        Wall,

        [Description("\u015aciana")]
        Face,

        [Description("P\u0142aszczyzna pomocnicza")]
        HelpPlane,

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

        //MENU
        [Description("Wyjdz")]
        ExitApp,
    }
}
