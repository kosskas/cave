using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts.Experimental.Items
{
    public class Point : MonoBehaviour, IDrawable, IRaycastable, ILabelable
    {
        private static readonly Color ColorNormal = Color.black;

        private static readonly Color ColorFocused = Color.red;

        private static readonly float Size = 0.025f;

        private static readonly char[] Labels = " ABCDEFGHIJKLMNOPRQSTUVWXYZ".ToCharArray();
        private int _labelIdx = 0;


        private Vector3 _pos;

        private GameObject _pointObject;

        private Renderer _pointRenderer;

        private SphereCollider _sphereCollider;
        private BoxCollider _boxCollider;

        void Start()
        {
            _pointObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _pointObject.transform.SetParent(gameObject.transform);
            _pointObject.transform.localScale = new Vector3(Size, Size, Size);

            _pointRenderer = _pointObject.GetComponent<Renderer>();
            _pointRenderer.material = new Material(Shader.Find("Unlit/Color"))
            {
                color = ColorNormal
            };
            _pointRenderer.shadowCastingMode = ShadowCastingMode.Off;

            Destroy(_pointObject.GetComponent<SphereCollider>());

            _boxCollider = gameObject.AddComponent<BoxCollider>();
            _boxCollider.size = new Vector3(Size, Size, Size);
            _boxCollider.center = Vector3.zero;
            _boxCollider.isTrigger = true;
        }

        void Update()
        {
            gameObject.transform.position = _pos;
        }


        public void Draw(params Vector3[] positions)
        {
            _pos = (positions.ElementAtOrDefault(0) == default(Vector3)) ? _pos : positions[0];
        }

        public void Erase()
        {
            throw new NotImplementedException();
        }

        public void OnHoverAction(Action<GameObject> action)
        {
            action(gameObject);
        }

        public void OnHoverEnter()
        {
            _pointRenderer.material.color = ColorFocused;
        }

        public void OnHoverExit()
        {
            _pointRenderer.material.color = ColorNormal;
        }

        public void NextLabel()
        {
            _labelIdx = (_labelIdx + 1 == Labels.Length) ? 0 : _labelIdx + 1;

            var label = gameObject.GetComponent<IndexedLabel>();
            if (label != null)
            {
                label.Text = Labels[_labelIdx].ToString();
            }
        }

        public void PrevLabel()
        {
            _labelIdx = (_labelIdx == 0) ? Labels.Length - 1 : _labelIdx - 1;

            var label = gameObject.GetComponent<IndexedLabel>();
            if (label != null)
            {
                label.Text = Labels[_labelIdx].ToString();
            }
        }
    }
}
