using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Experimental.Items;
using Assets.Scripts.Experimental.Utils;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public class IndexedLabel : MonoBehaviour
    {
        private Vector3 Offset(Transform t) => 0.08f * FontSize * t.up;

        private const float LabelFontSize = 0.6f;

        private static readonly Color ColorNormal = ReconstructionInfo.NORMAL;

        private TextMeshPro _textMeshPro;

        private GameObject _player;

        // ------

        private CircularIterator<AtomicLabel> _labels = null;

        private string ParseLabels()
        {
            if (_labels == null)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var label in _labels.All())
            {
                if (string.IsNullOrWhiteSpace(label.Text))
                    continue;

                if (sb.Length > 0)
                    sb.Append(" = ");

                if (label == _labels.Current)
                    sb.Append($"<color=#{ColorUtility.ToHtmlStringRGB(FocusedLabelColor)}>");

                sb.Append($"{label.Text}<sup>{label.UpperIndex}</sup><sub>{label.LowerIndex}</sub>");

                if (label == _labels.Current)
                    sb.Append("</color>");
            }
            
            return sb.ToString();
        }

        // ---

        public Color FocusedLabelColor { get; set; } = ReconstructionInfo.FOCUSED;

        public List<AtomicLabel> Labels => _labels.All().ToList();

        public AtomicLabel FocusedLabel => _labels.Current;

        public void AddLabel(string text, string upperIndex, string lowerIndex)
        {
            if (_labels == null)
                _labels = new CircularIterator<AtomicLabel>(new List<AtomicLabel>());

            _labels.Push(new AtomicLabel(text, upperIndex, lowerIndex));
        }

        public void RemoveFocusedLabel()
        {
            _labels.RemoveCurrent();
        }

        public void NextLabel()
        {
            _labels.Next();
        }

        public void PrevLabel()
        {
            _labels.Previous();
        }

        // -------

        public float FontSize { get; set; } = LabelFontSize;


        void Start()
        {
            _player = GameObject.Find("FPSPlayer");

            GameObject textObj = new GameObject("LABEL");
            textObj.transform.SetParent(transform);

            _textMeshPro = textObj.AddComponent<TextMeshPro>();
            _textMeshPro.fontMaterial.shader = Shader.Find("TextMeshPro/Distance Field Overlay");
            _textMeshPro.color = ColorNormal;
            _textMeshPro.alignment = TextAlignmentOptions.Center;
        }

        void Update()
        {
            _textMeshPro.transform.position = gameObject.transform.position + Offset(gameObject.transform);
            _textMeshPro.fontSize = FontSize;
            _textMeshPro.text = ParseLabels();

            Vector3 directionToPlayer = _player.transform.position - _textMeshPro.transform.position;
            _textMeshPro.transform.rotation = Quaternion.LookRotation(-directionToPlayer);
        }
    }
}
