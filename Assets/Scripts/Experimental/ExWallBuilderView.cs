using TMPro;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public class ExWallBuilderView
    {
        private readonly TextMeshPro _banner;

        public ExWallBuilderView()
        {
            _banner = GameObject.Find("ExperimentalModeWallBuilderView")?.GetComponent<TextMeshPro>();

            if (_banner == null)
                return;

            _banner.fontMaterial.shader = Shader.Find("TextMeshPro/Distance Field Overlay");

            _banner.text = string.Empty;
        }

        public void AppendToList(string point)
        {
            if (_banner == null)
                return;

            if (_banner.text.Length > 0)
                _banner.text += " -> ";

            _banner.text += point;
        }

        public void ClearList()
        {
            _banner.text = string.Empty;
        }
    }
}