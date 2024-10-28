using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Experimental.Items;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Experimental
{
    public class IndexedLabel : MonoBehaviour
    {
        private Vector3 Offset(Transform t) => 0.08f * _fontSize * t.up + 0.05f * t.right;


        private string _text = "";
        private string _upperIndex = "";
        private string _lowerIndex = "";

        private float _fontSize = 1;

        private TextMeshPro _textMeshPro;

        private Vector3 _pos;


        void Start()
        {
            GameObject textObj = new GameObject("LABEL");
            textObj.transform.SetParent(transform);

            _textMeshPro = textObj.AddComponent<TextMeshPro>();
            _textMeshPro.color = Color.black;
            _textMeshPro.alignment = TextAlignmentOptions.Center;
        }

        void Update()
        {
            _textMeshPro.transform.position = gameObject.transform.position + Offset(gameObject.transform);
            _textMeshPro.fontSize = _fontSize;
            _textMeshPro.text = String.IsNullOrWhiteSpace(_text) ? "" : $"{_text}<sup>{_upperIndex}</sup><sub>{_lowerIndex}</sub>";
        }


        // Properties
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                _text = value;
            }
        }

        public string UpperIndex
        {
            get
            {
                return _upperIndex;
            }

            set
            {
                _upperIndex = value;
            }
        }

        public string LowerIndex
        {
            get
            {
                return _lowerIndex;
            }

            set
            {
                _lowerIndex = value;
            }
        }

        public float FontSize
        {
            get
            {
                return _fontSize;
            }

            set
            {
                _fontSize = value;
            }
        }
    }
}
