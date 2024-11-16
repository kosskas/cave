namespace Assets.Scripts.Experimental
{
    public class AtomicLabel
    {
        public string Text { get; set; }

        public string UpperIndex { get; set; }

        public string LowerIndex { get; set; }

        public AtomicLabel(string text = "", string upperIndex = "", string lowerIndex = "")
        {
            Text = text;
            UpperIndex = upperIndex;
            LowerIndex = lowerIndex;
        }
    }
}