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
        private Vector3 Offset(Transform t) => 0.08f * FontSize * t.up;

        private TextMeshPro _textMeshPro;

        private GameObject _player;

        public string Text { get; set; } = "";

        public string UpperIndex { get; set; } = "";

        public string LowerIndex { get; set; } = "";

        public float FontSize { get; set; } = 1;


        void Start()
        {
            _player = GameObject.Find("FPSPlayer");

            GameObject textObj = new GameObject("LABEL");
            textObj.transform.SetParent(transform);

            _textMeshPro = textObj.AddComponent<TextMeshPro>();
            _textMeshPro.fontMaterial.shader = Shader.Find("TextMeshPro/Distance Field Overlay");
            _textMeshPro.color = Color.black;
            _textMeshPro.alignment = TextAlignmentOptions.Center;
        }

        void Update()
        {
            _textMeshPro.transform.position = gameObject.transform.position + Offset(gameObject.transform);
            _textMeshPro.fontSize = FontSize;
            _textMeshPro.text = String.IsNullOrWhiteSpace(Text) ? "" : $"{Text}<sup>{UpperIndex}</sup><sub>{LowerIndex}</sub>";

            Vector3 directionToPlayer = _player.transform.position - _textMeshPro.transform.position;
            _textMeshPro.transform.rotation = Quaternion.LookRotation(-directionToPlayer);
        }
    }
}
