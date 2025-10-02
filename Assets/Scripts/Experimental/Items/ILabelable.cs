using System.Collections.Generic;

namespace Assets.Scripts.Experimental
{
    public interface ILabelable
    {
        string FocusedLabel { get; set; }

        List<string> Labels { get; }

        void AddLabel();

        void RemoveFocusedLabel();

        void NextLabel();

        void PrevLabel();

        void NextText();

        void PrevText();

        bool EnabledLabels { get; set; }
    }
}
