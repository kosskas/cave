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
    public class IndexedLabel : MonoBehaviour, IDrawable
    {
        private readonly Vector3 _OFFSET = 0.06f * Vector3.up;


        private string _text = "";
        private string _upperIndex = "";
        private string _lowerIndex = "";

        private TextMeshPro _textMeshPro;

        private Vector3 _pos;


        private void SetLabel()
        {
            _textMeshPro.text = $"{_text}<sup>{_upperIndex}</sup><sub>{_lowerIndex}</sub>";
        }


        void Awake()
        {
            GameObject textObj = new GameObject("LABEL");
            textObj.transform.SetParent(transform);
            textObj.transform.Rotate(Vector3.up, 90f);

            _textMeshPro = textObj.AddComponent<TextMeshPro>();
            _textMeshPro.fontSize = 1;
            _textMeshPro.color = Color.red;
            _textMeshPro.alignment = TextAlignmentOptions.Center;
        }


        public void Draw(params Vector3[] positions)
        {
            _pos = positions[0] + _OFFSET;

            _textMeshPro.transform.position = _pos;
        }

        public void Erase()
        {
            throw new NotImplementedException();
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
                SetLabel();
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
                SetLabel();
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
                SetLabel();
            }
        }
    }
}
